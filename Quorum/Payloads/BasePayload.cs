#region License
//
// Copyright Tony Beveridge 2015. All rights reserved. 
// MIT license applies.
//
#endregion

namespace Quorum.Payloads {

    public class BasePayload {

        protected BasePayload() {
            TypeHint = GetType().Name;
        }

        public string TypeHint { get; set; }

    }
}
