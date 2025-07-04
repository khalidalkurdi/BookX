using DataAccess.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Models;
using Models.ViewModels;
using MyProject.Models;
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
        public IActionResult Details(int orderId)
        {
            orderVM = new OrderVM
            {
                orderHeader = _unitofwork.OrderHeader.Get(x=>x.Id==orderId,includeProperties: "user"),
                orderDetail = _unitofwork.OrderDetail.GetAll(x => x.OrderHeaderId == orderId,includeProperties:"product" )
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

            return RedirectToAction(nameof(Details), new {orderId=orderVM.orderHeader.Id});
        }
        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + " ," + SD.Role_Employee)]
        public IActionResult StartProccing()
        {            
            _unitofwork.OrderHeader.UpdateStatus(orderVM.orderHeader.Id, SD.StatusInProcess);
            _unitofwork.Save();
            TempData["success"] = "Order details Updated successfuly!";
            return RedirectToAction(nameof(Details), new { orderId=orderVM.orderHeader.Id});
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
            return RedirectToAction(nameof(Details), new { orderId=orderVM.orderHeader.Id});
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
