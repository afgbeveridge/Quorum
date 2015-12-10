#region License
//
// Copyright Tony Beveridge 2015. All rights reserved. 
// MIT license applies.
//
#endregion
using System;
using Newtonsoft.Json;

namespace Quorum.Integration {

    public class JsonPayloadParser : IPayloadParser {

        public TType As<TType>(object payload) {
            return JsonConvert.DeserializeObject<TType>(payload.ToString(), new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
        }

        public object As(object payload, Type t) {
            return JsonConvert.DeserializeObject(payload.ToString(), t, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
        }
    }
}
