using System.Collections.Generic;

namespace ControlCentre.Models {

    public class ConfigurationOfferModel : BaseRequestModel {

        public IEnumerable<string> StatedNexus { get; set; }

        public IEnumerable<string> ConfigurationTargets { get; set; }

    }
}