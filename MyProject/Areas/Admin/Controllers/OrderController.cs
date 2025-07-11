using DataAccess.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Models;
using Models.ViewModels;
using MyProject.Models;
using Stripe;
using Stripe.Checkout;
using Stripe.Climate;
using System.Diagnostics;
using System.Security.Claims;
using Utility;

namespace MyProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitofwork;
        [BindProperty]
        private OrderVM orderVM {  get; set; }
        public OrderController(IUnitOfWork unitOfWork)
        {
           _unitofwork = unitOfWork;
           
        }
        public IActionResult Index()
        {
            
            return View();
        } 
        public IActionResult Details(int orderHeaderId)
        {
            orderVM = new OrderVM
            {
                orderHeader = _unitofwork.OrderHeader.Get(x=>x.Id==orderHeaderId,includeProperties: "user"),
                orderDetail = _unitofwork.OrderDetail.GetAll(x => x.OrderHeaderId == orderHeaderId,includeProperties:"product" )
            };
            return View(orderVM);
        }
        [HttpPost(nameof(Details))]
        [Authorize(Roles = SD.Role_Admin+" ,"+SD.Role_Employee)]
        public IActionResult UpdateOrderDetail()
        {
            var orderHeaderFromDb = _unitofwork.OrderHeader.Get(x=>x.Id==orderVM.orderHeader.Id);

            orderHeaderFromDb.Name= orderVM.orderHeader.Name;
            orderHeaderFromDb.PhoneNumber= orderVM.orderHeader.PhoneNumber;
            orderHeaderFromDb.StreetAddress= orderVM.orderHeader.StreetAddress;
            orderHeaderFromDb.Country= orderVM.orderHeader.Country;
            orderHeaderFromDb.City= orderVM.orderHeader.City;
            orderHeaderFromDb.PostalCode= orderVM.orderHeader.PostalCode;
            if (!string.IsNullOrEmpty(orderHeaderFromDb.Carrier))
            {
                orderHeaderFromDb.Carrier = orderHeaderFromDb.Carrier;
            }
            if (!string.IsNullOrEmpty(orderHeaderFromDb.TrackingNumber))
            {
                orderHeaderFromDb.TrackingNumber = orderHeaderFromDb.TrackingNumber;
            }
            _unitofwork.OrderHeader.Update(orderHeaderFromDb);
            _unitofwork.Save();
            TempData["success"] = "Order details Updated successfuly!";

            return RedirectToAction(nameof(Details), new { orderHeaderId = orderVM.orderHeader.Id});
        }
        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + " ," + SD.Role_Employee)]
        public IActionResult StartProccing()
        {            
            _unitofwork.OrderHeader.UpdateStatus(orderVM.orderHeader.Id, SD.StatusInProcess);
            _unitofwork.Save();
            TempData["success"] = "Order details Updated successfuly!";
            return RedirectToAction(nameof(Details), new { orderHeaderId = orderVM.orderHeader.Id});
        }
        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + " ," + SD.Role_Employee)]
        public IActionResult ShipOrder()
        {
            var orderHeaderFromDb = _unitofwork.OrderHeader.Get(o=>o.Id==orderVM.orderHeader.Id);
            orderHeaderFromDb.Carrier = orderVM.orderHeader.Carrier;
            orderHeaderFromDb.TrackingNumber = orderVM.orderHeader.TrackingNumber;
            orderHeaderFromDb.OrderStatus = SD.StatusShipped;
            orderHeaderFromDb.ShippingDate= DateTime.Now;
            if (orderVM.orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                orderHeaderFromDb.PaymentDueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30));
            }
            
            _unitofwork.OrderHeader.Update(orderVM.orderHeader);
            _unitofwork.Save();
            TempData["success"] = "Order details Updated successfuly!";
            return RedirectToAction(nameof(Details), new { orderHeaderId = orderVM.orderHeader.Id});
        }
        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + " ," + SD.Role_Employee)]
        public IActionResult CancelOrder()
        {
            var orderHeaderFromDb = _unitofwork.OrderHeader.Get(u=>u.Id==orderVM.orderHeader.Id);
            if (orderHeaderFromDb.OrderStatus == SD.StatusApproved)
            {
                var options = new RefundCreateOptions()
                {
                    Reason=RefundReasons.RequestedByCustomer,
                    PaymentIntent=orderHeaderFromDb.PaymentIntentId
                };
                var service = new RefundService();
                Refund refund=service.Create(options);
                _unitofwork.OrderHeader.UpdateStatus(orderHeaderFromDb.Id,SD.StatusCancelled,SD.StatusRefunded);
            }
            else
            {
                _unitofwork.OrderHeader.UpdateStatus(orderHeaderFromDb.Id, SD.StatusCancelled, SD.StatusCancelled);
            }
            _unitofwork.Save();

            TempData["delete"] = "Order Canceled successfuly!";
            return RedirectToAction(nameof(Details), new { orderHeaderId = orderVM.orderHeader.Id });
        }
        [HttpPost(nameof(Details))]
        [Authorize(Roles = SD.Role_Admin + " ," + SD.Role_Employee)]
        public IActionResult Details_PAY_NOW()
        {
            orderVM.orderHeader= _unitofwork.OrderHeader.Get(x => x.Id == orderVM.orderHeader.Id, includeProperties: "user");
            orderVM.orderDetail= _unitofwork.OrderDetail.GetAll(x => x.OrderHeaderId == orderVM.orderHeader.Id, includeProperties: "product");

            //payment Logic
            var Domin = "https://localhost:7224";
            var options = new Stripe.Checkout.SessionCreateOptions
            {
                SuccessUrl = Domin + $"/Admin/order/PaymentConfirmation?orderHeaderId={orderVM.orderHeader.Id}",
                CancelUrl = Domin + $"/Admin/order/Details?orderHeaderId={orderVM.orderHeader.Id}",
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
            };
            foreach (var cart in  orderVM.orderDetail)
            {
                var sessionLineItem = new SessionLineItemOptions()
                {
                    PriceData = new SessionLineItemPriceDataOptions()
                    {
                        UnitAmount = (long)cart.Price * 100,    // convert 20.75$ => 2075
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions()
                        {
                            Name = cart.product.Title
                        }
                    },
                    Quantity = cart.Count
                };
                options.LineItems.Add(sessionLineItem);
            }
            var service = new SessionService();
            Session session = service.Create(options);
            _unitofwork.OrderHeader.UpdateStripePaymentId(orderVM.orderHeader.Id, session.Id, session.PaymentIntentId);
            _unitofwork.Save();
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }
        
        [Authorize(Roles = SD.Role_Admin + " ," + SD.Role_Employee)]
        public IActionResult PaymentConfirmation(int orderHeaderId)
        {
            orderVM.orderHeader = _unitofwork.OrderHeader.Get(x => x.Id == orderHeaderId);
            if (orderVM.orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                var service = new SessionService();
                Session session = service.Get(orderVM.orderHeader.SessionId);
                if (session.PaymentStatus.ToLower() == "paid")
                {

                    _unitofwork.OrderHeader.UpdateStripePaymentId(orderVM.orderHeader.Id, session.Id, session.PaymentIntentId);
                    _unitofwork.OrderHeader.UpdateStatus(orderVM.orderHeader.Id, orderVM.orderHeader.OrderStatus, SD.PaymentStatusApproved);
                    _unitofwork.Save();
                }                               
            }
            return View(orderHeaderId);
        }
        #region Api Call 
        [HttpGet]
        public IActionResult GetAll(string status)
        {
            IEnumerable<OrderHeader> orderHeadersList;
            if (User.IsInRole(SD.Role_Employee)|| User.IsInRole(SD.Role_Admin))
            {
                orderHeadersList = _unitofwork.OrderHeader.GetAll(includeProperties: "user").ToList();
                switch (status)
                {
                    case "pending":orderHeadersList= orderHeadersList.Where(x => x.PaymentStatus == SD.PaymentStatusDelayedPayment); break;
                    case "inprocess": orderHeadersList = orderHeadersList.Where(x => x.OrderStatus == SD.StatusInProcess); break;
                    case "completed": orderHeadersList = orderHeadersList.Where(x => x.OrderStatus == SD.StatusShipped); break;
                    case "approved": orderHeadersList = orderHeadersList.Where(x => x.OrderStatus == SD.StatusApproved); break;
                }
            }
            else
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
                orderHeadersList = _unitofwork.OrderHeader.GetAll(u => u.UserID == userId, includeProperties: "user");
            }
            
            return Json(new { data = orderHeadersList});
        }
        #endregion
    }//end controller
}
