#region License
//
// Copyright Tony Beveridge 2015. All rights reserved. 
// MIT license applies.
//
#endregion
using System.Net.Http;

namespace Quorum.Integration.Http {
    
    public interface IHttpCommunicationsModifier {
        /// <summary>
        /// Called when an HttpClient has just been created
        /// </summary>
        /// <param name="details">the current details object</param>
        void PostCreate(ClientDetails details);
        /// <summary>
        /// Called just before a request message will be dispatched. This method will be called after any quorum custom 
        /// http headers have been inserted
        /// </summary>
        /// <param name="msg">the message that will be sent</param>
        void RequestPreWrite(HttpRequestMessage msg);

    }

}
