using DataAccess.Db;
using DataAccess.InterfacesRepository;
using Microsoft.AspNetCore.Identity;
using Models;
using Models.ViewModels;
using MyProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repository
{
    public class ApplicationUserRepository : Repository<ApplicationUser>, IApplicationUserRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        public ApplicationUserRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public void Update(ApplicationUser entity)
        {
            _db.ApplicationUsers.Update(entity);
        }
    }
}
