
using DataAccess.Repository;
using DataAccess.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyProject.Models;
using Utility;




namespace MyProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles =SD.Role_Admin)]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitofwork;
        public CategoryController(IUnitOfWork unitOfWork)
        {
            _unitofwork = unitOfWork;
        }
        public IActionResult Index()
        {
            List<Category> list = _unitofwork.Category.GetAll().ToList();
            return View(list);
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(Category obj)
        {

            if (ModelState.IsValid)
            {
                _unitofwork.Category.Add(obj);
                _unitofwork.Save();
                TempData["success"] = "Done Created !";
                return RedirectToAction("Index");
            }
            return View();
        }
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var Categoryfromdb = _unitofwork.Category.Get(x => x.Id == id);
            if (Categoryfromdb == null)
            {
                return NotFound();
            }
            return View(Categoryfromdb);
        }
        [HttpPost]
        public IActionResult Edit(Category obj)
        {
            if (ModelState.IsValid)
            {
                _unitofwork.Category.Update(obj);
                _unitofwork.Save();
                TempData["success"] = "Done Updated !";
                return RedirectToAction("Index");
            }
            return View();
        }
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var Categoryfromdb = _unitofwork.Category.Get(x => x.Id == id);
            if (Categoryfromdb == null)
            {
                return NotFound();
            }
            return View(Categoryfromdb);
        }
        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePost(int? id)
        {
            var exist = _unitofwork.Category.Get(x => x.Id == id);
            if (exist == null)
            {
                return NotFound(id);
            }

            _unitofwork.Category.Remove(exist);
            _unitofwork.Save();
            TempData["delete"] = "Done Deleted !";
            return RedirectToAction("Index");
        }

    }//end controller
}
