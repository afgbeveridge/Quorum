using System;
using System.Collections.Generic;
using System.Linq;
using Castle.MicroKernel;
using Quorum.Integration;
using Quorum.Integration.Http;
using Quorum.Integration.Tcp;

namespace Quorum {

    public abstract class GenericHandlerSelector<TInterface> : IHandlerSelector {

        public bool HasOpinionAbout(string key, Type service) {
            return service == typeof(TInterface);
        }

        public IHandler SelectHandler(string key, Type service, IHandler[] handlers) {
            return handlers.Where(x => x.ComponentModel.Implementation == RelevantType).First();
        }

        protected abstract Type RelevantType { get; }

    }

    public class TypeChoiceSelector<TInterface> : GenericHandlerSelector<TInterface> {

        protected Dictionary<TransportType, Type> Types = new Dictionary<TransportType, Type>();

        protected override Type RelevantType {
            get {
                return Types[ActiveDisposition.Current];
            } 
        }
    }

    public class WriteableChannelSelector : TypeChoiceSelector<IWriteableChannel> {

        public WriteableChannelSelector() {
            Types[TransportType.Http] = typeof(HttpWriteableChannel);
            Types[TransportType.Tcp] = typeof(TcpWriteableChannel);
        }

    }

    public class ReadableChannelSelector : TypeChoiceSelector<IReadableChannel> {

        public ReadableChannelSelector() {
            Types[TransportType.Http] = typeof(HttpReadableChannel);
            Types[TransportType.Tcp] = typeof(TcpReadableChannel);
        }

    }

    public class EventListenerSelector : TypeChoiceSelector<IExposedEventListener<IExecutionContext>> {

        public EventListenerSelector() {
            Types[TransportType.Http] = typeof(HttpExposedEventListener);
            Types[TransportType.Tcp] = typeof(TcpExposedEventListener);
        }

    }

}
