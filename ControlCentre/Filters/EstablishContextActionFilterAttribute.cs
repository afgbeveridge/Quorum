using Quorum;
using System.Collections.Generic;
using System.Linq;
using Infra;
using System.Web.Mvc;
using ControlCentre.Models;

namespace ControlCentre.Filters {

    public class EstablishContextActionFilterAttribute : ActionFilterAttribute {

        private const float TimeSlice = 0.8f;

        public override void OnActionExecuting(ActionExecutingContext filterContext) {
            var model = filterContext.ActionParameters.FirstOrDefault();
            if (! new KeyValuePair<string, object>().Equals(model)) {
                var val = model.Value as BaseRequestModel;
                val.IsNotNull().IfTrue(() => EstablishContext(val));
            }
            base.OnActionExecuting(filterContext);
        }

        private void EstablishContext(BaseRequestModel model) {
            var cfg = Builder.Resolve<IConfiguration>();
            if (model.Port > 0)
                cfg.LocalSet(Constants.Configuration.ExternalEventListenerPort.Key, model.Port);
            // Take only a slice, allow for transport cost and time
            cfg.LocalSet(Constants.Configuration.ResponseLimit.Key, (int)(model.Timeout * TimeSlice));
            ActiveDisposition.Initialise(model.TransportType);
        }

    }
}