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
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Threading;

namespace Infra {

    public static class ObjectExtensions {

        public static T Fluently<T>(this T src, Action<T> action) {
            action(src);
            return src;
        }

        public static bool IsNull(this object src) {
            return src == null;
        }

        public static bool IsNotNull(this object src) {
            return src != null;
        }

        public static void GuardedExecution(this object src, Action a, Action<Exception> onFailure = null) {
            try {
                a();
            }
            catch (Exception ex) {
                //Diagnostics.LogException(typeof(ObjectExtensions), "Guarded execution failed", ex);
                try {
                    if (onFailure.IsNotNull()) onFailure(ex);
                }
                catch (Exception) {
                    // Diagnostics.LogException(typeof(ObjectExtensions), "Guarded execution error handler failed (!)", e);
                }
            }
        }

        public static string Serialize(this object src) {
            BinaryFormatter formatter = new BinaryFormatter();
            using (MemoryStream stream = new MemoryStream()) {
                try {
                    formatter.Serialize(stream, src);
                }
                catch (SerializationException ex) {
                    throw new InvalidOperationException("The object graph could not be serialized", ex);
                }
                stream.Seek(0L, SeekOrigin.Begin);
                return Convert.ToBase64String(stream.ToArray());
            }
        }

        public static TObject Deserialize<TObject>(this object src, byte[] obj) {
            TObject result = default(TObject);
            using (MemoryStream stream = new MemoryStream(obj)) {
                try {
                    result = (TObject)new BinaryFormatter().Deserialize(stream);
                }
                catch (SerializationException ex) {
                    throw new InvalidOperationException("The object graph could not be deserialized", ex);
                }
                return result;
            }
        }
    }

    public static class EnumerableExtensions {

        public static void ForEach<T>(this IEnumerable<T> src, Action<T> action) {
            foreach (T obj in src)
                action(obj);
        }

        public static void ForEach<T>(this IEnumerable<T> src, Action<int, T> action) {
            int idx = 0;
            foreach (T obj in src)
                action(idx++, obj);
        }

        public static IEnumerable<IEnumerable<T>> Chunk<T>(this IEnumerable<T> source, int chunksize) {
            while (source.Any()) {
                yield return source.Take(chunksize);
                source = source.Skip(chunksize);
            }
        }

    }

    public static class NetworkStreamExtensions {

        private const int ReadSize = 512;
        private const int Cycle = 32;

        public static async Task<string> ReadAll(this NetworkStream stream, Func<NetworkStream, Task<int?>> frameSizer) {
            return await ReadAll(stream, await frameSizer(stream));
        }

        public static async Task<string> ReadAll(this NetworkStream stream, int? frameLength) {
            byte[] readBuffer = new byte[ReadSize];
            string result = null;
            if (frameLength.HasValue) {
                int toRead = frameLength.Value;
                using (var writer = new MemoryStream()) {
                    do {
                        int numberOfBytesRead = await stream.ReadAsync(readBuffer, 0, readBuffer.Length).ConfigureAwait(false);
                        if (numberOfBytesRead > 0) {
                            writer.Write(readBuffer, 0, numberOfBytesRead);
                            toRead -= numberOfBytesRead;
                        }
                    } while (toRead > 0);
                    result = Encoding.UTF8.GetString(writer.ToArray());
                }
            }
            return result;
        }

        public static async Task<byte[]> ReadExactly(this NetworkStream stream, int numBytes) {
            byte[] readBuffer = new byte[numBytes];
            int requested = numBytes, cycles = Cycle;
            LogFacade.Instance.LogDebug("Asked to read exactly " + numBytes + " byte(s)");
            LogFacade.Instance.LogDebug("Stream, DA? " + stream.DataAvailable + ", readable? " + stream.CanRead);
            do {
                int numberOfBytesRead = await stream.ReadAsync(readBuffer, requested - numBytes, numBytes).ConfigureAwait(false);
                if (numberOfBytesRead > 0)
                    numBytes -= numberOfBytesRead;
                cycles--;
            } while (numBytes > 0 && cycles > 0);
            LogFacade.Instance.LogDebug("Finished read exactly: read byte(s) => " + (requested - numBytes) + ", cycles remaining: " + cycles);
            return readBuffer;
        }

    }

    public static class WaitHandleExtensions {

        public static async Task<bool> WaitOneAsync(this WaitHandle handle, int millisecondsTimeout, CancellationToken cancellationToken = default(CancellationToken)) {
            RegisteredWaitHandle registeredHandle = null;
            CancellationTokenRegistration tokenRegistration = default(CancellationTokenRegistration);
            bool hasCancellationToken = cancellationToken != default(CancellationToken);
            try {
                var tcs = new TaskCompletionSource<bool>();
                registeredHandle = ThreadPool.RegisterWaitForSingleObject(
                    handle,
                    (state, timedOut) => ((TaskCompletionSource<bool>)state).TrySetResult(!timedOut),
                    tcs,
                    millisecondsTimeout,
                    true);
                if (hasCancellationToken)
                    tokenRegistration = cancellationToken.Register(
                        state => ((TaskCompletionSource<bool>)state).TrySetCanceled(),
                        tcs);
                return await tcs.Task;
            }
            finally {
                if (registeredHandle != null)
                    registeredHandle.Unregister(null);
                if (hasCancellationToken)
                    tokenRegistration.Dispose();
            }
        }

        public static Task<bool> WaitOneAsync(this WaitHandle handle, TimeSpan timeout, CancellationToken cancellationToken) {
            return handle.WaitOneAsync((int)timeout.TotalMilliseconds, cancellationToken);
        }

        public static Task<bool> WaitOneAsync(this WaitHandle handle, CancellationToken cancellationToken) {
            return handle.WaitOneAsync(Timeout.Infinite, cancellationToken);
        }

    }

    public static class DateTimeExtensions {

        private static readonly DateTime _1970 = new DateTime(1970, 1, 1);

        public static double AsUNIXEpochMilliseconds(this DateTime src) {
            return (src - _1970).TotalMilliseconds;
        }

    }

}
