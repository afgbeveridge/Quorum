#region License
//
// Copyright Tony Beveridge 2015. All rights reserved. 
// MIT license applies.
//
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using Castle.MicroKernel;
using Quorum.Integration;
using Quorum.Integration.Http;
using Quorum.Integration.Tcp;
using Infra;

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
            Types[TransportType.Http] = Types[TransportType.Https] = typeof(HttpWriteableChannel);
            Types[TransportType.Tcp] = Types[TransportType.Tcps] = typeof(TcpWriteableChannel);
        }

    }

    public class ReadableChannelSelector : TypeChoiceSelector<IReadableChannel> {

        public ReadableChannelSelector() {
            Types[TransportType.Http] = Types[TransportType.Https] = typeof(HttpReadableChannel);
            Types[TransportType.Tcp] = Types[TransportType.Tcps] = typeof(TcpReadableChannel);
        }

    }

    public class EventListenerSelector : TypeChoiceSelector<IExposedEventListener<IExecutionContext>> {

        public EventListenerSelector() {
            Types[TransportType.Http] = Types[TransportType.Https] = typeof(HttpExposedEventListener);
            Types[TransportType.Tcp] = Types[TransportType.Tcps] = typeof(TcpExposedEventListener);
        }

    }

    public class LocalStorageSelector : GenericHandlerSelector<IConfigurationOverrideStorage> {

        protected Dictionary<bool, Type> Types = new Dictionary<bool, Type>();

        public LocalStorageSelector() {
            Types[true] = typeof(TransientConfigurationOverrideStorage);
            Types[false] = typeof(StaticConfigurationOverrideStorage);
        }

        protected override Type RelevantType {
            get {
                return Types[ActiveDisposition.IsTransient];
            }
        }
    }

}
