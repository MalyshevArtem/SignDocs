using QRCoder;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;

namespace SignDocs.Signature
{
    public class QRSignature : ISignature
    {
        public string DocId { get; private set; }
        public string DocPath { get; private set; }
        public string Month { get; private set; }
        public string Type { get; private set; }
        public string User { get; private set; }
        public string Year { get; private set; }

        private string signature;

        private List<string> signatureChunks = new List<string>();

        public QRSignature(string docPath, string docId, string user, string signature)
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
            var qrFolder = Path.Combine(DocPath, Year, Month, Type, DocId, User, "QR");
            Directory.CreateDirectory(qrFolder);

            CreateSignatureChunks();
            await Task.Run(() => CreateQR());
        }


        private void CreateSignatureChunks()
        {
            int quotient = signature.Length / 2;
            int remainder = signature.Length % 2;

            signatureChunks.Add("<chunk>1</chunk><signature>" + signature.Substring(0, quotient + remainder) + "</signature>");
            signatureChunks.Add("<chunk>2</chunk><signature>" + signature.Substring(quotient + remainder) + "</signature>");
        }

        private void CreateQR()
        {
            for (int i = 0; i < signatureChunks.Count; i++)
            {
                using (var generator = new QRCodeGenerator())
                {
                    using (QRCodeData data = generator.CreateQrCode(signatureChunks[i], QRCodeGenerator.ECCLevel.Q))
                    {
                        using (var QRCode = new QRCode(data))
                        {
                            using (Bitmap img = QRCode.GetGraphic(2))
                            {
                                var qrPath = Path.Combine(DocPath, Year, Month, Type, DocId, User, "QR", $"{i.ToString()}.jpg");
                                img.Save(qrPath);
                            }
                        }
                    }
                }
            }
        }
    }
}
