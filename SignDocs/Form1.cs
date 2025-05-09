using System;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using System.Collections.Generic;

namespace SignDocs
{
    public partial class Form1 : Form
    {
        private MockCryptoService _service = new MockCryptoService();

        private int _result;

        private string _keysFolder = "C:\\Keys\\";
        private string _docsFolder = "C:\\docs\\";

        private string _docsPath = "C:\\SignDocs\\docs.xml";
        private string _newUserPath = "C:\\SignDocs\\newUserAddition.xml";
        private string _userIdPath = "C:\\SignDocs\\userKeyCheck.xml";

        public Form1()
        {
            InitializeComponent();
            Directory.CreateDirectory(_keysFolder);

            FormClosing += Form1_FormClosing;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var fileDialog = new OpenFileDialog();

            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = fileDialog.FileName;
            }
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(_docsFolder))
            {
                Utilities.ShowWarning($"The \"{_docsFolder}\" has not been found!");
                return;
            }

            var paths = new List<string>() { _docsPath, _newUserPath, _userIdPath, };

            foreach (var path in paths)
            {
                if (!ValidatePath(path))
                {
                    return;
                }
            }

            if (!ValidateInputs())
            {
                return;
            }

            if (!ValidatePassword())
            {
                return;
            }

            if (!ValidateSerialNumber())
            {
                return;
            }

            if (!ValidateCertificate())
            {
                return;
            }

            ChangeControlsValues();

            try
            {
                await CreateSignaturesAsync(progressBar1);
                ExecuteCompleted();
            }
            catch (Exception ex)
            {
                ExecuteNotCompleted(ex);
            }
        }

        private bool ValidatePath(string path)
        {
            if (!File.Exists(path))
            {
                Utilities.ShowWarning($"The \"{path}\" has not been found!");
                return false;
            }

            return true;
        }

        private bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text))
            {
                Utilities.ShowWarning("The path to a private key is not chosen!");
                return false;
            }

            if (string.IsNullOrWhiteSpace(textBox2.Text))
            {
                Utilities.ShowWarning("The password is not provided");
                return false;
            }

            return true;
        }

        private bool ValidatePassword()
        {
            if (!_service.LoadCertificate(textBox1.Text, textBox2.Text))
            {
                Utilities.ShowWarning("The password is not valid");
                return false;
            }

            return true;
        }

        private bool ValidateSerialNumber()
        {
            string serialValue = _service.GetSerialNumber();

            var xmlDoc = new XmlDocument();
            xmlDoc.Load(_userIdPath);

            XmlNodeList IdNodes = xmlDoc.SelectNodes("//Id");

            if (IdNodes[0].InnerText != serialValue)
            {
                Utilities.ShowWarning("You are not the owner of this key!");
                return false;
            }

            return true;
        }

        private bool ValidateCertificate()
        {
            if (!_service.ValidateExpirationDate())
            {
                Utilities.ShowWarning($"Your private key is expired!");
                return false;
            }

            if (!ValidatePolicy())
            {
                return false;
            }

            if (!ValidateUsage())
            {
                return false;
            }

            return true;
        }

        private bool ValidatePolicy()
        {
            var validPolicy = "123.456.789.0";

            string actualPolicy = _service.GetPolicy();

            if (!actualPolicy.Contains(validPolicy))
            {
                Utilities.ShowWarning("Invalid private key policy!");
                return false;
            }

            return true;
        }

        private bool ValidateUsage()
        {
            var keyUsage1 = "digitalSignature";
            var keyUsage2 = "nonRepudiation";

            string keyUsage = _service.GetUsage();

            if (!keyUsage.Contains(keyUsage1) || !keyUsage.Contains(keyUsage2))
            {
                Utilities.ShowWarning("Invalid key usage");
                return false;
            }

            return true;
        }

        private void ChangeControlsValues()
        {
            progressBar1.Minimum = 0;

            var docs = new XmlDocument();
            docs.Load(_docsPath);

            XmlNodeList IdNodes = docs.SelectNodes("//Id");
            progressBar1.Maximum = IdNodes.Count;

            label1.Text = "Please, wait ...";

            label2.Visible = false;
            label2.Enabled = false;

            textBox1.Visible = false;
            textBox1.Enabled = false;

            textBox2.Visible = false;
            textBox2.Enabled = false;

            button1.Visible = false;
            button1.Enabled = false;

            button2.Visible = false;
            button2.Enabled = false;

            progressBar1.Visible = true;
            progressBar1.Enabled = true;
        }

        private void ExecuteCompleted()
        {
            label1.Text = "The files have been succesfully signed!";
            button4.Visible = true;
            button4.Enabled = true;

            AddHexName();
        }

        private void AddHexName()
        {
            var xml = new XmlDocument();
            xml.Load(_newUserPath);

            var addNode = xml.SelectNodes("//Add");

            if (addNode[0].InnerText.Contains("0"))
            {
                var hexNode = xml.SelectNodes("//Hex");

                hexNode[0].InnerText = Utilities.ConvertHexadecimal(_userName);

                xml.Save(_newUserPath);
            }
        }

        private void ExecuteNotCompleted(Exception ex)
        {
            label1.Text = "The process has failed!";
            progressBar1.Visible = false;

            textBox3.Visible = true;
            textBox3.Enabled = true;

            if (ex.Data.Contains("DocId"))
            {
                textBox3.Text = $"The document: {ex.Data["DocId"]}{Environment.NewLine}{ex.ToString()}";
            }
            else
            {
                textBox3.Text = ex.ToString();
            }

            button4.Visible = true;
            button4.Enabled = true;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            AddResult();
        }

        private void AddResult()
        {
            var xml = new XmlDocument();

            XmlDeclaration declare = xml.CreateXmlDeclaration("1.0", null, null);
            xml.AppendChild(declare);

            XmlElement result = xml.CreateElement("Result");
            result.InnerText = _result.ToString();
            xml.AppendChild(result);

            xml.Save("C:\\SignDocs\\result.xml");
        }
    }
}