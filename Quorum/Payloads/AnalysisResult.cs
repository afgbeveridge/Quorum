namespace Quorum.Payloads {

    public class AnalysisResult : BasePayload {

        public string Protocol { get; set; }

        public bool Responded { get { return !string.IsNullOrEmpty(Protocol); } }

    }
}
