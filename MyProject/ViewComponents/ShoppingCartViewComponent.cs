using DataAccess.UnitOfWork;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Utility;

namespace MyProject.ViewComponents
{
    public class ShoppingCartViewComponent : ViewComponent
    {
        private readonly IUnitOfWork _unitOfWork;

        public ShoppingCartViewComponent( IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (claim != null)
            {                
                if (HttpContext.Session.GetInt32(SD.SessionShoppingCart) == null)
                {
                    var cartCount = _unitOfWork.ShoppingCart.GetAll(u => u.UserID == claim.Value).Count();
                    HttpContext.Session.SetInt32(SD.SessionShoppingCart, cartCount);
                }
                return View(HttpContext.Session.GetInt32(SD.SessionShoppingCart));                 
            }
            else
            {
                HttpContext.Session.Clear();
                return View(0);
            }            
        }

    }
}
