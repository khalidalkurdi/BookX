using DataAccess.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Models;
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
           
            return View();
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
                productVM.product = _unitofwork.Product.Get(u => u.Id == id, includeProperties:"ProductImages");
                return View(productVM);
            }
        }
        [HttpPost]
        public IActionResult UpSert(ProductVM productVM,List<IFormFile>? files)
        {
            if (ModelState.IsValid)
            {
                string wwwRoot = _webHostEnvironment.WebRootPath;
                //case -- update --    
                if (productVM.product.Id !=null && productVM.product.Id != 0)
                { 
                    _unitofwork.Product.Update(productVM.product);
                    if (productVM.product.ProductImages != null)
                    {
                        foreach (var image in productVM.product.ProductImages)
                        {
                            //delet old image
                            var oldimagepath = Path.Combine(wwwRoot, image.ImageUrl.TrimStart('\\'));
                            if (System.IO.File.Exists(oldimagepath))
                            {                           
                                System.IO.File.Delete(oldimagepath);
                            }

                        }                        
                    }
                    TempData["update"] = "Updated Successfuly. !";
                }
                else
                {// create new product
                    _unitofwork.Product.Add(productVM.product);
                    TempData["success"] = "Done Created. !";
                }
                _unitofwork.Save();
                
                if (files != null)
                {
                    //add images
                    foreach (IFormFile file in files)
                    {
                        string fileName = Guid.NewGuid().ToString()+Path.GetExtension(file.FileName);
                        string productPath = @"images\products\product-"+productVM.product.Id;
                        string finalPath = Path.Combine(wwwRoot, productPath);

                        if (!Directory.Exists(finalPath))
                        {
                            Directory.CreateDirectory(finalPath);
                        }
                        using (var fileStream = new FileStream(Path.Combine(finalPath, fileName),FileMode.Create))
                        {
                            file.CopyTo(fileStream);
                        }
                        var productImage = new ProductImage
                        {
                            ImageUrl = @"\" + productPath + @"\" + fileName,
                            ProductId= productVM.product.Id,
                        };

                        if (productVM.product.ProductImages==null)
                        {
                            productVM.product.ProductImages=new List<ProductImage>();
                        }
                        productVM.product.ProductImages.Add(productImage);
                    }
                    _unitofwork.Product.Update(productVM.product);
                    _unitofwork.Save();
                }                  
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
            var oldDirectoryPath = Path.Combine(_webHostEnvironment.WebRootPath, @"images\products\product-"+id);
            if (Directory.Exists(oldDirectoryPath))
            {
                Directory.Delete(oldDirectoryPath,true);
            }
            _unitofwork.Product.Remove(productToBeDelete);
            _unitofwork.Save();
            return Json(new { success = true, message = "Delete Successfuly!" });
        }
        public IActionResult DeleteImage(int ImageId)
        {
            var ImageToBeDelete = _unitofwork.ProductImage.Get(x => x.Id == ImageId);
            if (ImageToBeDelete != null)
            {
                if(!string.IsNullOrEmpty(ImageToBeDelete.ImageUrl))
                {
                    var oldImage = Path.Combine(_webHostEnvironment.WebRootPath, ImageToBeDelete.ImageUrl.TrimStart('\\'));
                    if (System.IO.Path.Exists(oldImage))
                    {
                        System.IO.File.Delete(oldImage);
                    }
                    _unitofwork.ProductImage.Remove(ImageToBeDelete);
                    _unitofwork.Save();
                    TempData["delete"] = " The Image deleted Successfuly. !";
                }
            }            
            return RedirectToAction(nameof(UpSert), new { id = ImageToBeDelete.ProductId});
        }
        #endregion
    }//end controller
}
