using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Triton.FleetManagement.Web.Helper;
using Triton.FleetManagement.Web.Models;
using Triton.Service.Data;
using Triton.Service.Model.TritonFleetManagement.Tables;
using Triton.Service.Utils;

namespace Triton.FleetManagement.Web.Controllers
{
    [Authorize(Roles = "Admin Director, Service Advisor, Managing Director, Development Manager, Developer, Junior Developer")]
    public class VehicleController : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Search()
        {

            var vehicleModel = new VehicleViewModel()
            {
                Customers = new SelectList(await GetCustomers(), "CustomerID", "Name")
            };
            return View(vehicleModel);
        }

        [HttpPost]
        public async Task<IActionResult> Search(VehicleViewModel vehicleViewModel)
        {
            if(vehicleViewModel.RegistrationNumber == null)
            {
                vehicleViewModel.RegistrationNumber = "null";
            }

            var vehicleModel = new VehicleViewModel()
            {
                Customers = new SelectList(await GetCustomers(), "CustomerID", "Name"),
                Vehicles = await VehicleService.GetVehiclesPerCustomer(vehicleViewModel.Vehicle.Vehicles.CustomerID, vehicleViewModel.RegistrationNumber),
                ShowReport = true
            };

            return View(vehicleModel);
        }

        [HttpGet]
        public async Task<IActionResult> Vehicles()
        {
            var vehicleModel = new VehicleViewModel()
            {
                Vehicles = await VehicleService.GetVehicles()
            };

            return View(vehicleModel);
        }

        [HttpGet]
        public async Task<IActionResult> VehicleByID(int VehicleID)
        {
            var vehicleModel = new VehicleViewModel()
            {
                VehicleDetailsByID = await VehicleService.GetVehicleDetailsByID(VehicleID)
            };

            return View(vehicleModel);
        }

        [HttpGet]
        public async Task<IActionResult> VehiclePerCustomer(int CustomerID)
        {
            var vehicleModel = new VehicleViewModel()
            {

                Vehicles = await VehicleService.GetVehiclesPerCustomer(CustomerID, "null")
            };

            return View(vehicleModel);
        }

        [HttpGet]
        public async Task<IActionResult> Deactivate(int VehicleID)
        {
            var vehicleModel = new VehicleViewModel();
            vehicleModel.VehicleDetailsByID = await VehicleService.GetVehicleDetailsByID(VehicleID);
            return View(vehicleModel);
        }

        [HttpPost]
        public async Task<IActionResult> Deactivate(VehicleViewModel model)
        {
                var vehicleHelper = new VehicleHelper();
                model.VehicleDetailsByID.DeletedByUserID = User.GetUserId();
                var result = await VehicleService.DeActivateVehicleAsync(model.VehicleDetailsByID);
                string redirectUrl = string.Format("/Vehicle/VehiclePerCustomer?customerID= {0}", model.VehicleDetailsByID.CustomerID);

                return result ? RedirectToAction("Message", "Home", new { type = StringHelper.Types.UpdateSuccess, url = redirectUrl })
                              : RedirectToAction("Message", "Home", new { type = StringHelper.Types.UpdateFailed, url = redirectUrl });
        }


        [HttpGet]
        public async Task<IActionResult> Activate(int VehicleID)
        {
            var vehicleModel = new VehicleViewModel();
            vehicleModel.VehicleDetailsByID = await VehicleService.GetVehicleDetailsByID(VehicleID);
            return View(vehicleModel);
        }

        [HttpPost]
        public async Task<IActionResult> Activate(VehicleViewModel model)
        {
                var vehicleHelper = new VehicleHelper();
                var result = await VehicleService.ActivateVehicleAsync(model.VehicleDetailsByID);
                string redirectUrl = string.Format("/Vehicle/VehiclePerCustomer?customerID= {0}", model.VehicleDetailsByID.CustomerID);

            return result ? RedirectToAction("Message", "Home", new { type = StringHelper.Types.UpdateSuccess, url = redirectUrl })
                              : RedirectToAction("Message", "Home", new { type = StringHelper.Types.UpdateFailed, url = redirectUrl });
        }


        [HttpGet]
        public async Task<IActionResult> Create(int CustomerID)
        {
            var vehicleModel = new VehicleViewModel()
            {
                CustomerID = CustomerID,
                TailLiftTypes =  new SelectList(await VehicleService.GetLookUpCodesPerCategory(77), "LookUpCodeID", "Name"),
                ServiceIntervals = new SelectList(await VehicleService.GetLookUpCodesPerCategory(78), "LookUpCodeID", "Name"),
                VehicleClasses = new SelectList(await VehicleService.GetLookUpCodesPerCategory(80), "LookUpCodeID", "Name"),
                VehicleBrands = new SelectList(await VehicleService.GetLookUpCodesPerCategory(79), "LookUpCodeID", "Name"),
                Customers = new SelectList(await VehicleService.GetAllCustomers(), "CustomerID", "Name")
            };


            return View(vehicleModel);
        }

        [HttpPost]
        public async Task<IActionResult> Create(VehicleViewModel model)
        {
            var vehicleHelper = new VehicleHelper();
            string redirectUrl ="";
            if (model.CustomerID != 0)
            {
                redirectUrl = string.Format("/Vehicle/VehiclePerCustomer?customerID= {0}", model.CustomerID);
            }
            else if(model.Vehicle.Vehicles.CustomerID != 0)
            {
                redirectUrl = string.Format("/Vehicle/VehiclePerCustomer?customerID= {0}", model.Vehicle.Vehicles.CustomerID);
            }
            
            var result = await VehicleService.InsertVehicleAsync(vehicleHelper.CreateVehicle(model, User.GetUserId()));
            if (result == true)
            {
                model.TailLiftTypes = new SelectList(await VehicleService.GetLookUpCodesPerCategory(77), "LookUpCodeID", "Name");
                model.ServiceIntervals = new SelectList(await VehicleService.GetLookUpCodesPerCategory(78), "LookUpCodeID", "Name");
                model.VehicleClasses = new SelectList(await VehicleService.GetLookUpCodesPerCategory(80), "LookUpCodeID", "Name");
                model.VehicleBrands = new SelectList(await VehicleService.GetLookUpCodesPerCategory(79), "LookUpCodeID", "Name");
                model.Customers = new SelectList(await VehicleService.GetAllCustomers(), "CustomerID", "Name");
                model.CarExistMessage = "Vehicle registration already exist";
                model.CarExist = "True";
                return View(model);
            }

            return result == false ? RedirectToAction("Message", "Home", new { type = StringHelper.Types.SaveSuccess, url = redirectUrl })
              : RedirectToAction("Message", "Home", new { type = StringHelper.Types.SaveFailed, url = redirectUrl });

        }

        [HttpGet]
        public async Task<IActionResult> Update(int VehicleID)
        {
            var vehicleModel = new VehicleViewModel()
            {
                VehicleDetailsByID = await VehicleService.GetVehicleDetailsByID(VehicleID),
                TailLiftTypes = new SelectList(await VehicleService.GetLookUpCodesPerCategory(77), "LookUpCodeID", "Name"),
                ServiceIntervals = new SelectList(await VehicleService.GetLookUpCodesPerCategory(78), "LookUpCodeID", "Name"),
                VehicleClasses = new SelectList(await VehicleService.GetLookUpCodesPerCategory(80), "LookUpCodeID", "Name"),
                VehicleBrands = new SelectList(await VehicleService.GetLookUpCodesPerCategory(79), "LookUpCodeID", "Name"),
                Customers = new SelectList(await VehicleService.GetAllCustomers(), "CustomerID", "Name")
            };
            return View(vehicleModel);
        }

        [HttpPost]
        public async Task<IActionResult> Update(VehicleViewModel model)
        {
            var vehicleHelper = new VehicleHelper();
            var result = await VehicleService.CheckIfRegistrationExists(model.VehicleDetailsByID.RegistrationNumber);
            var VehicleDetails = await VehicleService.GetVehicleDetailsByID(model.VehicleDetailsByID.VehicleID);
            string redirectUrl;
            if (VehicleDetails.RegistrationNumber == model.VehicleDetailsByID.RegistrationNumber)
            {

                if (VehicleDetails.RegistrationNumber != model.VehicleDetailsByID.RegistrationNumber)
                {
                    if (VehicleDetails.RegistrationNumber == model.VehicleDetailsByID.RegistrationNumber)
                    {
                        model.CarExistMessage = "";
                    }
                    if (VehicleDetails.RegistrationNumber != model.VehicleDetailsByID.RegistrationNumber)
                    {
                        model.CarExistMessage = StringHelper.Html.VehicleRegExistMessage;
                    }
                    if (result.RegistrationNumber == "False")
                    {
                        await VehicleService.UpdateVehicleAsync(vehicleHelper.UpdateVehicle(model));
                        redirectUrl = string.Format("/Vehicle/VehiclePerCustomer?customerID= {0}", model.VehicleDetailsByID.CustomerID);

                        return RedirectToAction("Message", "Home", new { type = StringHelper.Types.UpdateSuccess, url = redirectUrl });
                    }
                    else
                    {
                        var vehicleModel = new VehicleViewModel()
                        {
                            TailLiftTypes = new SelectList(await VehicleService.GetLookUpCodesPerCategory(77), "LookUpCodeID", "Name"),
                            ServiceIntervals = new SelectList(await VehicleService.GetLookUpCodesPerCategory(78), "LookUpCodeID", "Name"),
                            VehicleClasses = new SelectList(await VehicleService.GetLookUpCodesPerCategory(80), "LookUpCodeID", "Name"),
                            VehicleBrands = new SelectList(await VehicleService.GetLookUpCodesPerCategory(79), "LookUpCodeID", "Name"),
                            Customers = new SelectList(await VehicleService.GetAllCustomers(), "CustomerID", "Name")
                        };

                        vehicleModel.CarExistMessage = StringHelper.Html.VehicleRegExistMessage;
                        vehicleModel.CarExist = "True";
                        return View(vehicleModel);
                    }
                }
                await VehicleService.UpdateVehicleAsync(vehicleHelper.UpdateVehicle(model));
                redirectUrl = string.Format("/Vehicle/VehiclePerCustomer?customerID= {0}", model.VehicleDetailsByID.CustomerID);

                return RedirectToAction("Message", "Home", new { type = StringHelper.Types.UpdateSuccess, url = redirectUrl });
            }


            if (VehicleDetails.RegistrationNumber == model.VehicleDetailsByID.RegistrationNumber)
            {
                model.CarExistMessage = "";
            }
            if (VehicleDetails.RegistrationNumber != model.VehicleDetailsByID.RegistrationNumber)
            {
                model.CarExistMessage = StringHelper.Html.VehicleRegExistMessage;
            }
            if (result.RegistrationNumber == "False")
            {
                await VehicleService.UpdateVehicleAsync(vehicleHelper.UpdateVehicle(model));
                redirectUrl = string.Format("/Vehicle/VehiclePerCustomer?customerID= {0}", model.VehicleDetailsByID.CustomerID);

                return RedirectToAction("Message", "Home", new { type = StringHelper.Types.UpdateSuccess, url = redirectUrl });
            }
            else
            {
                var vehicleModel = new VehicleViewModel()
                {
                    TailLiftTypes = new SelectList(await VehicleService.GetLookUpCodesPerCategory(77), "LookUpCodeID", "Name"),
                    ServiceIntervals = new SelectList(await VehicleService.GetLookUpCodesPerCategory(78), "LookUpCodeID", "Name"),
                    VehicleClasses = new SelectList(await VehicleService.GetLookUpCodesPerCategory(80), "LookUpCodeID", "Name"),
                    VehicleBrands = new SelectList(await VehicleService.GetLookUpCodesPerCategory(79), "LookUpCodeID", "Name"),
                    Customers = new SelectList(await  VehicleService.GetAllCustomers(), "CustomerID", "Name")
                };

                vehicleModel.CarExistMessage = StringHelper.Html.VehicleRegExistMessage;
                vehicleModel.CarExist = "True";
                return View(vehicleModel);
            }
        }

        private async Task<List<Customer>> GetCustomers()
        {
            var customerList = await VehicleService.GetAllCustomers();
            customerList.Insert(0, new Customer { CustomerID = 0, Name = "All" });
            return customerList;
        }

        public async Task<JsonResult> GetVehicleByCustomerID(int customerID)
        {
            var vehicles = await VehicleService.GetVehiclesByCustomerIDAsyn(customerID);
            foreach (var item in vehicles)
            {
                if (item.VinNumber == "0")
                {
                    item.VinNumber = "N/A";
                }
                if (item.FleetNumber == "0")
                {
                    item.FleetNumber = "N/A";
                }
            }
            return Json(vehicles);
        }
    }
}
