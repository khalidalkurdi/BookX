using DataAccess.UnitOfWork;
using Microsoft.AspNetCore.Mvc;
using Models;

namespace MyProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Authorize(Roles = SD.Role_Admin)]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitofwork;
        public CompanyController(IUnitOfWork unitOfWork)
        {
            _unitofwork = unitOfWork;
            
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult UpSert(int? id)
        {
            if (id == null || id == 0)
            {//create
                return View(new Company());
            }
            else
            {//update
               var Company = _unitofwork.Company.Get(u => u.Id == id);
                return View(Company);
            }
        }
        [HttpPost]
        public IActionResult UpSert(Company Company)
        {
            if (ModelState.IsValid)
            {
                if (Company.Id != 0)
                {
                    _unitofwork.Company.Update(Company);
                    TempData["update"] = "Updated Successfuly. !";
                }
                else
                {
                    _unitofwork.Company.Add(Company);
                    TempData["success"] = "Done Created. !";
                }
                _unitofwork.Save();
                return RedirectToAction("Index");
            }
            return View(Company);
        }
        #region Api Call 
        [HttpGet]
        public IActionResult GetAll()
        {
            List<Company> list = _unitofwork.Company.GetAll().ToList();
            return Json(new { data = list });
        }
        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var CompanyToBeDelete = _unitofwork.Company.Get(x => x.Id == id);
            if (CompanyToBeDelete == null)
            {
                return Json(new { success = false, message = "Error while delete a company !" });
            }
            _unitofwork.Company.Remove(CompanyToBeDelete);
            _unitofwork.Save();
            return Json(new { success = true, message = "Delete Successfuly!" });
        }
        #endregion
    }//end controller
}
