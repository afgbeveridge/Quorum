using System.Collections.Generic;

namespace ControlCentre.Models {

    public class ConfigurationOfferModel : BaseRequestModel {

        public IEnumerable<string> Nexus { get; set; }

        public IEnumerable<string> ConfigurationTargets { get; set; }

    }
}