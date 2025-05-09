using System.Text;
using System;
using System.Windows.Forms;

namespace SignDocs
{
    internal static class Utilities
    {
        internal static void ShowWarning(string message)
        {
            MessageBox.Show(message, "Notification", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        internal static string MakeCapitals(string name)
        {
            string[] words = name.Split(' ');

            for (int i = 0; i < words.Length; i++)
            {
                words[i] = words[i][0] + words[i].Substring(1).ToLower();
            }

            return string.Join(" ", words);
        }

        internal static string ConvertHexadecimal(string name)
        {
            var nameBytes = Encoding.UTF8.GetBytes(name);
            var stringBuilder = new StringBuilder();

            foreach (byte oneByte in nameBytes)
            {
                stringBuilder.Append(Convert.ToString(oneByte, 16).PadLeft(3, '0'));
            }

            return stringBuilder.ToString();
        }
    }
}