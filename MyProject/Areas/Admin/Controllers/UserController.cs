using DataAccess.Db;
using DataAccess.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.ViewModels;
using System;
using Utility;

namespace MyProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<IdentityUser> _userManager;
        public UserController(ApplicationDbContext db , RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager)
        {
           _db = db;
           _roleManager = roleManager;
            _userManager = userManager;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult RoleManagment(string userId)
        {           
            var RoleManagmentVM = new RoleManagmentVM
            {                
                User= _db.ApplicationUsers.Include(u=>u.company).FirstOrDefault(u => u.Id == userId),

                CompanyLsit= _db.Companies.Select(c => new SelectListItem
                {
                    Text = c.Name,
                    Value = c.Id.ToString()
                }),

                RoleLsit= _roleManager.Roles.Select(r => new SelectListItem
                {
                    Text = r.Name,
                    Value = r.Name
                })
            };
            var roleId= _db.UserRoles.FirstOrDefault(u => u.UserId == userId).RoleId;
            RoleManagmentVM.User.Role = _db.Roles.FirstOrDefault(u=>u.Id ==roleId).Name;
            return View(RoleManagmentVM);
        }
        [HttpPost,ActionName(nameof(RoleManagment))]
        public IActionResult RoleManagmentPost(RoleManagmentVM roleManagmentVM)
        {
            var roleId = _db.UserRoles.FirstOrDefault(u=>u.UserId== roleManagmentVM.User.Id).RoleId;
            var oldRole = _db.Roles.FirstOrDefault(u=>u.Id== roleId).Name;

            var userFromDb = _db.ApplicationUsers.FirstOrDefault(u => u.Id == roleManagmentVM.User.Id);
            if (roleManagmentVM.User.Role != oldRole)
            {
                if (roleManagmentVM.User.Role == SD.Role_Company )
                {
                    userFromDb.CompanyId = roleManagmentVM.User.CompanyId;
                }
                if(oldRole==SD.Role_Company)
                {
                    userFromDb.CompanyId = null;
                }         
                _userManager.RemoveFromRoleAsync(userFromDb, oldRole).GetAwaiter().GetResult();
                _userManager.AddToRoleAsync(userFromDb, roleManagmentVM.User.Role).GetAwaiter().GetResult();
            }
            else if(roleManagmentVM.User.Role==SD.Role_Company)
            {
                userFromDb.CompanyId = roleManagmentVM.User.CompanyId;
            }
            _db.SaveChanges();

            TempData["success"] = "The role Updateed  Successfuly !";
            return RedirectToAction(nameof(Index));
        }

        #region Api Call 
        [HttpGet]
        public IActionResult GetAll()
        {
            List<ApplicationUser> list = _db.ApplicationUsers.Include(u => u.company).ToList();
            var roles = _db.Roles.ToList();
            var userRoles=_db.UserRoles.ToList();

            foreach (var user in list)
            {
                var roleId = userRoles.FirstOrDefault(u=>u.UserId==user.Id).RoleId;
                user.Role = roles.FirstOrDefault(r=>r.Id ==roleId).Name;

                if(user.company == null)
                {
                    user.company = new() { Name=""};
                }
            }
            return Json(new { data = list });
        }
        [HttpPost]
        public IActionResult LockUnLock([FromBody] string id)
        {
            var userFromDb = _db.ApplicationUsers.FirstOrDefault(u => u.Id == id);
            if(userFromDb == null)
            {
                return Json(new { success = false, message = "Error while lock / unlock user" });
            }
            if (userFromDb.LockoutEnd!=null && userFromDb.LockoutEnd > DateTime.Now)
            {//unlock
                userFromDb.LockoutEnd = DateTime.Now;
            }
            else
            {//lock
                userFromDb.LockoutEnd = DateTime.Now.AddYears(100);
            }
            _db.SaveChanges();
            return Json(new { success = true, message = "Operation Successful" });
        }
        #endregion
    }//end controller
}
