using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using SignDocs.Signature;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace SignDocs
{
    public partial class Form1 : Form
    {
        private string _userName;

        private async Task CreateSignaturesAsync(ProgressBar progressBar)
        {
            _userName = GetKeyOwner();

            var docs = new XmlDocument();
            docs.Load(_docsPath);

            XmlNodeList IdNodes = docs.SelectNodes("//Id");
            XmlNodeList UsersCountNodes = docs.SelectNodes("//UsersCount");

            for (int i = 0; i < IdNodes.Count; i++)
            {
                var docId = IdNodes[i].InnerText;
                var docPath = $"{_docsFolder}{docId}.pdf";

                string signature = string.Empty;
                await Task.Run(() => _service.SignData(true, docPath, ref signature));

                DocSignature docSignature = new DocSignature(_docsFolder, docId, _userName, signature);
                await docSignature.CreateAsync();

                await Task.Run(() => _service.SignData(false, docPath, ref signature));

                QRSignature qrSignature = new QRSignature(_docsFolder, docId, _userName, signature);
                await qrSignature.CreateAsync();

                int QRsPerRow = CheckFileFormat(docId);
                int usersCount = Convert.ToInt32(UsersCountNodes[i].InnerText);

                if (CheckAllSigned(QRsPerRow, usersCount, qrSignature))
                {
                    QRAdder adder = new QRAdder(_docsFolder, docId, QRsPerRow);
                    await Task.Run(() => adder.Create());
                }

                SetProgressBarValue(progressBar);
            }

            _result = 1;
        }

        private string GetKeyOwner()
        {
            string name = _service.GetName();
            string givenName = _service.GetGivenName();

            if (givenName is null)
            {
                return Utilities.MakeCapitals(name);
            }

            return Utilities.MakeCapitals($"{name} {givenName}");
        }

        private int CheckFileFormat(string docId)
        {
            var docPath = $"{_docsFolder}{docId}.pdf";

            using (PdfDocument doc = PdfReader.Open(docPath, PdfDocumentOpenMode.ReadOnly))
            {
                PdfPage docPage = doc.Pages[0];

                if (docPage.Width.Point == 612.0 && docPage.Height.Point == 792.0)
                {
                    return 4;
                }
                
                if (docPage.Width.Point == 792.0 && docPage.Height.Point == 612.0)
                {
                    return 6;
                }

                var ex = new InvalidOperationException($"Unsupported PDF page dimensions.");
                ex.Data["DocId"] = docId;

                throw ex;
            }
        }

        private bool CheckAllSigned(int QRsPerRow, int usersCount, ISignature sign)
        {
            string namesPath = Path.Combine(_docsFolder, sign.Year, sign.Month, sign.Type, sign.DocId);

            string[] namesDirs = Directory.GetDirectories(namesPath);
            int QRsCount = 0;

            if (namesDirs.Length == usersCount)
            {
                for (int i = 0; i < namesDirs.Length; i++)
                {
                    QRsCount += Directory.GetFiles(namesDirs[i] + "\\QR").Length;
                }

                if (QRsCount / 2 == usersCount)
                {
                    return true;
                }
            }

            return false;
        }

        private void SetProgressBarValue(ProgressBar progressBar)
        {
            progressBar.Invoke(new Action(() => progressBar.Value += 1));
        }
    }
}
