using System;
using System.IO;
using System.Threading.Tasks;

namespace SignDocs.Signature
{
    public class DocSignature : ISignature
    {
        public string DocPath { get; private set; }
        public string DocId { get; private set; }
        public string User { get; private set; }
        public string Type { get; private set; }
        public string Year { get; private set; }
        public string Month { get; private set; }

        private string signature;

        public DocSignature(string docPath, string docId, string user, string signature)
        {
            DocPath = docPath;
            DocId = docId;
            User = user;
            this.signature = signature;

            Type = docId.Substring(0, 5);
            Year = docId.Substring(docId.Length - 8, 4);
            Month = docId.Substring(docId.Length - 4, 2);
        }

        public async Task CreateAsync()
        {
            var qrPath = Path.Combine(DocPath, Year, Month, Type, DocId, User, "QR");
            Directory.CreateDirectory(qrPath);

            byte[] signatureBytes = Convert.FromBase64String(signature);

            var signaturePath = Path.Combine(DocPath, Year, Month, Type, DocId, User, $"{User}.cms");

            using (var stream = new FileStream(signaturePath, FileMode.Create))
            {
                await stream.WriteAsync(signatureBytes, 0, signatureBytes.Length);
            }
        }
    }
}
