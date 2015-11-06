using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSM {
    
    public class EventInstance : IEventInstance {

        public EventInstance() {
            Id = Guid.NewGuid();
        }

        public string EventName { get; set; }

        public Guid Id { get; set; }

        public static IEventInstance Create(string name, object payload = null) {
            return new EventInstance { EventName = name, Payload = payload };
        }

        public object Payload { get; set; }

        public object ResponseContainer { get; set; }

        public string PreviousStateName { get; set; }

        public bool NoQueue { get; set; }
    }
}
