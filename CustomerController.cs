using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Triton.CRM.Models;
using Triton.Model.CRM.Tables;
using Triton.Service.Data;
using Triton.Service.Model.CRM.Tables;
using Triton.Service.Utils;

namespace Triton.CRM.Controllers
{
    [Authorize(Roles = "Service Consultant, Business Development Manager, Sales Administrator, Sales Director, National Sales Manager, Development Manager, Junior Developer")]
    public class CustomerController : Controller
    {

        [HttpGet]
        public async Task<IActionResult> Search()
        {
            var csm = new CustomerSearchModel();
            csm.RepsCustomerDetails = await CustomerService.GetRepsCustomers(User.GetUserId());
            if (csm.RepsCustomerDetails.Count() == 0)
            {
                csm.CustomerDetails = null;
                csm.RepsCustomerDetails = null;
                csm.ShowReport = false;
                return View(csm);
            }
            else
            {
                csm.CustomerDetails = null;
                csm.ShowReport = true;
                return View(csm);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Search(string accountCode)
        {
            var csm = new CustomerSearchModel();
            if(accountCode != null)
            {
                csm.CustomerDetails = await CustomerService.GetCustomerDetails(accountCode);
                csm.RepsCustomerDetails = null;
                csm.ShowReport = true;
                return View(csm);
            }
            else
            {
                csm.RepsCustomerDetails = await CustomerService.GetRepsCustomers(User.GetUserId());
                if (csm.RepsCustomerDetails.Count() == 0)
                {
                    csm.CustomerDetails = null;
                    csm.ShowReport = false;
                }
                return View(csm);
            }
        }

        [HttpGet]
        public async Task<IActionResult> ContactList(string accountCode)
        {
            var csm = new CustomerSearchModel();
            if (accountCode != null)
            {
                csm.CustomerAdditionalContactsList = await CustomerAdditionalContactsService.GetCustomerAdditionalContactsByAccountCode(accountCode);
                csm.Customers = await CustomerService.GetCRMCustomerByAccountCode(accountCode);
                csm.ErrorMessage = "<div class=\"alert alert-soft-danger\" role=\"alert\"><center>There are no contacts</center></div>";
                csm.Customers = await CustomerService.GetCRMCustomerByAccountCode(accountCode);
                csm.CustomerName = csm.Customers.Name;
                csm.ShowReport = true;
                return View(csm);
            }
            else
            {
                return View(new CustomerSearchModel 
                {
                    CustomerAdditionalContactsList = new List<CustomerAdditionalContacts>(),
                    CustomerName = ""
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ContactList(CustomerSearchModel csm)
        {
            var model = new CustomerSearchModel();

            try
            {
                if (csm.AccountCode != null)
                {
                    model.CustomerAdditionalContactsList = await CustomerAdditionalContactsService.GetCustomerAdditionalContactsByAccountCode(csm.AccountCode);
                    model.Customers = await CustomerService.GetCRMCustomerByAccountCode(csm.AccountCode);
                    if(model.Customers == null)
                    {
                        model.CustomerName = "";
                    }
                    else
                    {
                        model.CustomerName = model.Customers.CustomerName;
                    }
                    model.ErrorMessage = "<div class=\"alert alert-soft-danger\" role=\"alert\"><center>There are no contacts</center></div>";
                    model.ShowReport = true;
                    return View(model);
                }
                else
                {
                    csm.CustomerAdditionalContacts.DeletedOn = System.DateTime.Now;
                    csm.CustomerAdditionalContacts.DeletedByUserID = User.GetUserId();

                    await CustomerAdditionalContactsService.UpdateCustomerAdditionalContactsAsync(csm.CustomerAdditionalContacts);
                    model.Customers = await CustomerService.GetCustomerAccountCodeByCustomerId(csm.CustomerAdditionalContacts.CustomerID);
                    model.CustomerAdditionalContactsList = await CustomerAdditionalContactsService.GetCustomerAdditionalContactsByAccountCode(csm.Customers.AccountCode);

                    return RedirectToAction("ContactList", new { accountCode = csm.Customers.AccountCode });
                }

            }
            catch
            {
                model.ErrorMessage = "<div class=\"alert alert-soft-danger\" role=\"alert\"><center>There are no contacts</center></div>";
            }
            return View(model);

        }

        [HttpGet]
        public IActionResult Create(int customerID, string accountCode, string Businessname)
        {
            var csm = new CustomerSearchModel();

            csm.CustomerID = customerID;
            csm.AccountCode = accountCode;
            csm.CustomerName = Businessname;

            return View(csm);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CustomerSearchModel model, int CustomerID)
        {
            CustomerSearchModel csm = new CustomerSearchModel();

            if (model.CustomerAdditionalContacts.Cell == null)
            {
                model.CustomerAdditionalContacts.Cell = "0";
            }
            if (model.CustomerAdditionalContacts.Tel == null)
            {
                model.CustomerAdditionalContacts.Tel = "0";
            }
            if (model.CustomerAdditionalContacts.Email == null)
            {
                model.CustomerAdditionalContacts.Email = "0";
            }

            csm.CustomerAdditionalContacts = await CustomerAdditionalContactsService.GetCustomerAdditionalEmailCellTelByID(model.CustomerAdditionalContacts.Cell, model.CustomerAdditionalContacts.Tel, model.CustomerAdditionalContacts.Email, CustomerID);
            if (csm.CustomerAdditionalContacts.Cell == "False" && csm.CustomerAdditionalContacts.Tel == "False" && csm.CustomerAdditionalContacts.Email == "False")
            {
                model.CustomerAdditionalContacts.CustomerID = CustomerID;

                await CustomerAdditionalContactsService.InsertCustomerAdditionalContactsAsync(model.CustomerAdditionalContacts);

                string u = string.Format("/Customer/ContactList?accountCode={0}", model.AccountCode);

                return RedirectToAction("Message", "Home", new { type = Service.Utils.StringHelper.Types.SaveSuccess, url = u });


            }
            else
            {
                if (csm.CustomerAdditionalContacts.Cell == "True")
                {
                    model.Cell = "True";
                }
                if (csm.CustomerAdditionalContacts.Tel == "True")
                {
                    model.Tel = "True";
                }
                if (csm.CustomerAdditionalContacts.Email == "True")
                {
                    model.Email = "True";
                }
                return View(model);
            }
        }


        [HttpGet]
        public async Task<IActionResult> Update(int CustomerAdditionalContactID, string accountCode, string Businessname)
        {
            var csm = new CustomerSearchModel();
            csm.CustomerAdditionalContacts = await CustomerAdditionalContactsService.GetCustomerAdditionalContactsByID(CustomerAdditionalContactID);
            csm.AccountCode = accountCode;
            csm.CustomerName = Businessname;
            return View(csm);
        }

        [HttpPost]
        public async Task<IActionResult> Update(CustomerSearchModel model)
        {
            var csm = new CustomerSearchModel();
            string u = "";
            if (model.CustomerAdditionalContacts.Cell == null)
            {
                model.CustomerAdditionalContacts.Cell = "0";
            }
            if (model.CustomerAdditionalContacts.Tel == null)
            {
                model.CustomerAdditionalContacts.Tel = "0";
            }
            if (model.CustomerAdditionalContacts.Email == null)
            {
                model.CustomerAdditionalContacts.Email = "0";
            }
            csm.UserDetails = await CustomerAdditionalContactsService.GetCustomerAdditionalContactsByID(model.CustomerAdditionalContacts.CustomerAdditionalContactID);
            csm.CustomerAdditionalContacts = await CustomerAdditionalContactsService.GetCustomerAdditionalEmailCellTelByID(model.CustomerAdditionalContacts.Cell, model.CustomerAdditionalContacts.Tel, model.CustomerAdditionalContacts.Email, model.CustomerAdditionalContacts.CustomerID);
            if (csm.UserDetails.Cell == model.CustomerAdditionalContacts.Cell || csm.UserDetails.Tel == model.CustomerAdditionalContacts.Tel || csm.UserDetails.Email == model.CustomerAdditionalContacts.Email)
            {

                if (csm.UserDetails.Cell != model.CustomerAdditionalContacts.Cell || csm.UserDetails.Tel != model.CustomerAdditionalContacts.Tel || csm.UserDetails.Email != model.CustomerAdditionalContacts.Email)
                {
                    if (csm.UserDetails.Cell == model.CustomerAdditionalContacts.Cell)
                    {
                        model.Cell = "";
                        csm.CustomerAdditionalContacts.Cell = "False";
                    }
                    if (csm.UserDetails.Cell != model.CustomerAdditionalContacts.Cell)
                    {
                        model.Cell = "True";
                        if (csm.CustomerAdditionalContacts.Cell == "False")
                        {
                            model.Cell = "";
                        }
                    }
                    if (csm.UserDetails.Tel == model.CustomerAdditionalContacts.Tel)
                    {
                        model.Tel = "";
                        csm.CustomerAdditionalContacts.Tel = "False";
                    }
                    if (csm.UserDetails.Tel != model.CustomerAdditionalContacts.Tel)
                    {
                        model.Tel = "True";
                        if (csm.CustomerAdditionalContacts.Tel == "False")
                        {
                            model.Tel = "";
                        }
                    }
                    if (csm.UserDetails.Email == model.CustomerAdditionalContacts.Email)
                    {
                        model.Email = "";
                        csm.CustomerAdditionalContacts.Email = "False";
                    }
                    if (csm.UserDetails.Email != model.CustomerAdditionalContacts.Email)
                    {
                        model.Email = "True";
                        if (csm.CustomerAdditionalContacts.Email == "False")
                        {
                            model.Email = "";
                        }
                    }
                    if (csm.CustomerAdditionalContacts.Cell == "False" && csm.CustomerAdditionalContacts.Tel == "False" && csm.CustomerAdditionalContacts.Email == "False")
                    {
                        await CustomerAdditionalContactsService.UpdateCustomerAdditionalContactsAsync(model.CustomerAdditionalContacts);
                        csm.Customers = await CustomerService.GetCustomerAccountCodeByCustomerId(model.CustomerAdditionalContacts.CustomerID);
                        u = string.Format("/Customer/ContactList?accountCode={0}", csm.Customers.AccountCode);
                        return RedirectToAction("Message", "Home", new { type = Service.Utils.StringHelper.Types.UpdateSuccess, url = u });
                    }
                    else
                    {
                        return View(model);
                    }
                }
                await CustomerAdditionalContactsService.UpdateCustomerAdditionalContactsAsync(model.CustomerAdditionalContacts);
                csm.Customers = await CustomerService.GetCustomerAccountCodeByCustomerId(model.CustomerAdditionalContacts.CustomerID);
                u = string.Format("/Customer/ContactList?accountCode={0}", csm.Customers.AccountCode);
                return RedirectToAction("Message", "Home", new { type = Service.Utils.StringHelper.Types.UpdateSuccess, url = u });
            }


            if (csm.UserDetails.Cell == model.CustomerAdditionalContacts.Cell)
            {
                model.Cell = "";
                csm.CustomerAdditionalContacts.Cell = "False";
            }
            if (csm.UserDetails.Cell != model.CustomerAdditionalContacts.Cell)
            {
                model.Cell = "True";
                if (csm.CustomerAdditionalContacts.Cell == "False")
                {
                    model.Cell = "";
                }
            }
            if (csm.UserDetails.Tel == model.CustomerAdditionalContacts.Tel)
            {
                model.Tel = "";
                csm.CustomerAdditionalContacts.Tel = "False";
            }
            if (csm.UserDetails.Tel != model.CustomerAdditionalContacts.Tel)
            {
                model.Tel = "True";
                if (csm.CustomerAdditionalContacts.Tel == "False")
                {
                    model.Tel = "";
                }
            }
            if (csm.UserDetails.Email == model.CustomerAdditionalContacts.Email)
            {
                model.Email = "";
                csm.CustomerAdditionalContacts.Email = "False";
            }
            if (csm.UserDetails.Email != model.CustomerAdditionalContacts.Email)
            {
                model.Email = "True";
                if (csm.CustomerAdditionalContacts.Email == "False")
                {
                    model.Email = "";
                }
            }
            if (csm.CustomerAdditionalContacts.Cell == "False" && csm.CustomerAdditionalContacts.Tel == "False" && csm.CustomerAdditionalContacts.Email == "False")
            {
                await CustomerAdditionalContactsService.UpdateCustomerAdditionalContactsAsync(model.CustomerAdditionalContacts);
                csm.Customers = await CustomerService.GetCustomerAccountCodeByCustomerId(model.CustomerAdditionalContacts.CustomerID);
                u = string.Format("/Customer/ContactList?accountCode={0}", csm.Customers.AccountCode);
                return RedirectToAction("Message", "Home", new { type = Service.Utils.StringHelper.Types.UpdateSuccess, url = u });
            }
            else
            {
                return View(model);
            }

        }


        [HttpGet]
        public async Task<IActionResult> Delete(int CustomerAdditionalContactID, int CustomerID)
        {
            var csm = new CustomerSearchModel();
            csm.CustomerAdditionalContacts = await CustomerAdditionalContactsService.GetCustomerAdditionalContactsByID(CustomerAdditionalContactID);
            csm.Customers = await CustomerService.GetCustomerAccountCodeByCustomerId(CustomerID);
            return PartialView("Delete", csm);
        }

    }
}
