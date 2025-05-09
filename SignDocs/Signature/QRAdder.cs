using PdfSharp.Drawing;
using PdfSharp;
using PdfSharp.Pdf;
using System.IO;
using PdfSharp.Pdf.IO;
using System;

namespace SignDocs.Signature
{
    public class QRAdder
    {
        private string docsFolder;
        private string docId;
        private string type;
        private string year;
        private string month;
        private int QRPerRow;

        private int hHeading;
        private int vHeading;
        private int hName;
        private int vName;
        private int hQR;
        private int vQR;
        private int QRDistance;

        private XFont fontHeading = new XFont("Arial", 10, XFontStyle.Bold);
        private XFont fontName = new XFont("Arial", 9);
        private XSolidBrush brush = XBrushes.Black;

        public QRAdder(string docsFolder, string docId, int QRPerRow)
        {
            this.docsFolder = docsFolder;
            this.docId = docId;
            this.QRPerRow = QRPerRow;

            type = docId.Substring(0, 5);
            year = docId.Substring(docId.Length - 8, 4);
            month = docId.Substring(docId.Length - 4, 2);
        }

        public void Create()
        {
            try
            {
                using (var lockDoc = new FileStream(docsFolder + docId + ".pdf", FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    using (PdfDocument pdf = PdfReader.Open(lockDoc))
                    {
                        CreateDocQR(pdf);

                        pdf.Save($"{docsFolder}{year}\\{month}\\{type}\\{docId}\\{docId}.pdf");
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Data["DocId"] = docId;
                throw;
            }
        }

        private void CreateDocQR(PdfDocument pdf)
        {
            string[] QRPaths = GetQRPaths();

            int QRCount = QRPaths.Length;
            int pagesCount = QRCount / 24 + 1;
            int index = 0;

            for (int i = 1; i < pagesCount + 1; i++)
            {
                if (i == pagesCount)
                {
                    if (QRCount != 0)
                    {
                        AddQR(pdf, index, QRPaths, QRCount);
                    }
                }
                else
                {
                    index = AddQR(pdf, index, QRPaths);
                    QRCount -= 24;
                }
            }
        }

        private string[] GetQRPaths()
        {
            string[] namesFolders = Directory.GetDirectories($"{docsFolder}{year}\\{month}\\{type}\\{docId}");
            var QRPaths = new string[namesFolders.Length * 2];
            string[] userQR;
            var n = 0;

            for (int i = 0; i < QRPaths.Length; i = i + 2)
            {
                userQR = Directory.GetFiles(namesFolders[n] + "\\QR");
                QRPaths[i] = userQR[0];
                QRPaths[i + 1] = userQR[1];
                n += 1;
            }

            return QRPaths;
        }

        private void AddQR(PdfDocument pdf, int index, string[] QRPaths, int QRCount)
        {
            Add(pdf, index, QRPaths, QRCount);
        }

        private int AddQR(PdfDocument pdf, int index, string[] QRPaths)
        {
            return Add(pdf, index, QRPaths);
        }

        private int Add(PdfDocument pdf, int index, string[] QRPaths, int QRCount = 24)
        {
            PdfPage newPage = pdf.AddPage();

            SetOrientation(newPage);
            SetPositions(QRPerRow);
            DrawObjects(newPage, ref index, QRPaths, QRCount);

            newPage.Close();
            return index;
        }

        private void SetOrientation(PdfPage newPage)
        {
            if (QRPerRow == 6)
            {
                newPage.Orientation = PageOrientation.Landscape;
            }
        }

        private void SetPositions(int QRPerRow)
        {
            if (QRPerRow == 6)
            {
                hHeading = 285;
                vHeading = 30;
                hName = 113;
                vName = 65;
                hQR = 110;
                vQR = 70;
            }

            if (QRPerRow == 4)
            {
                hHeading = 170;
                vHeading = 25;
                hName = 98;
                vName = 55;
                hQR = 95;
                vQR = 60;
            }
        }

        private void DrawObjects(PdfPage newPage, ref int index, string[] QRPaths, int QRCount)
        {
            using (XGraphics drawer = XGraphics.FromPdfPage(newPage))
            {
                DrawHeading(drawer);

                int rowsCount = QRCount / QRPerRow;
                int lastRowQR = QRPerRow;

                if (QRCount != 24)
                {
                    if (QRCount % QRPerRow != 0)
                    {
                        rowsCount = QRCount / QRPerRow + 1;
                        lastRowQR = QRCount % QRPerRow;
                    }
                }

                for (int j = 0; j < rowsCount; j++)
                {
                    if (QRCount != 24)
                    {
                        if (rowsCount - 1 == j)
                        {
                            QRPerRow = lastRowQR;
                        }
                    }

                    for (int n = 1; n < QRPerRow + 1; n++)
                    {
                        QRDistance = 0;

                        if (n % 2 == 0)
                        {
                            QRDistance = 10;
                        }

                        if (n % 2 != 0)
                        {
                            DrawName(drawer, ref index, QRPaths);
                        }

                        DrawQR(drawer, ref index, QRPaths);
                    }

                    ChangePositions(QRPerRow);
                }
            }
        }

        private void DrawHeading(XGraphics drawer)
        {
            var heading = "THE DIGITAL SIGNATURES OF THE DOCUMENT";

            drawer.DrawString(heading, fontHeading, brush, new XPoint(hHeading, vHeading));
        }

        private void DrawName(XGraphics drawer, ref int index, string[] QRPaths)
        {  
            string userName = Directory.GetParent(Path.GetDirectoryName(QRPaths[index])).Name;
            drawer.DrawString(userName, fontName, brush, new XPoint(hName, vName));

            hName += 210;
        }

        private void DrawQR(XGraphics drawer, ref int index, string[] QRPaths)
        {
            using (var img = XImage.FromFile(QRPaths[index]))
            {
                index += 1;

                drawer.DrawImage(img, hQR, vQR, 100, 100);

                hQR += 100 + QRDistance;
            }
        }

        private void ChangePositions(int QRPerRow)
        {
            if (QRPerRow == 6)
            {
                vName += 130;
                vQR += 130;
                hName = 113;
                hQR = 110;
            }

            if (QRPerRow == 4)
            {
                vName += 130;
                vQR += 130;
                hName = 98;
                hQR = 95;
            }
        }
    }
}
