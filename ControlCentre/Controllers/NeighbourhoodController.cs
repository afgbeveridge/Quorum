using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Quorum;
using Quorum.Services;
using Infra;
using ControlCentre.Models;
using ControlCentre.Filters;

namespace ControlCentre.Controllers {

    public class NeighbourhoodController : Controller {

        public ActionResult Index() {
            return View();
        }

        public ActionResult ApparentNeighbours() {
            return this.Json(new { machines = Service.VisibleComputers(true) }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult QueryMachines(QueryModel model) {
            EstablishContext(model);
            return this.Json(new { machines = Service.Query(model.Machines, true) });
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
            cfg.LocalSet(Constants.Configuration.ResponseLimit.Key, model.Timeout);
            ActiveDisposition.Initialise(model.TransportType);
        }

    }

}