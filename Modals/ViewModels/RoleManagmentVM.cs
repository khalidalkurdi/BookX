using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ViewModels
{
    public class RoleManagmentVM
    {
        [BindProperty]
        public ApplicationUser user { get; set; }               
        public IEnumerable<SelectListItem> RoleLsit { get; set; }   
        public IEnumerable<SelectListItem> CompanyLsit { get; set; }   

    }
}
