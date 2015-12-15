#region License
//
// Copyright Tony Beveridge 2015. All rights reserved. 
// MIT license applies.
//
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FSM;
using Quorum.Integration;
using Quorum.Payloads;
using System.Net;
using Infra;
using System.Diagnostics;

namespace Quorum.States {

    public class QueryState : BaseState<IExecutionContext> {

        public IPayloadBuilder Builder { get; set; }

        public IPayloadParser Parser { get; set; }

        public IWriteableChannel Channel { get; set; }

        public INetworkEnvironment Network { get; set; }

        public IConfiguration Configuration { get; set; }

        public override async Task<StateResult> OnEntry(IStateMachineContext<IExecutionContext> context) {
            return await Execute(context, context.CurrentEvent);
        }

        public override async Task<StateResult> Execute(IStateMachineContext<IExecutionContext> context, IEventInstance anEvent) {
            Stopwatch watch = new Stopwatch();
            try {
                Interruptable = false;
                watch.Start();
                var queryResponse = Neighbour.Fabricate(context);
                LogFacade.Instance.LogDebug("Created response body in ms = " + watch.ElapsedMilliseconds);
                /// Tell the world who we are
                var request = Parser.As<QueryRequest>(anEvent.Payload);
                int timeout = request.Timeout.HasValue ? request.Timeout.Value : Configuration.Get<int>(Constants.Configuration.ResponseLimit);
                LogFacade.Instance.LogDebug("Query timeout set at ms = " + timeout);
                // This is a wait as the caller is not guaranteed to be async possible
                await Channel.Respond(anEvent.ResponseContainer, 
                                Builder.Create(queryResponse), 
                                timeout);
            }
            catch { 
            }
            finally {
                watch.Stop();
                LogFacade.Instance.LogDebug("Query response in ms = " + watch.ElapsedMilliseconds);
            }
            // This is a bounce state, so we revert to the previous state
            return new StateResult { Revert = true };
        }

    }

}
