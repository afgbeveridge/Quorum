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

        public void Write(string content) {
            if (Response.IsNotNull()) {
                // Now interpret response
                // Construct a response. 
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(content);
                // Get a response stream and write the response to it.
                Response.ContentLength64 = buffer.Length;
                Response.StatusCode = Status;
                Stream output = Response.OutputStream;
                output.Write(buffer, 0, buffer.Length);
                output.Close();
            }
        }
    }
}
