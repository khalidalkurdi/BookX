using DataAccess.Db;
using DataAccess.UnitOfWork;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace DataAccess.DbInitializer
{
    public class DbInitializer : IDbInitializer
    {
        private readonly ApplicationDbContext _db;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<IdentityUser> _userManager;
        public DbInitializer(ApplicationDbContext db, RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager)
        {
            _db = db;
            this._roleManager = roleManager;
            this._userManager = userManager;
        }
        public void Initialize()
        {
            // applied migrations
            try
            {
                if (_db.Database.GetPendingMigrations().Count() > 0)
                {
                    _db.Database.Migrate();
                }
            }
            catch (Exception ex)
            {
                // log the exception
                throw new Exception("Database migration failed", ex);
            }
            // Add Roles if they do not exist
            if (  _roleManager.RoleExistsAsync(SD.Role_Customer).GetAwaiter().GetResult())
            {
                 _roleManager.CreateAsync(new IdentityRole(SD.Role_Company)).GetAwaiter().GetResult();
                 _roleManager.CreateAsync(new IdentityRole(SD.Role_Customer)).GetAwaiter().GetResult();
                 _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin)).GetAwaiter().GetResult();
                 _roleManager.CreateAsync(new IdentityRole(SD.Role_Employee)).GetAwaiter().GetResult();
                
                // Create Admin user if it does not exist
                _userManager.CreateAsync(new ApplicationUser()
                {
                    Name = "Khalid AL-Kurdi",
                    Email = "Khalidalkurdi541@gmail.com",
                    PhoneNumber = "0789486842",
                    City = "Zarqa",
                    StreetAddress = "Army Street",
                    Country = "Jordan",
                    PostalCode = "12345"
                }, "Admin").GetAwaiter().GetResult();

                var userFromDb = _db.Users.FirstOrDefault(u=>u.Email== "Khalidalkurdi541@gmail.com");
                if (userFromDb != null)
                {
                    _userManager.AddToRoleAsync(userFromDb, SD.Role_Admin).GetAwaiter().GetResult();
                }

            }

        }
    }
}

