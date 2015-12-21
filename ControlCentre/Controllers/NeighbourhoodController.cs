#region License
//
// Copyright Tony Beveridge 2015. All rights reserved. 
// MIT license applies.
//
#endregion
using System.Web.Mvc;
using Quorum;
using Quorum.Services;
using Infra;
using ControlCentre.Models;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ControlCentre.Controllers {

    public class NeighbourhoodController : Controller {

        public ActionResult Index() {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> ContactableNeighbours(QueryModel model) {
            EstablishContext(model);
            return this.Json(new { machines = await Service.Ping(model.Machines, model.Timeout) }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ApparentNeighbours(ProbeQueryModel model) {
            EstablishContext(model);
            return this.Json(new { machines = Service.VisibleComputers(model.Scope.IsNull() || model.Scope.ToLowerInvariant().Contains("work")) });
        }

        [HttpPost]
        public async Task<ActionResult> QueryMachines(QueryModel model) {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            EstablishContext(model);
            var result = this.Json(new { machines = await Service.Query(model.Machines, true) });
            watch.Stop();
            LogFacade.Instance.LogDebug("QueryMachines executed in " + watch.ElapsedMilliseconds + " ms");
            return result;
        }

        [HttpPost]
        public ActionResult RenderInEligible(TargetRequestModel model) {
            EstablishContext(model);
            return this.Json(new { success = Service.RenderInEligible(model.Name) });
        }

        [HttpPost]
        public ActionResult RenderEligible(TargetRequestModel model) {
            EstablishContext(model);
            return this.Json(new { success = Service.RenderEligible(model.Name) });
        }

        private ICommunicationsService Service { get { return Builder.Resolve<ICommunicationsService>(); } }

        private void EstablishContext(BaseRequestModel model) {
            var cfg = Builder.Resolve<IConfiguration>();
            cfg.LocalSet(Constants.Configuration.ExternalEventListenerPort.Key, model.Port);
            // Take only a slice, allow for transport cost and time
            cfg.LocalSet(Constants.Configuration.ResponseLimit.Key, (int) (model.Timeout * 0.8));
            ActiveDisposition.Initialise(model.TransportType);
        }

    }

}