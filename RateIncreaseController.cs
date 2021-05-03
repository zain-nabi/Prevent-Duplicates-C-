using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Triton.CRM.Helper;
using Triton.CRM.Models;
using Triton.Service.Data;
using Triton.Service.Utils;

namespace Triton.CRM.Controllers
{
    [Authorize(Roles = "Service Consultant, Business Development Manager, Sales Administrator, Sales Director, National Sales Manager, Development Manager, Junior Developer")]
    public class RateIncreaseController : Controller
    {
        [HttpGet]
        public async Task<IActionResult> RateIncreases(int RateCycleID, string RateCycleText)
        {
            var rateIncreaseViewModel = new RateIncreaseViewModel()
            {
                RateIncreaseList = await RateIncreaseService.GetRateIncreasesPerCycle(RateCycleID),
                RateCycleID = RateCycleID,
                RateCycleText = RateCycleText
            };
            rateIncreaseViewModel.ViewRateIncreaseList = (rateIncreaseViewModel.RateIncreaseList.OrderByDescending(p => p.RateIncreaseID)).ToList();
            rateIncreaseViewModel.ViewRateIncreaseList.FirstOrDefault().isCompleted = true;
            return View(rateIncreaseViewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int RateCycleID, string RateCycleText)
        {
            var rateIncreaseViewModel = new RateIncreaseViewModel()
            {
                RateCycleList = new SelectList(await RateCycleService.GetAllRateCycles(), "RateCycleID", "Description"),
                RateCycleID = RateCycleID,
                RateCycleText = RateCycleText
            };
            return View(rateIncreaseViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Create(RateIncreaseViewModel model)
        {
            var rateIncreaseViewModel = new RateIncreaseViewModel();
            var rateIncreaseHelper = new RateIncreaseHelper();
            rateIncreaseViewModel.IsRateIncreaseExist = await RateIncreaseService.CheckIfRateIncreaseExist(model.RateIncrease.RateIncreasePeriod.ToString("yyyy-MM-dd"));
            if (rateIncreaseViewModel.IsRateIncreaseExist.RateIncreaseString == "False")
            {
                await RateIncreaseService.InsertRateIncreaseAsync(rateIncreaseHelper.InsertRateIncrease(model, User.GetUserId()));
                string RateCycleText = await rateIncreaseHelper.GetRateCycleDescription(model.RateIncrease.RateCycleID);
                return RedirectToAction("RateIncreases", "RateIncrease", new { RateCycleID = model.RateIncrease.RateCycleID, RateCycleText = RateCycleText});
            }
            else
            {
                if (rateIncreaseViewModel.IsRateIncreaseExist.RateIncreaseString == "True")
                {
                    model.RateIncreasePeriodExist = true;
                    model.RateIncreaseErrorMessage = "Rate Increase Period already exist";
                    model.RateCycleList = new SelectList(await RateCycleService.GetAllRateCycles(), "RateCycleID", "Description");
                }
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int RateIncreaseID, string RateCycleText)
        {
            var rateIncreaseViewModel = new RateIncreaseViewModel();
            rateIncreaseViewModel.RateIncreaseByID = await RateIncreaseService.GetRateIncreaseByID(RateIncreaseID);
            rateIncreaseViewModel.RateCycleList = new SelectList(await RateCycleService.GetAllRateCycles(), "RateCycleID", "Description");
            rateIncreaseViewModel.RateCycleText = RateCycleText;
            return View(rateIncreaseViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(RateIncreaseViewModel model)
        {
            var rateIncreaseViewModel = new RateIncreaseViewModel();
            var rateIncreaseHelper = new RateIncreaseHelper();
            string redirectUrl = "";
            string RateCycleText = await rateIncreaseHelper.GetRateCycleDescription(model.RateIncreaseByID.RateCycleID);
            rateIncreaseViewModel.RateIncreaseByID = await RateIncreaseService.GetRateIncreaseByID(model.RateIncreaseByID.RateIncreaseID);
            rateIncreaseViewModel.IsRateIncreaseExist = await RateIncreaseService.CheckIfRateIncreaseExist(model.RateIncreaseByID.RateIncreasePeriod.ToString("yyyy-MM-dd"));
            if (rateIncreaseViewModel.RateIncreaseByID.RateIncreasePeriod == model.RateIncreaseByID.RateIncreasePeriod)
            {

                if (rateIncreaseViewModel.RateIncreaseByID.RateIncreasePeriod != model.RateIncreaseByID.RateIncreasePeriod)
                {
                    if (rateIncreaseViewModel.RateIncreaseByID.RateIncreasePeriod == model.RateIncreaseByID.RateIncreasePeriod)
                    {
                        model.RateIncreasePeriodExist = false;
                        rateIncreaseViewModel.IsRateIncreaseExist.RateIncreaseString = "False";
                    }
                    if (rateIncreaseViewModel.RateIncreaseByID.RateIncreasePeriod != model.RateIncreaseByID.RateIncreasePeriod)
                    {
                        model.RateIncreasePeriodExist = true;
                        model.RateIncreaseErrorMessage = "Rate Increase Period already exist";
                        if (rateIncreaseViewModel.IsRateIncreaseExist.RateIncreaseString == "False")
                        {
                            model.RateIncreasePeriodExist = false;
                        }
                    }
                    if (rateIncreaseViewModel.IsRateIncreaseExist.RateIncreaseString == "False")
                    {
                        await RateIncreaseService.UpdateRateIncreaseAsync(rateIncreaseHelper.UpdateRateIncrease(model));                        
                        redirectUrl = string.Format("/RateIncrease/RateIncreases?RateCycleID={0}&RateCycleText={1}", model.RateIncreaseByID.RateCycleID, RateCycleText);
                        return RedirectToAction("Message", "Home", new { type = Service.Utils.StringHelper.Types.UpdateSuccess, url = redirectUrl });
                    }
                    else
                    {
                        model.RateCycleList = new SelectList(await RateCycleService.GetAllRateCycles(), "RateCycleID", "Description");
                        return View(model);
                    }
                }
                await RateIncreaseService.UpdateRateIncreaseAsync(rateIncreaseHelper.UpdateRateIncrease(model));
                redirectUrl = string.Format("/RateIncrease/RateIncreases?RateCycleID={0}&RateCycleText={1}", model.RateIncreaseByID.RateCycleID, RateCycleText);
                return RedirectToAction("Message", "Home", new { type = Service.Utils.StringHelper.Types.UpdateSuccess, url = redirectUrl });
            }


            if (rateIncreaseViewModel.RateIncreaseByID.RateIncreasePeriod == model.RateIncreaseByID.RateIncreasePeriod)
            {
                model.RateIncreasePeriodExist = false;
                rateIncreaseViewModel.IsRateIncreaseExist.RateIncreaseString = "False";
            }
            if (rateIncreaseViewModel.RateIncreaseByID.RateIncreasePeriod != model.RateIncreaseByID.RateIncreasePeriod)
            {
                model.RateIncreasePeriodExist = true;
                model.RateIncreaseErrorMessage = "Rate Increase Period already exist";
                if (rateIncreaseViewModel.IsRateIncreaseExist.RateIncreaseString == "False")
                {
                    model.RateIncreasePeriodExist = false;
                }
            }
            if (rateIncreaseViewModel.IsRateIncreaseExist.RateIncreaseString == "False")
            {
                await RateIncreaseService.UpdateRateIncreaseAsync(rateIncreaseHelper.UpdateRateIncrease(model));
                redirectUrl = string.Format("/RateIncrease/RateIncreases?RateCycleID={0}&RateCycleText={1}", model.RateIncreaseByID.RateCycleID, RateCycleText);
                return RedirectToAction("Message", "Home", new { type = Service.Utils.StringHelper.Types.UpdateSuccess, url = redirectUrl });
            }
            else
            {
                model.RateCycleList = new SelectList(await RateCycleService.GetAllRateCycles(), "RateCycleID", "Description");
                return View(model);
            }
        }


        [HttpGet]
        public async Task<IActionResult> Delete(int RateIncreaseID)
        {
            var rateIncreaseViewModel = new RateIncreaseViewModel();
            rateIncreaseViewModel.RateIncreaseByID = await RateIncreaseService.GetRateIncreaseByID(RateIncreaseID);
            return PartialView("Delete", rateIncreaseViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(RateIncreaseViewModel model)
        {
            var rateIncreaseHelper = new RateIncreaseHelper();
            var rateIncreaseViewModel = new RateIncreaseViewModel();
            rateIncreaseViewModel.RateIncreaseByID = await RateIncreaseService.GetRateIncreaseByID(model.RateIncreaseByID.RateIncreaseID);
            await RateIncreaseService.UpdateRateIncreaseAsync(rateIncreaseHelper.DeleteRateIncrease(model, User.GetUserId()));
            return RedirectToAction("RateIncreases", "RateIncrease",new { RateCycleID = model.RateIncreaseByID.RateCycleID, RateCycleText = rateIncreaseViewModel.RateIncreaseByID.ShortDescription});
        }
    }
}
