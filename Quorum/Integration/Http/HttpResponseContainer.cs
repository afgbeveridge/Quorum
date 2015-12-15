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
using System.Net;
using Infra;
using System.IO;

namespace Quorum.Integration.Http {
    
    public class HttpResponseContainer {

        public int Status { get; set; }

        public HttpListenerResponse Response { get; set; }

        public async Task WriteAsync(string content) {
            if (Response.IsNotNull()) {
                LogFacade.Instance.LogDebug("Response container dispatches: '" + content + "'");
                // Now interpret response
                // Construct a response. 
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(content);
                // Get a response stream and write the response to it.
                Response.ContentLength64 = buffer.Length;
                Response.StatusCode = Status;
                Stream output = Response.OutputStream;
                await output.WriteAsync(buffer, 0, buffer.Length);
                output.Close();
            }
        }
    }
}
