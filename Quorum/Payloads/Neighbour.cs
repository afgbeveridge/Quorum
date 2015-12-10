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
using Infra;
using System.Diagnostics;

namespace Quorum.Payloads {

    public enum MachineStrength {  Compute, IO, Memory }

    public class Neighbour : BasePayload {

        public bool IsMaster { get; set; }

        public bool IsValid { 
            get {
                return Hardware.IsNotNull();
            } 
        }

        public bool InEligibleForElection { get; set; }

        public string Name { get; set; }

        // Could be extended to allow heterogeneous addressing
        public string ContactAddress { get; set; }

        // Other qualities

        public double UpTime { get; set; }

        public double AbsoluteBootTime { get; set; }

        public double? TimeUntilRestart { get; set; }

        public long? LastRequestElapsedTime { get; set; }

        public string Strength { get; set; }

        public HardwareDetails Hardware { get; set; }

        public IEnumerable<PendingEvent> PendingEvents { get; set; }

        public IEnumerable<IEventSummary> HandledEvents { get; set; }

        public long WorkUnitsExecuted { get; set; }

        public static Neighbour Fabricate(IStateMachineContext<IExecutionContext> context) {
                var strength = new Configuration().Get<string>(Constants.Configuration.MachineStrength);
                var actualStrength = String.IsNullOrEmpty(strength) ? MachineStrength.Compute : (MachineStrength)Enum.Parse(typeof(MachineStrength), strength, true);
                return new Neighbour {
                    IsMaster = context.ExecutionContext.IsMaster,
                    Name = context.ExecutionContext.HostName,
                    UpTime = context.EnclosingMachine.UpTime,
                    AbsoluteBootTime = context.EnclosingMachine.AbsoluteBootTime,
                    Strength = actualStrength.ToString(),
                    // Could be null if not yet interrogated
                    Hardware = HardwareDetails.Instance ?? new HardwareDetails(),
                    InEligibleForElection = context.ExecutionContext.InEligibleForElection,
                    PendingEvents = context.EnclosingMachine.PendingEvents.Select(e => new PendingEvent {  Id = e.Id, Name = e.EventName, CreatedOn = e.CreatedOn, AgeInSeconds = (DateTime.Now  - e.CreatedOn).TotalSeconds}).ToArray(),
                    HandledEvents = context.EnclosingMachine.StatisticsHandler.HandledEventsInNameOrder.ToArray(),
                    WorkUnitsExecuted = context.ExecutionContext.WorkerExecutionUnits
                };
        }

        public static Neighbour NonRespondingNeighbour(string name) {
            return new Neighbour { Name = name };
        }

    }

    public class HardwareDetails {

        public static HardwareDetails Instance { get; private set; }

        internal HardwareDetails() {
        }

        public string PhysicalMemory { get; set; }
        
        public string CPUManufacturer { get; set; }
        
        public string CPUSpeed { get; set; }
        
        public string OS { get; set; }

        public static HardwareDetails Interrogate() {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            Instance = new HardwareDetails {
                PhysicalMemory = HardwareInfo.GetPhysicalMemory(),
                CPUManufacturer = HardwareInfo.GetCPUManufacturer(),
                CPUSpeed = HardwareInfo.GetCpuSpeedInGHz().HasValue ? HardwareInfo.GetCpuSpeedInGHz().Value.ToString() : "Unknown",
                OS = HardwareInfo.GetOSInformation()
            };
            LogFacade.Instance.LogInfo("Interrogated hardware in ms = " + watch.ElapsedMilliseconds);
            return Instance;
        }
    }

}
