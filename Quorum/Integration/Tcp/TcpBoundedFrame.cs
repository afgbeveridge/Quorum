#region License
//
// Copyright Tony Beveridge 2015. All rights reserved. 
// MIT license applies.
//
#endregion
using System.Text;
using System.Threading.Tasks;
using System;
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
            DirectiveFromContentSeparator = new string((char)3, 1);
            FrameStartMarker = "~";
        }

        public static int LengthBytes { get; set; }

        public static string DirectiveFromContentSeparator { get; set; }

        private static string FrameStartMarker { get; set; }

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

        public static TcpBundle Parse(string unconverted) {
            var entireSet = Encoding.ASCII.GetString(Convert.FromBase64String(unconverted));
            int idx = entireSet.IndexOf(DirectiveFromContentSeparator);
            TcpBundle result = new TcpBundle { Content = idx < 0 ? entireSet : entireSet.EndsWith(DirectiveFromContentSeparator) ? String.Empty : entireSet.Substring(idx + 1) };
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
            return Encoding.ASCII.GetBytes(SizeUpString(content, directives));
        }

        public string SizeUpString(string content, string directives = "") {
            // We base64 the content to allow easier usage downstream for other potential consumers
            var unconverted = directives + (!string.IsNullOrEmpty(directives) ? DirectiveFromContentSeparator : string.Empty) + content;
            var all = Convert.ToBase64String(Encoding.ASCII.GetBytes(unconverted));
            LogFacade.Instance.LogDebug("Converted tcp send content: " + all);
            return FrameStartMarker + LengthBytes + all.Length.ToString().PadLeft(LengthBytes, '0') + all;
        }

        public static async Task<int?> DetermineFrameSize(Stream stream) {
            // 1st byte is frame start marker
            // 2nd byte is size of length spec
            // Retained for possible future use
            await EnsureFrameStartCorrect(stream);
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

        // Crude
        private static async Task EnsureFrameStartCorrect(Stream stream) {
            LogFacade.Instance.LogDebug("Require frame start marker");
            var fs = await stream.ReadExactly(FrameStartMarker.Length);
            DBC.True((char)fs[0] == FrameStartMarker[0], () => "No frame start marker present in TCP frame");
        }

        // If a complete frame is offered up, length bytes and all, try and decipher
        public static string DissectCompleteFrame(string src) {
            return src.Substring(LengthBytes + 1 + FrameStartMarker.Length);
        }

    }

    public class TcpBundle {
        public string Content { get; set; }
        public IDictionary<string, string> Directives { get; set; }
    }

}
