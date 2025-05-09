using System;

namespace SignDocs
{
    public class MockCryptoService
    {
        private MockCertificate certificate;

        public MockCryptoService()
        {
            certificate = new MockCertificate();
        }

        public bool LoadCertificate(string keyPath, string password) => true;

        public string GetSerialNumber() => certificate.SerialNumber;

        public string GetPolicy() => certificate.Policy;

        public string GetUsage() => certificate.Usage;

        public string GetName() => certificate.Name;

        public string GetGivenName() => certificate.GivenName;

        public bool ValidateExpirationDate() => true;

        public void SignData(bool attached, string path, ref string signature)
        {
            signature = attached ? GenerateBase64String(200000) : GenerateBase64String(1500);
        }

        private string GenerateBase64String(int count)
        {
            var random = new Random();
            var bytes = new byte[count];

            random.NextBytes(bytes);

            return Convert.ToBase64String(bytes);
        }
    }
}
