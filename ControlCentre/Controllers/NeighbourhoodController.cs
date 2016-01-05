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
using System.Net;

namespace ControlCentre.Controllers {

    [EstablishContextActionFilter]
    public class NeighbourhoodController : Controller {

        public ActionResult Index() {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> ContactableNeighbours(QueryModel model) {
            return this.Json(new { machines = await CommsService.Ping(model.Machines, model.Timeout) });
        }

        [HttpPost]
        public ActionResult ApparentNeighbours(ProbeQueryModel model) {
            return this.Json(new { machines = CommsService.VisibleComputers(model.Scope.IsNull() || model.Scope.ToLowerInvariant().Contains("work")) });
        }

        [HttpPost]
        public async Task<ActionResult> QueryMachines(QueryModel model) {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            var result = this.Json(new { machines = await CommsService.Query(model.Machines, true) });
            watch.Stop();
            LogFacade.Instance.LogDebug("QueryMachines executed in " + watch.ElapsedMilliseconds + " ms");
            return result;
        }

        [HttpPost]
        public ActionResult RenderInEligible(EligibilityRequestModel model) {
            return this.Json(new { success = CommsService.RenderInEligible(model.Name) });
        }

        [HttpPost]
        public ActionResult RenderEligible(EligibilityRequestModel model) {
            return this.Json(new { success = CommsService.RenderEligible(model.Name) });
        }

        [HttpPost]
        public async Task<ActionResult> Analyze(TargetRequestModel target) {
            return this.Json(new { result = await CommsService.Analyze(Builder.Container.AsContainer(), target.Name) });
        }

        [HttpGet]
        public ActionResult GenerateCustomHeaders() {
            string sourceHost = Request.IsLocal ? Builder.Resolve<INetworkEnvironment>().HostName : Request.UserHostName.ToLowerInvariant();
            return this.Json(new { result = SecurityService.EncodedFrameDirectivesFor(sourceHost) }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<ActionResult> ConfigurationOffer(ConfigurationOfferModel model) {
            await CommsService.OfferConfiguration(model.ConfigurationTargets, model.StatedNexus);
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        private ICommunicationsService CommsService { get { return Builder.Resolve<ICommunicationsService>(); } }

        private ISecurityService SecurityService { get { return Builder.Resolve<ISecurityService>(); } }

    }

}