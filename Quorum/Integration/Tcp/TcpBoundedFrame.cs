#region License
//
// Copyright Tony Beveridge 2015. All rights reserved. 
// MIT license applies.
//
#endregion
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Infra;

namespace Quorum.Integration.Tcp {

    public class TcpBoundedFrame {

        private const char DirectiveLine = '|';
        private const char DirectiveSeparator = '=';

        static TcpBoundedFrame() {
            LengthBytes = new Configuration().WithAppropriateOverrides().Get(Constants.Configuration.TcpFrameSizeSpecificationLength);
            DirectiveFromContentSeparator = (char) 3;
        }

        public static int LengthBytes { get; set; }

        public static char DirectiveFromContentSeparator { get; set; }

        public async Task<int> FrameAndWrite(Stream stream, string content, string directives = "") {
            return await FrameAndWrite(stream, SizeUp(content, directives));
        }

        private async Task<int> FrameAndWrite(Stream stream, byte[] frame) {
            await stream.WriteAsync(frame, 0, frame.Length).ConfigureAwait(false);
            return frame.Length;
        }

        public static string FormDirective(string key, object value) {
            return key + "=" + value;
        }

        public static string CombineDirectives(params string[] directives) {
            return string.Join(new string(DirectiveLine, 1), directives);
        }

        public static TcpBundle Parse(string entireSet) {
            int idx = entireSet.IndexOf(DirectiveFromContentSeparator);
            TcpBundle result = new TcpBundle { Content = idx < 0 ? entireSet : entireSet.Substring(idx + 1) };
            if (idx > 0) {
                // for is x=y|a=b and so on
                result.Directives = 
                    entireSet
                    .Substring(0, idx)
                    .Split(DirectiveLine)
                    .Select(s => {
                        var split = s.Split(DirectiveSeparator);
                        return new { key = split.First().ToLowerInvariant(), value = split[1] };
                    })
                    .ToDictionary(t => t.key, t => t.value);
            }
            return result;
        }

        public byte[] SizeUp(string content, string directives = "") {
            var all = directives + (!string.IsNullOrEmpty(directives) ? new string(DirectiveFromContentSeparator, 1) : string.Empty) + content;
            return Encoding.ASCII.GetBytes(LengthBytes + all.Length.ToString().PadLeft(LengthBytes, '0') + all);
        }

        public static async Task<int?> DetermineFrameSize(Stream stream) {
            // First byte is size of length spec
            // Retained for possible future use
            LogFacade.Instance.LogDebug("Request to determine frame size");
            int? remaining = null;
            var lenSpec = await stream.ReadExactly(1);
            var toRead = (char)lenSpec[0] - '0';
            LogFacade.Instance.LogDebug("Reckoned length bytes to number " + toRead);
            var lenBytes = await stream.ReadExactly(toRead);
            remaining = int.Parse(Encoding.ASCII.GetString(lenBytes, 0, toRead));
            LogFacade.Instance.LogDebug("Determined frame size to be: " + remaining);
            return remaining;
        }

    }

    public class TcpBundle {
        public string Content { get; set; }
        public IDictionary<string, string> Directives { get; set; }
    }

}
