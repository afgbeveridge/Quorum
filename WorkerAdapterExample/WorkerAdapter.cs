using System.Threading.Tasks;
using Quorum;
using Infra;

namespace WorkerAdapterExample {

    /// <summary>
    /// This type is the one by default referenced in the configuration of the Quorum Windows Service. If you replace the relevant parts below,
    /// and build the windows service project, your implementation will obviously be used.
    /// </summary>
    public class WorkerAdapter : BaseMasterWorkAdapter {

        /// <summary>
        /// When invoked, the receiver should execute a unit of work.
        /// </summary>
        /// <returns>false if the receiver indicates it has a fault and cannot continue</returns>
        protected override async Task<bool> Work() {
            await Task.Delay(10);
            WorkUnitExecuted();
            return ContinueExecuting;
        }

        /// <summary>
        /// Invoked when the receivers containing state machine is stopping, allowing the execution of clean up tasks.
        /// </summary>
        /// <returns>An awaitable task with no result</returns>
        protected override async Task Stopping() {
            LogFacade.Instance.LogInfo("Stopping...");
            await Task.Delay(10);
        }

        /// <summary>
        /// Invoked when the receiver is considered stopped
        /// </summary>
        protected override void Stopped() {
            LogFacade.Instance.LogInfo("Stopped.");
        }

    }
}

