﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSM {
    
    public interface IEventInstance {

        string EventName { get; set; }

        /// <summary>
        /// An id that connects the sender of the event to the result, as needs be. Could be nullable
        /// </summary>
        Guid Id { get; set; }

        object Payload { get; set; }

        object ResponseContainer { get; set; }

        string PreviousStateName { get; set; }

        bool NoQueue { get; set; }

        DateTime CreatedOn { get; }

    }
}
