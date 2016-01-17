using System;
using System.Collections.Generic;
using System.IO;
using FSM;
using Infra;
using Quorum;
using Quorum.Integration;
using Quorum.Services;
using System.Threading.Tasks;
using System.Linq;

namespace QuorumInteractionTool {

    public class Program {

        private const string ConfigurationFile = "quorum.config.json";
        private const string GeneralHelpFile = "qit.help.txt";

        public static void Main(string[] args) {
            try {
                DBC.True(File.Exists(ConfigurationFile), () => "The file " + ConfigurationFile + " must exist");
                Bootstrap();
                Configure();
                DBC.True(args.Any(), GeneralHelp);
                Execute(args.First());
            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Execution finished with errors");
                Environment.Exit(1);
            }
        }

        private static IContainer Container { get; set; }

        private static QuorumConfiguration CurrentConfiguration { get; set; }

        private static void Bootstrap() {
            Builder bldr = new Builder();
            bldr.CreateBaseRegistrations();
            Container = bldr.AsContainer();
            System.Reflection.Assembly
                .GetExecutingAssembly()
                .GetExportedTypes()
                .Where(t => t.GetInterfaces().Contains(typeof(ICommand)))
                .ToList()
                .ForEach(t => bldr.Register<ICommand>(t));
        }

        private static void Configure() {
            CurrentConfiguration = Container.Resolve<IPayloadParser>().As<QuorumConfiguration>(File.ReadAllText(ConfigurationFile));
            ActiveDisposition.AcceptTransportType(CurrentConfiguration.TransportType);
            IConfiguration config = Container.Resolve<IConfiguration>();
            config.LocalSet((ActiveDisposition.ExpectsEncryption ? Constants.Configuration.ExternalSecureEventListenerPort : Constants.Configuration.ExternalEventListenerPort).Key, CurrentConfiguration.Port);
            config.LocalSet(Constants.Configuration.ResponseLimit.Key, CurrentConfiguration.ResponseLimit);
        }

        private static void Execute(string shorthand) {
            var exec = new CommandExecutor(Container);
            DBC.True(exec.CanHandle(shorthand), () => "The option " + shorthand + " is not understood");
            var result = exec.Execute(CurrentConfiguration, Container).GetAwaiter().GetResult();
            Console.WriteLine(result);
        }

        private static string GeneralHelp() {
            var commandsText = string.Join(Environment.NewLine, Container.ResolveAll<ICommand>().Select(c => c.HelpText));
            return File.ReadAllText(GeneralHelpFile) + Environment.NewLine + (commandsText);
        }

    }

    public class QuorumConfiguration {
        public int Port { get; set; }
        public int ResponseLimit { get; set; }
        public string TransportType { get; set; }
        public string Members {
            set {
                Nexus = value.Split(',');
            }
        }
        public IEnumerable<string> Nexus { get; set; }
    }

    public class CommandExecutor {

        private ICommand Target { get; set; }

        private IContainer Container { get; set; }

        public CommandExecutor(IContainer container) {
            Container = container;
        }

        public bool CanHandle(string shorthand) {
            Target = Container.ResolveAll<ICommand>().FirstOrDefault(c => c.Understands(shorthand));
            return Target.IsNotNull();
        }

        public async Task<string> Execute(QuorumConfiguration cfg, IContainer container) {
            object result = await Target.Execute(cfg, container);
            return container.Resolve<IPayloadBuilder>().Create(new { Result = result });
        }

    } 

    public interface ICommand {
        bool Understands(string shorthand);
        Task<object> Execute(QuorumConfiguration cfg, IContainer container);
        string HelpText { get; }
    }

    public class QueryCommand : ICommand {

        public async Task<object> Execute(QuorumConfiguration cfg, IContainer container) {
            return await container.Resolve<ICommunicationsService>().Query(cfg.Nexus, true);
        }

        public bool Understands(string shorthand) {
            return shorthand == "query";
        }

        public string HelpText {
            get {
                return "query - query all defined quorum members";
            }
        }
    }

    public class PingCommand : ICommand {

        public async Task<object> Execute(QuorumConfiguration cfg, IContainer container) {
            return await container.Resolve<ICommunicationsService>().Ping(cfg.Nexus, cfg.ResponseLimit);
        }

        public bool Understands(string shorthand) {
            return shorthand == "ping";
        }

        public string HelpText {
            get {
                return "ping - ping all defined quorum members";
            }
        }
    }

    public class DiscoverCommand : ICommand {

        public async Task<object> Execute(QuorumConfiguration cfg, IContainer container) {
            return await container.Resolve<ICommunicationsService>().BroadcastDiscovery(cfg.Nexus, cfg.Nexus);
        }

        public bool Understands(string shorthand) {
            return shorthand == "discover";
        }

        public string HelpText {
            get {
                return "discover - ask quorum members to self validate";
            }
        }
    }

    public class NullCommand : ICommand {

        public Task<object> Execute(QuorumConfiguration cfg, IContainer container) {
            return Task.FromResult<object>(string.Empty);
        }

        public bool Understands(string shorthand) {
            return shorthand == "null";
        }

        public string HelpText {
            get {
                return "null - exit without action";
            }
        }
    }

}
