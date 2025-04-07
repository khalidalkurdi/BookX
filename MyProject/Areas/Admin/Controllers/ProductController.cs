using DataAccess.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Models.ViewModels;
using MyProject.Models;
using Utility;

namespace MyProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitofwork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(IUnitOfWork unitOfWork , IWebHostEnvironment webHostEnvironment )
        {
           _unitofwork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            List<Product> list = _unitofwork.Product.GetAll(includeProperties: "Category").ToList();
            return View(list);
        } 
        public IActionResult UpSert(int? id)
        {

            ProductVM productVM = new ProductVM()
            {
                CategoryList = _unitofwork.Category.GetAll().Select(
                u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                }),
                product = new Product()
            };
            
            if (id == null || id == 0)
            {//create
                return View(productVM);
            }
            else
            {//update
                productVM.product = _unitofwork.Product.Get(u => u.Id == id);
                return View(productVM);
            }
        }
        [HttpPost]
        public IActionResult UpSert(ProductVM productVM,IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                string wwwRoot = _webHostEnvironment.WebRootPath;
                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString()+Path.GetExtension(file.FileName);
                    string productPath = Path.Combine(wwwRoot, @"images\product");
                    //case -- update --
                    if (!string.IsNullOrEmpty(productVM.product.ImageUrl))
                    {//delet old image
                        var oldimagepath = Path.Combine(wwwRoot, productVM.product.ImageUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(oldimagepath))
                        {                           
                            System.IO.File.Delete(oldimagepath);
                        }
                    }
                    using (var fileStream = new FileStream(Path.Combine(productPath, fileName),FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }
                    productVM.product.ImageUrl = @"\images\product\" + fileName;
                }//add
                 

                if (productVM.product.Id !=null && productVM.product.Id != 0)
                { 
                    _unitofwork.Product.Update(productVM.product);
                    TempData["update"] = "Updated Successfuly. !";
                }
                else
                {
                    _unitofwork.Product.Add(productVM.product);
                    TempData["success"] = "Done Created. !";
                }
                _unitofwork.Save();
                return RedirectToAction("Index");
            }
            return View(productVM);
        }        
        #region Api Call 
        [HttpGet]
        public IActionResult GetAll()
        {
            List<Product> list = _unitofwork.Product.GetAll(includeProperties: "Category").ToList();
            return Json(new { data = list});
        }
        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var productToBeDelete = _unitofwork.Product.Get(x => x.Id == id);
            if (productToBeDelete == null)
            {
                return Json(new {success= false, message="Error while delete a prouct !"});
            }
            var oldImage = Path.Combine(_webHostEnvironment.WebRootPath, productToBeDelete.ImageUrl.TrimStart('\\'));
            if (System.IO.Path.Exists(oldImage))
            {
                System.IO.File.Delete(oldImage);
            }
            _unitofwork.Product.Remove(productToBeDelete);
            _unitofwork.Save();
            return Json(new { success = true, message = "Delete Successfuly!" });
        }
        #endregion
    }//end controller
}
