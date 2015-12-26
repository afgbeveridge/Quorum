using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Permissions;

namespace Infra {

    public static class Crypto {

        [StorePermission(SecurityAction.Demand, EnumerateStores = true, OpenStore = true, EnumerateCertificates = true)]
        public static X509Certificate2 GetCertificate(StoreName storeName, StoreLocation storeLocation, Func<X509Certificate2, bool> certFinder) {
            DBC.True(certFinder.IsNotNull(), () => "Must supply a certificate finder");
            X509Store store = new X509Store(storeName, storeLocation);
            store.Open(OpenFlags.ReadOnly);
            X509Certificate2 cert = store.Certificates.Cast<X509Certificate2>().FirstOrDefault(certFinder);
            store.Close();
            DBC.False(cert.IsNull(), () => string.Format("No certificate matching the request found in {0}/{1}", storeLocation, storeName));
            return cert;
        }

    }
}
