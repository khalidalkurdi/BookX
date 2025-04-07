using DataAccess.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.ViewModels;
using System.Security.Claims;

namespace MyProject.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private ShoppingCartVM ShoppingCartVM;
        public CartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            
            ShoppingCartVM =new() { 
                ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(u=>u.UserID==userId,includeProperties: "product")
            };
            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.CartPrice = GetPriceBasedOnQuantity(cart);
                ShoppingCartVM.OrderTotal += (cart.Count * cart.CartPrice);
            }
            return View(ShoppingCartVM);
        }
        public IActionResult plus(int Id)
        {
            var cartFromDb =_unitOfWork.ShoppingCart.Get(c=>c.Id==Id);
            cartFromDb.Count += 1;
            _unitOfWork.ShoppingCart.Update(cartFromDb);
            _unitOfWork.Save();
            return RedirectToAction("Index");
        }
        public IActionResult minus(int Id)
        {
            var cartFromDb =_unitOfWork.ShoppingCart.Get(c=>c.Id==Id);
            if (cartFromDb.Count > 1)
            {
                cartFromDb.Count -= 1;
                _unitOfWork.ShoppingCart.Update(cartFromDb);
            }
            else
            {
                _unitOfWork.ShoppingCart.Remove(cartFromDb);
            }
            _unitOfWork.Save();
            return RedirectToAction("Index");
        }
        public IActionResult remove(int Id)
        {
            var cartFromDb =_unitOfWork.ShoppingCart.Get(c=>c.Id==Id);
            _unitOfWork.ShoppingCart.Remove(cartFromDb);
            _unitOfWork.Save();
            return RedirectToAction("Index");
        }
        public double GetPriceBasedOnQuantity(ShoppingCart Cart)
        {
            if (Cart.Count<=50)
            {
                return Cart.product.Price;
            }else if (Cart.Count <= 100)
            {
                return Cart.product.Price50;
            }
            else
            {
                return Cart.product.Price100;
            }
        }
    }
}
