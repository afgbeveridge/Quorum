#region License
//
// Copyright Tony Beveridge 2015. All rights reserved. 
// MIT license applies.
//
#endregion
using System.Web.Mvc;
using Quorum;

namespace ControlCentre.Controllers {

    public class QCCController : Controller {
        
        public ActionResult Configuration() {
            return View();
        }

        public ActionResult Logging() {
            return View();
        }

        public ActionResult SetTransportType(string type) {
            ActiveDisposition.Initialise(type);
            return this.Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

    }

}