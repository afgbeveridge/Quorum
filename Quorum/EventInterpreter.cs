#region License
//
// Copyright Tony Beveridge 2015. All rights reserved. 
// MIT license applies.
//
#endregion
using System;
using System.Collections.Generic;
using FSM;
using Quorum.Integration;
using Quorum.Payloads;
using Quorum.States;

namespace Quorum {

    public class EventInterpreter : IEventInterpreter<IExecutionContext> {

        // Requests that target a directly executable aspect
        private static readonly Dictionary<string, Type> ExecMap = new Dictionary<string, Type> { 
            { typeof(QueryRequest).Name, typeof(QueryState) },
            { typeof(OutOfBandDiscoveryRequest).Name, typeof(DiscoveryState) }
        };

        // Requests that target state changes directed through the state machine
        private static readonly Dictionary<string, string> TypeNameMap = new Dictionary<string, string> { 
            { typeof(DeathAnnouncement).Name, EventNames.NeighbourDying },
            { typeof(AbdicationState).Name, EventNames.Abdication },
            { typeof(PretenderState).Name, EventNames.MakePretender },
            { typeof(DeathState).Name, EventNames.Die },
            { typeof(ConfigurationBroadcast).Name, EventNames.ConfigurationOffered }
        }; 

        public IPayloadParser Parser { get; set; }

        // Get dynamic Json object, get TypeHint, look up
        public InterpreterResult<IExecutionContext> TranslateToAction(object inbound) {
            var dyna = Parser.As<dynamic>(inbound);
            string hint = dyna.TypeHint;
            InterpreterResult<IExecutionContext> result = null;
            if (hint != null)
                result = IsSimple(hint) ?? IsExecutable(hint);
            return result;
        }

        private InterpreterResult<IExecutionContext> IsSimple(string hint) { 
            return TypeNameMap.ContainsKey(hint) ? new InterpreterResult<IExecutionContext> { EventName = TypeNameMap[hint] } : null;
        }

        private InterpreterResult<IExecutionContext> IsExecutable(string hint) {
            var execType = ExecMap.ContainsKey(hint) ? ExecMap[hint] : null;
            return
                execType == null ? 
                null : 
                new InterpreterResult<IExecutionContext> {
                    EventName = hint,
                    ExecutableStateType = execType
            };
        }
    }
}
