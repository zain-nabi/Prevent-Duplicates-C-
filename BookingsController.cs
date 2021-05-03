using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Triton.FleetManagement.Web.Helper;
using Triton.FleetManagement.Web.Models;
using Triton.Service.Data;
using Triton.Service.Model.TritonFleetManagement.Custom;
using Triton.Service.Model.TritonFleetManagement.StoredProcs;
using Triton.Service.Model.TritonFleetManagement.Tables;
using Triton.Service.Utils;

namespace Triton.FleetManagement.Web.Controllers
{
    [Authorize(Roles = "Admin Director, Service Advisor, Managing Director, Development Manager, Developer, Junior Developer")]
    public class BookingsController : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Index(int customerID)
        {
            var bookings = await BookingsService.LookUpCodesAsync(customerID);
            return View(bookings);
        }

        [HttpPost]
        public async Task<IActionResult> Index(BookingsModel bookingsModel, int customerID)
        {
            string estimatedArrivalDate = bookingsModel.Bookings.EstimatedArrival.Value.ToString("yyyy-MM-dd");
            var Exist = await BookingsService.CheckIfBookingExist(bookingsModel.Bookings.CustomerID, bookingsModel.Bookings.VehicleID, estimatedArrivalDate);
            if(Exist.OrderNumber == "True")
            {
                bookingsModel = await BookingsService.LookUpCodesAsync(customerID);
                bookingsModel.BookingExistMessage = " - Vehicle booking already exist";
                bookingsModel.ExistingBookingsID = Exist.BookingsID;
                return View(bookingsModel);
            }

            bookingsModel.Bookings.CreatedByUserID = User.GetUserId();
            bookingsModel.Bookings.StatusLCID = Convert.ToInt32(bookingsModel.LookupCodeModel.LookUpCodeID);
            var result = await BookingsService.InsertAsync(bookingsModel);

            const string redirectUrl = "/Bookings/GetAllBookings";

            return result ? RedirectToAction("Message", "Home", new { type = StringHelper.Types.SaveSuccess, url = redirectUrl })
                          : RedirectToAction("Message", "Home", new { type = StringHelper.Types.SaveFailed, url = redirectUrl });
        }

        [HttpGet]
        public async Task<IActionResult> GetAllBookings()
        {
            var bvm = new BookingsViewModel();
            bvm.BookingsPerCustomerList = new SelectList(await GetCustomers(), "CustomerID", "Name");
            bvm.ShowReport = false;
            return View(bvm);
        }

        [HttpPost]
        public async Task<IActionResult> GetAllBookings(int CustomerID, string daterange)
        {
            var bvm = new BookingsViewModel();

            DateTime StartDate = DateTime.Now;
            DateTime EndDate = DateTime.Now;
            if (daterange != null)
            {
                var dateSplit = daterange.Split("-");
                StartDate = Convert.ToDateTime(dateSplit[0].Trim());
                EndDate = Convert.ToDateTime(dateSplit[1].Trim());
            }
            string d1 = StartDate.ToString("yyyy-MM-dd");
            string d2 = EndDate.ToString("yyyy-MM-dd");

            bvm.Bookings = await BookingsService.GetBookingsPerCustomer(CustomerID, d1, d2);
            bvm.BookingsPerCustomerList = new SelectList(await GetCustomers(), "CustomerID", "Name");
            bvm.ShowReport = true;
            return View(bvm);
        }

        [HttpGet]
        public async Task<IActionResult> GetBookingsById(int bookingsID)
        {
            var bvm = new BookingsViewModel();
            bvm.BookingsModel = await BookingsService.GetBookingsByIdAsync(bookingsID);
            bvm.MechanicStatusTypesList = new SelectList(bvm.BookingsModel.MechanicTypes, "LookUpCodeID", "Name");
            bvm.MechanicalEmployeesList = new SelectList(bvm.BookingsModel.MechanicalEmployees, "EmployeeID", "FullNames");
            bvm.CustomerList = new SelectList(bvm.BookingsModel.Customers, "CustomerID", "Name");
            bvm.VehicleList = new SelectList(bvm.BookingsModel.Vehicles, "VehicleID", "RegistrationNumber");
            bvm.ShowReport = true;
            return View(bvm);
        }

        [HttpGet]
        public async Task<IActionResult> DeleteBookings(int bookingsID)
        {
            var bvm = new BookingsViewModel();
            bvm.BookingDetailsByID = await BookingsService.GetBookingDetailsByID(bookingsID);
            return View(bvm);
        }

        [HttpPost]
        public async Task<IActionResult> EditBookings(BookingsViewModel model)
        {
            var bookingsHelper = new BookingsHelper();
            var result = await BookingsService.UpdateAsync(bookingsHelper.UpdateBooking(model, User.GetUserId()));

            const string redirectUrl = "/Bookings/GetAllBookings";

            return result ? RedirectToAction("Message", "Home", new { StringHelper.Types.UpdateSuccess, url = redirectUrl })
                          : RedirectToAction("Message", "Home", new { StringHelper.Types.UpdateFailed, url = redirectUrl });
        }


        [HttpPost]
        public async Task<IActionResult> DeleteBookings(proc_BookingDetails_GetByID bookingDetails)
        {
            var bookingsHelper = new BookingsHelper();
            var result = await BookingsService.DeleteBooking(bookingsHelper.DeleteBookings(bookingDetails, User.GetUserId()));

            const string redirectUrl = "/Bookings/GetAllBookings";

            return result ? RedirectToAction("Message", "Home", new { StringHelper.Types.UpdateSuccess, url = redirectUrl })
                          : RedirectToAction("Message", "Home", new { StringHelper.Types.UpdateFailed, url = redirectUrl });
        }

        public async Task<JsonResult> GetVendorCodesPerCustomer()
        {
            var vendorcodes = await BookingsService.GetVendorCodesPerCustomer();
            foreach(var item in vendorcodes)
            {
                if(item.VendorCodes == null)
                {
                    item.VendorCodes = "N/A";
                }
            }
            return Json(vendorcodes);
        }

        private async Task<List<CustomersModels>> GetCustomers()
        {
            var customerList = await BookingsService.GetAllCustomersAsync();
            customerList.Insert(0, new CustomersModels { CustomerID = 0, Name = "All" });
            return customerList;
        }
    }
}
