using DataAccess.UnitOfWork;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Models.ViewModels;
using MyProject.Models;
using NuGet.Packaging.Signing;
using System.Xml.Serialization;


namespace MyProject.Areas.Admin.Controllers
{
    [Area("Admin")]
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
            List<Product> list = _unitofwork.Product.GetAll("Category").ToList();
            return View(list);
        } 
        public IActionResult UpSert(int? id)
        {
            
            ProductVM productVM = new ProductVM() {
                CategoryList = _unitofwork.Category.GetAll().Select(
                u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                }),
                product =new Product()
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
                    string productPath = Path.Combine(wwwRoot, @"images\product");
                    string fileName = Guid.NewGuid().ToString()+Path.GetExtension(file.FileName);
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
                 

                if (productVM.product.Id !=null)
                { _unitofwork.Product.Update(productVM.product);
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
        
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var productfromdb = _unitofwork.Product.Get(x => x.Id == id);
            if (productfromdb == null)
            {
                return NotFound();
            }
            return View(productfromdb);
        }
        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePost(int? id)
        {
            var exist = _unitofwork.Product.Get(x => x.Id == id);
            if (exist == null)
            {
                return NotFound(id);
            }

            _unitofwork.Product.Remove(exist);
            _unitofwork.Save();
            TempData["delete"] = "Done Deleted. !";
            return RedirectToAction("Index");
        }
    }//end controller
}
