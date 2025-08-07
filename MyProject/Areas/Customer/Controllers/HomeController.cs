using DataAccess.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using MyProject.Models;
using System.Diagnostics;
using System.Security.Claims;
using Utility;
namespace MyProject.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            List<Product> productList = _unitOfWork.Product.GetAll(includeProperties: "Category,ProductImages").ToList();
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (claim != null)
            {
                var cartCount = _unitOfWork.ShoppingCart.GetAll(u => u.UserID == claim.Value).Count();
                HttpContext.Session.SetInt32(SD.SessionShoppingCart, cartCount);
            }
            else
            {
                HttpContext.Session.SetInt32(SD.SessionShoppingCart, 0);
            }

            return View(productList);
        }
        [HttpGet]
        public IActionResult Details(int producdId)
        {
            var ShoppingCart = new ShoppingCart
            {
                product=_unitOfWork.Product.Get(p=>p.Id==producdId, includeProperties: "Category,ProductImages"),
                ProductId=producdId

            };
            return View(ShoppingCart);
        }
        [HttpPost]
        [Authorize]
        public IActionResult Details(ShoppingCart cart)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            cart.UserID= userId;

            var cartFromDb = _unitOfWork.ShoppingCart.Get(sh=>sh.ProductId==cart.ProductId && sh.UserID==userId);
            if (cartFromDb != null)
            {
                cartFromDb.Count += cart.Count;
                _unitOfWork.ShoppingCart.Update(cartFromDb);
                _unitOfWork.Save();
            }
            else
            {
                _unitOfWork.ShoppingCart.Add(cart);
                _unitOfWork.Save();
                var cartCount = _unitOfWork.ShoppingCart.GetAll(u => u.UserID == userId).Count();
                HttpContext.Session.SetInt32(SD.SessionShoppingCart, cartCount);
            }

            TempData["update"] = "Cart updated successfuly!";
            return RedirectToAction("Index");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
