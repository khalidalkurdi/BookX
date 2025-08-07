using DataAccess.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.ViewModels;
using Stripe.Checkout;
using System.Security.Claims;
using Utility;

namespace MyProject.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        [BindProperty]
        public ShoppingCartVM ShoppingCartVM { get; set; }
        public CartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            
            ShoppingCartVM =new() { 
                ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(u=>u.UserID==userId,includeProperties: "product").ToList(),
                orderHeader=new OrderHeader()
            };

            IEnumerable<ProductImage> productImages = _unitOfWork.ProductImage.GetAll().ToList();
            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.product.ProductImages = productImages.Where(p=>p.ProductId==cart.product.Id).ToList();
                cart.CartPrice = GetPriceBasedOnQuantity(cart);
                ShoppingCartVM.orderHeader.OrderTotal += (cart.Count * cart.CartPrice);
            }
            return View(ShoppingCartVM);
        }
        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM = new()
            {
                ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(u => u.UserID == userId, includeProperties: "product"),
                orderHeader = new OrderHeader()
            };

            ShoppingCartVM.orderHeader.user = _unitOfWork.ApplicationUser.Get(u => u.Id == userId);
            ShoppingCartVM.orderHeader.Name = ShoppingCartVM.orderHeader.user.Name;
            ShoppingCartVM.orderHeader.StreetAddress = ShoppingCartVM.orderHeader.user.StreetAddress;
            ShoppingCartVM.orderHeader.PhoneNumber = ShoppingCartVM.orderHeader.user.PhoneNumber;
            ShoppingCartVM.orderHeader.Country = ShoppingCartVM.orderHeader.user.Country;
            ShoppingCartVM.orderHeader.City = ShoppingCartVM.orderHeader.user.City;
            ShoppingCartVM.orderHeader.PostalCode = ShoppingCartVM.orderHeader.user.PostalCode;

            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.CartPrice = GetPriceBasedOnQuantity(cart);
                ShoppingCartVM.orderHeader.OrderTotal += (cart.Count * cart.CartPrice);
            }
            return View(ShoppingCartVM);
        }
        [HttpPost]
        [ActionName("Summary")]
        public IActionResult SummaryPOST()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM.ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(u => u.UserID == userId, includeProperties: "product");
            
            ShoppingCartVM.orderHeader.UserID = userId;
            ShoppingCartVM.orderHeader.OrderDate = DateTime.Now;

            ApplicationUser AppUser = _unitOfWork.ApplicationUser.Get(u=>u.Id==userId);
            
            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.CartPrice = GetPriceBasedOnQuantity(cart);
                ShoppingCartVM.orderHeader.OrderTotal += (cart.Count * cart.CartPrice);
            }


            if (AppUser.CompanyId.GetValueOrDefault()==0)
            {// normal user
                ShoppingCartVM.orderHeader.OrderStatus=SD.StatusPending;
                ShoppingCartVM.orderHeader.PaymentStatus=SD.PaymentStatusPending;
            }
            else
            {// company user
                ShoppingCartVM.orderHeader.OrderStatus = SD.StatusApproved;
                ShoppingCartVM.orderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;
            }
            _unitOfWork.OrderHeader.Add(ShoppingCartVM.orderHeader);
            _unitOfWork.Save();

            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                OrderDetail orderDetail = new()
                {
                    ProductId = cart.ProductId,
                    OrderHeaderId=ShoppingCartVM.orderHeader.Id,
                    Count=cart.Count,
                    Price=cart.CartPrice,
                };
                _unitOfWork.OrderDetail.Add(orderDetail);
            }
            _unitOfWork.Save();

            if (AppUser.CompanyId.GetValueOrDefault()==0)
            {
                //payment Logic
                var Domin = "https://localhost:7224";
                var options = new Stripe.Checkout.SessionCreateOptions
                {
                    SuccessUrl =Domin+ $"/customer/cart/orderconfirmation?id={ShoppingCartVM.orderHeader.Id}",
                    CancelUrl=Domin+$"/customer/cart/Index",
                    LineItems = new List<SessionLineItemOptions>(),
                    Mode = "payment",
                };
                foreach (var cart in ShoppingCartVM.ShoppingCartList)
                {
                    var sessionLineItem = new SessionLineItemOptions()
                    {
                        PriceData= new SessionLineItemPriceDataOptions()
                        {
                            UnitAmount= (long)cart.CartPrice*100,    // convert 20.75$ => 2075
                            Currency="usd",
                            ProductData=new SessionLineItemPriceDataProductDataOptions()
                            {
                                Name=cart.product.Title
                            }
                        },
                        Quantity=cart.Count
                    };
                    options.LineItems.Add(sessionLineItem);
                }
                var service = new SessionService();
                Session session = service.Create(options);
                _unitOfWork.OrderHeader.UpdateStripePaymentId(ShoppingCartVM.orderHeader.Id,session.Id,session.PaymentIntentId);
                _unitOfWork.Save();
                Response.Headers.Add("Location",session.Url);
                return new StatusCodeResult(303);

            }
            return RedirectToAction(nameof(OrderConfirmation),new {ShoppingCartVM.orderHeader.Id});
        }
        public IActionResult OrderConfirmation(int orderHeaderId)
        {
            var orderHeadeFromDb = _unitOfWork.OrderHeader.Get(x => x.Id == orderHeaderId);
            if (orderHeadeFromDb.PaymentStatus != SD.PaymentStatusDelayedPayment)
            {
                var service = new SessionService();
                Session session = service.Get(orderHeadeFromDb.SessionId);
                if (session.PaymentStatus.ToLower()=="paid" )
                {

                    _unitOfWork.OrderHeader.UpdateStripePaymentId(orderHeaderId,session.Id,session.PaymentIntentId);
                    _unitOfWork.OrderHeader.UpdateStatus(orderHeaderId,SD.StatusApproved,SD.PaymentStatusApproved);
                    _unitOfWork.Save();
                }
                var cartsFromDb= _unitOfWork.ShoppingCart.GetAll(x=>x.UserID==orderHeadeFromDb.UserID).ToList();
                _unitOfWork.ShoppingCart.RemoveRange(cartsFromDb);
                _unitOfWork.Save();
                HttpContext.Session.Clear();
            }
            return View(orderHeaderId);
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
            var cartFromDb =_unitOfWork.ShoppingCart.Get(c=>c.Id==Id,traked: true);
            if (cartFromDb.Count > 1)
            {
                cartFromDb.Count -= 1;
                _unitOfWork.ShoppingCart.Update(cartFromDb);
            }
            else
            {
                _unitOfWork.ShoppingCart.Remove(cartFromDb);
                var countCart = _unitOfWork.ShoppingCart.GetAll(u => u.UserID == cartFromDb.UserID).Count() - 1;
                HttpContext.Session.SetInt32(SD.SessionShoppingCart, countCart);
            }
            _unitOfWork.Save();
            return RedirectToAction("Index");
        }
        public IActionResult remove(int Id)
        {
            var cartFromDb =_unitOfWork.ShoppingCart.Get(c=>c.Id==Id,traked:true);
            _unitOfWork.ShoppingCart.Remove(cartFromDb);
            var countCart = _unitOfWork.ShoppingCart.GetAll(u => u.UserID == cartFromDb.UserID).Count()-1;
            HttpContext.Session.SetInt32(SD.SessionShoppingCart, countCart);    
            _unitOfWork.Save();

            return RedirectToAction("Index");
        }
        private double GetPriceBasedOnQuantity(ShoppingCart Cart)
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
