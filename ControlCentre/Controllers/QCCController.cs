using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Quorum;

namespace ControlCentre.Controllers {

    public class QCCController : Controller {
        
        public ActionResult Configuration() {
            return View();
        }

        public ActionResult SetTransportType(string type) {
            ActiveDisposition.AcceptTransportType(type);
            return this.Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

    }

}