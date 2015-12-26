namespace ControlCentre.Models {

    public class EligibilityRequestModel : TargetRequestModel {
        public bool RenderIneligible { get; set; }
        public bool RenderEligible { get; set; }
    }
}