using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using Infra;

namespace Quorum.Integration.Tcp {

    public class TcpBoundedFrame {

        private const int LengthBytes = 6;

        public async Task<int> FrameAndWrite(NetworkStream stream, string content) {
            return await FrameAndWrite(stream, SizeUp(content));
        }

        public async Task<int> FrameAndWrite(NetworkStream stream, byte[] frame) {
            await stream.WriteAsync(frame, 0, frame.Length).ConfigureAwait(false);
            return frame.Length;
        }

        public byte[] SizeUp(string content) {
            return Encoding.ASCII.GetBytes(LengthBytes + content.Length.ToString().PadLeft(LengthBytes, '0') + content);
        }

        public static async Task<int?> DetermineFrameSize(NetworkStream stream) {
            // First byte is size of length spec
            // Retained for possible future use
            LogFacade.Instance.LogInfo("Request to determine frame size");
            int? remaining = null;
            var lenSpec = await stream.ReadExactly(1);
            var toRead = (char)lenSpec[0] - '0';
            LogFacade.Instance.LogWarning("Reckoned length bytes to number " + toRead);
            var lenBytes = await stream.ReadExactly(toRead);
            remaining = int.Parse(Encoding.ASCII.GetString(lenBytes, 0, toRead));
            LogFacade.Instance.LogInfo("Determined frame size to be: " + remaining);
            return remaining;
        }

    }

}
