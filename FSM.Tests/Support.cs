namespace FSM.Tests {

    public class EmptyContext : IMinimalContext {
        public string HostName { get; set; }
        public long NodeId { get; set; }
    }

    public class EmptyState : BaseState<EmptyContext> {

    }

    public class FaultingState : BaseState<EmptyContext> {

    }
}
