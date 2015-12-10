#region License
//
// Copyright Tony Beveridge 2015. All rights reserved. 
// MIT license applies.
//
#endregion
using Newtonsoft.Json;

namespace Quorum.Integration {

    public class JsonPayloadBuilder : IPayloadBuilder {

        public string Create<TType>(TType obj) {
            return JsonConvert.SerializeObject(obj, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
        }
    }
}
