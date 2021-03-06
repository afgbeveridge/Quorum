﻿#region License
//
// Copyright Tony Beveridge 2015. All rights reserved. 
// MIT license applies.
//
#endregion
using System;
using System.Net.Http;
using Infra;

namespace Quorum.Integration.Http {

    public class BaseHttpChannel {

        public BaseHttpChannel(INetworkEnvironment network, IConfiguration config) {
            Network = network;
            Config = config;
        }

        protected INetworkEnvironment Network { get; private set; }

        protected IConfiguration Config { get; private set; }

        protected ClientDetails CreateClient(string address, int timeoutMs, ActionDisposition disposition = ActionDisposition.Read) {
            LogFacade.Instance.LogDebug("Base http timeout set at ms = " + timeoutMs);
            var handler =
                !Config.Get(Constants.Configuration.ByPassCertificateValidation) ?
                new WebRequestHandler() :
                new WebRequestHandler { ServerCertificateValidationCallback = (sender, cert, chain, policyErrors) => true,
            };
            var details = new ClientDetails {
                Client = new HttpClient(handler) { Timeout = TimeSpan.FromMilliseconds(timeoutMs) }, 
                Address = new HttpAddressingScheme(Network) { Name = address }.Address
            };
            if (Modifier.IsNotNull()) 
                Modifier.PostCreate(details);
            return details;
        }

        public IHttpCommunicationsModifier Modifier { get; set; }

        protected HttpRequestMessage CreateRequest(string uri, HttpMethod method, HttpContent content) {
            var msg = new HttpRequestMessage {
                Content = content,
                Method = method,
                RequestUri = new Uri(uri)
            };
            if (Config.Get(Constants.Configuration.EmitCustomHeader)) {
                var header = Config.Get(Constants.Configuration.CustomHeader);
                msg.Headers.Add(header, Network.UniqueId.ToString());
                msg.Headers.Add(Config.Get(Constants.Configuration.HostNameHeader), Network.SeedForUniqueId);
            }
            if (Modifier.IsNotNull())
                Modifier.RequestPreWrite(msg); 
            return msg;
        }
    }

}
