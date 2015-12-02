#region License
//
// Copyright Tony Beveridge 2013-2015. All rights reserved. 
// This source code and any derivations can only be used with the express written permission of the author.
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
                catch (Exception e) {
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
					result = (TObject) new BinaryFormatter().Deserialize(stream);
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

        public static async Task<string> ReadAll(this NetworkStream stream) {
            byte[] readBuffer = new byte[ReadSize];
            string result = null;
            using (var writer = new MemoryStream()) {
                do {
                    int numberOfBytesRead = await stream.ReadAsync(readBuffer, 0, readBuffer.Length).ConfigureAwait(false);
                    if (numberOfBytesRead > 0) 
                        writer.Write(readBuffer, 0, numberOfBytesRead);
                } while (stream.DataAvailable);
                result = Encoding.UTF8.GetString(writer.ToArray());
            }
            return result;
        }

    }

}
