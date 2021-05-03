using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Triton.CRM.Models;
using Triton.Service.Data;
using Triton.Service.Data.Branch;

namespace Triton.CRM.Controllers
{
    public class ApplicationUsersController : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Users()
        {
            var userModel = new ApplicationUserModel { UsersList = await ClaimsService.GetUsers() };
            return View(userModel);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var userModel = new ApplicationUserModel()
            {
                Users = new Model.Applications.Tables.tblUsers() { Archive_User = "0", StatusID = 1 },
                BranchSelectList = new SelectList(await BranchServices.GetBranchByCompanyID(1), "BranchID", "BranchFullName"),
                DepartmentSelectList = new SelectList(await DepartmentService.GetAllActiveDepartments(1), "DepartmentID", "Department")
            };
            return View(userModel);
        }

        [HttpPost]
        public async Task<IActionResult> Create(ApplicationUserModel model)
        {
            var appmodel = new ApplicationUserModel
            {
                Users = await ClaimsService.GetUserNameByID(model.Users.UserName),
                User = await ClaimsService.GetEmailAddressIfExists(model.Users.EmailAddress)
            };

            if ((appmodel.Users.UserName == "False") && (appmodel.User.EmailAddress == "False"))
            {
                await ClaimsService.InsertUserAsync(model.Users);
                var redirectUrl = "/ApplicationUsers/Users";
                return RedirectToAction("Message", "Home", new { type = Service.Utils.StringHelper.Types.SaveSuccess, url = redirectUrl });
            }

            model.UserNameErrorMessage = "Username already exists";
            model.EmailAddressErrorMessage = "Email address already exists";
            model.Username = appmodel.Users.UserName;
            model.EmailAddress = appmodel.User.EmailAddress;
            model.Users = new Model.Applications.Tables.tblUsers() { Archive_User = "0", StatusID = 1 };
            model.BranchSelectList = new SelectList(await BranchServices.GetBranchByCompanyID(1), "BranchID", "BranchFullName");
            model.DepartmentSelectList = new SelectList(await DepartmentService.GetAllActiveDepartments(1), "DepartmentID", "Department");
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Update(int UserID)
        {
            var userModel = new ApplicationUserModel();
            userModel.Users = await ClaimsService.GetApplicationUserByID(UserID);
            userModel.BranchSelectList = new SelectList(await BranchServices.GetBranchByCompanyID(1), "BranchID", "BranchFullName");
            userModel.DepartmentSelectList = new SelectList(await DepartmentService.GetAllActiveDepartments(1), "DepartmentID", "Department");
            return View(userModel);
        }

        [HttpPost]
        public async Task<IActionResult> Update(ApplicationUserModel model)
        {
            string redirectUrl;
            var appmodel = new ApplicationUserModel();
            appmodel.UserDetails = await ClaimsService.GetApplicationUserByID(model.Users.UserID);
            appmodel.Users = await ClaimsService.GetUserNameByID(model.Users.UserName);
            appmodel.User = await ClaimsService.GetEmailAddressIfExists(model.Users.EmailAddress);
            if (appmodel.UserDetails.EmailAddress == model.Users.EmailAddress || appmodel.UserDetails.UserName == model.Users.UserName)
            {
                
                if (appmodel.UserDetails.EmailAddress != model.Users.EmailAddress || appmodel.UserDetails.UserName != model.Users.UserName)
                {
                    if (appmodel.UserDetails.EmailAddress == model.Users.EmailAddress)
                    {
                        model.EmailAddressErrorMessage = "";
                    }
                    if (appmodel.UserDetails.EmailAddress != model.Users.EmailAddress)
                    {
                        model.EmailAddressErrorMessage = Service.Utils.StringHelper.Html.EmailAddressErrorMessage;
                    }
                    if (appmodel.UserDetails.UserName == model.Users.UserName)
                    {
                        model.UserNameErrorMessage = "";
                    }
                    if (appmodel.UserDetails.UserName != model.Users.UserName)
                    {
                        model.UserNameErrorMessage = Service.Utils.StringHelper.Html.UserNameErrorMessage;
                    }
                    model.Username = appmodel.Users.UserName;
                    if(model.Username == "False")
                    {
                        await ClaimsService.UpdateUserAsync(model.Users);
                        redirectUrl = "/ApplicationUsers/Users";
                        return RedirectToAction("Message", "Home", new { type = Service.Utils.StringHelper.Types.UpdateSuccess, url = redirectUrl });
                    }
                    model.EmailAddress = appmodel.User.EmailAddress;
                    if (model.EmailAddress == "False")
                    {
                        await ClaimsService.UpdateUserAsync(model.Users);
                        redirectUrl = "/ApplicationUsers/Users";
                        return RedirectToAction("Message", "Home", new { type = Service.Utils.StringHelper.Types.UpdateSuccess, url = redirectUrl });
                    }
                    else
                    {
                        model.Users = new Model.Applications.Tables.tblUsers() { Archive_User = "0", StatusID = 1 };
                        model.BranchSelectList = new SelectList(await BranchServices.GetBranchByCompanyID(1), "BranchID", "BranchFullName");
                        model.DepartmentSelectList = new SelectList(await DepartmentService.GetAllActiveDepartments(1), "DepartmentID", "Department");
                        return View(model);
                    }
                }
                await ClaimsService.UpdateUserAsync(model.Users);
                redirectUrl = "/ApplicationUsers/Users";
                return RedirectToAction("Message", "Home", new { type = Service.Utils.StringHelper.Types.UpdateSuccess, url = redirectUrl });
            }


            if (appmodel.UserDetails.EmailAddress == model.Users.EmailAddress)
            {
                model.EmailAddressErrorMessage = "";
            }
            if (appmodel.UserDetails.EmailAddress != model.Users.EmailAddress)
            {
                model.EmailAddressErrorMessage = Service.Utils.StringHelper.Html.EmailAddressErrorMessage;
            }
            if (appmodel.UserDetails.UserName == model.Users.UserName)
            {
                model.UserNameErrorMessage = "";
            }
            if (appmodel.UserDetails.UserName != model.Users.UserName)
            {
                model.UserNameErrorMessage = Service.Utils.StringHelper.Html.UserNameErrorMessage;
            }
            model.Username = appmodel.Users.UserName;
            model.EmailAddress = appmodel.User.EmailAddress;
            if (model.Username == "False" && model.EmailAddress == "False")
            {
                await ClaimsService.UpdateUserAsync(model.Users);
                redirectUrl = "/ApplicationUsers/Users";
                return RedirectToAction("Message", "Home", new { type = Service.Utils.StringHelper.Types.UpdateSuccess, url = redirectUrl });
            }
            else
            {
                model.Users = new Model.Applications.Tables.tblUsers() { Archive_User = "0", StatusID = 1 };
                model.BranchSelectList = new SelectList(await BranchServices.GetBranchByCompanyID(1), "BranchID", "BranchFullName");
                model.DepartmentSelectList = new SelectList(await DepartmentService.GetAllActiveDepartments(1), "DepartmentID", "Department");
                return View(model);
            }
        }
    }
}
