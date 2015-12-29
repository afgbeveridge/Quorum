#region License
//
// Copyright Tony Beveridge 2015. All rights reserved. 
// MIT license applies.
//
#endregion
using System.Web.Mvc;
using Quorum.Services;
using Infra;
using ControlCentre.Models;
using System.Threading.Tasks;
using System.Diagnostics;
using ControlCentre.Filters;

namespace ControlCentre.Controllers {

    [EstablishContextActionFilter]
    public class NeighbourhoodController : Controller {

        public ActionResult Index() {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> ContactableNeighbours(QueryModel model) {
            return this.Json(new { machines = await Service.Ping(model.Machines, model.Timeout) }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ApparentNeighbours(ProbeQueryModel model) {
            return this.Json(new { machines = Service.VisibleComputers(model.Scope.IsNull() || model.Scope.ToLowerInvariant().Contains("work")) });
        }

        [HttpPost]
        public async Task<ActionResult> QueryMachines(QueryModel model) {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            var result = this.Json(new { machines = await Service.Query(model.Machines, true) });
            watch.Stop();
            LogFacade.Instance.LogDebug("QueryMachines executed in " + watch.ElapsedMilliseconds + " ms");
            return result;
        }

        [HttpPost]
        public ActionResult RenderInEligible(EligibilityRequestModel model) {
            return this.Json(new { success = Service.RenderInEligible(model.Name) });
        }

        [HttpPost]
        public ActionResult RenderEligible(EligibilityRequestModel model) {
            return this.Json(new { success = Service.RenderEligible(model.Name) });
        }

        [HttpPost]
        public async Task<ActionResult> Analyze(TargetRequestModel target) {
            return this.Json(new { result = await Service.Analyze(Builder.Container.AsContainer(), target.Name) });
        }

        [HttpPost]
        public ActionResult ConfigurationOffer(ConfigurationOfferModel model) {
            return this.Json(new { result = true });
        }

        private ICommunicationsService Service { get { return Builder.Resolve<ICommunicationsService>(); } }

    }

}