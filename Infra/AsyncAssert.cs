using System;
using NUnit.Framework;
using System.Threading.Tasks;

namespace Infra {

    // https://gist.github.com/danielrbradley/6671613
    public static class AsyncAssert {
        /// <summary>
        /// Assert that an async method fails due to a specific exception.
        /// </summary>
        /// <typeparam name="T">Exception type expected</typeparam>
        /// <param name="asyncDelegate">Test async delegate</param>
        public static async Task Throws<T>(Func<Task> asyncDelegate) where T : Exception {
            try {
                await asyncDelegate();
                Assert.Fail("Expected exception of type: {0}", typeof(T));
            }
            catch (T) {
                // swallow this exception because it is expected
            }
            catch (AssertionException) {
                throw;
            }
            catch (Exception ex) {
                Assert.Fail("Expected exception of type: {0} but was: {1}", typeof(T), ex);
            }
        }
    }
}
