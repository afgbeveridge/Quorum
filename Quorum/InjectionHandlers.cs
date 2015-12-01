using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.Windsor;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel;
using Quorum.Integration;
using Quorum.Integration.Http;
using Quorum.Integration.Tcp;

namespace Quorum {

    public class GenericHandlerSelector<TInterface> : IHandlerSelector {

        protected Dictionary<TransportType, Type> Types = new Dictionary<TransportType, Type>();

        public bool HasOpinionAbout(string key, Type service) {
            return service == typeof(TInterface);
        }

        public IHandler SelectHandler(string key, Type service, IHandler[] handlers) {
            return handlers.Where(x => x.ComponentModel.Implementation == Types[ActiveDisposition.Current]).First();
        }

    }

    public class WriteableChannelSelector : GenericHandlerSelector<IWriteableChannel> {

        public WriteableChannelSelector() {
            Types[TransportType.Http] = typeof(HttpWriteableChannel);
            Types[TransportType.Tcp] = typeof(TcpWriteableChannel);
        }

    }

    public class ReadableChannelSelector : GenericHandlerSelector<IReadableChannel> {

        public ReadableChannelSelector() {
            Types[TransportType.Http] = typeof(HttpReadableChannel);
            Types[TransportType.Tcp] = typeof(TcpReadableChannel);
        }

    }

    public class EventListenerSelector : GenericHandlerSelector<IExposedEventListener<IExecutionContext>> {

        public EventListenerSelector() {
            Types[TransportType.Http] = typeof(HttpExposedEventListener);
            Types[TransportType.Tcp] = typeof(TcpExposedEventListener);
        }

    }

}
