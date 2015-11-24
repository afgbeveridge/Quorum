using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Quorum.Services;

namespace ControlCentre.Controllers {

    public class DiscoveryController : Controller {

        public ActionResult Index() {
            return View();
        }

        public ActionResult Neighbourhood() {
            return this.Json(new { machines = Service.VisibleComputers(true) }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult QueryMachines(IEnumerable<string> machines) {
            return this.Json(new { machines = Service.Query(machines) });
        }

        private IDiscoveryService Service { get { return Builder.Resolve<IDiscoveryService>(); } }

    }

}