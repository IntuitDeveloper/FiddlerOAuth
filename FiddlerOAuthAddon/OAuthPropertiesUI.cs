using System;
using System.Windows.Forms;
using System.IO;
using System.Xml.Linq;

namespace FiddlerOAuthAddon
{
    public partial class OAuthPropertiesUI : UserControl
    {
        public OAuthPropertiesUI()
        {
            InitializeComponent();
            EnableFilter.Checked = OAuthAddon.EnableOAuthOthorization;
            optionsGroupBox.Enabled = EnableFilter.Checked;
        }

        private void EnableFilter_Click(object sender, EventArgs e)
        {
            OAuthAddon.EnableOAuthOthorization = EnableFilter.Checked;
        }

        private void EnableFilter_CheckedChanged(object sender, EventArgs e)
        {
            optionsGroupBox.Enabled = EnableFilter.Checked;
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            openFileDialog.InitialDirectory =
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),  @"Fiddler2\Filters");

            openFileDialog.CheckFileExists = true;
            if( openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string fileName = openFileDialog.FileName;

                try
                {
                    XElement xElement = XElement.Load(fileName);
                    txtConsumerKey.Text     = GetString(xElement, "ConsumerKey");
                    txtConsumerSecret.Text  = GetString(xElement, "ConsumerSecret");
                    txtToken.Text           = GetString(xElement, "Token");
                    txtTokenSecret.Text     = GetString(xElement, "TokenSecret");
                    cmbSignMethod.Text      = GetString(xElement, "SignMethod");

                    OAuthAddon.Apply(txtConsumerKey.Text, txtConsumerSecret.Text, cmbSignMethod.Text, txtToken.Text, txtTokenSecret.Text);
                    btnApply.Enabled = false;
                    EnableFilter.Checked = true;
                    OAuthAddon.EnableOAuthOthorization = EnableFilter.Checked;
                }
                catch (Exception)
                {
                    MessageBox.Show("UI Load failed");
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
                                                {
                                                    Filter = "OAuth Key File|*.okey",
                                                    Title = "Save OAuth Keys",
                                                    InitialDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),@"Fiddler2\Filters"),
                                                    CheckFileExists = false
                                                };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string fileName = saveFileDialog.FileName;
                var xDoc = new XDocument(
                        new XElement("Root",
                            new XElement("ConsumerKey", txtConsumerKey.Text),
                            new XElement("ConsumerSecret", txtConsumerSecret.Text),
                            new XElement("Token", txtToken.Text),
                            new XElement("TokenSecret", txtTokenSecret.Text),
                            new XElement("SignMethod", cmbSignMethod.Text)
                            )
                    );
                File.WriteAllText(fileName, xDoc.ToString());
            }
        }


        private void btnApply_Click(object sender, EventArgs e)
        {
            btnApply.Enabled = false;
            OAuthAddon.Apply(txtConsumerKey.Text, txtConsumerSecret.Text, cmbSignMethod.Text, txtToken.Text, txtTokenSecret.Text);
        }

        #region Utility Functions
        private string TagValue(XElement elem, XName name)
        {
            if (elem == null)
                return null;

            var val = name == null ? elem : elem.Element(name);
            return val == null ? null : val.Value;
        }

        private string GetString(XElement elem, XName tagName, string defaultValue /* = ""*/)
        {
            var val = TagValue(elem, tagName);

            return string.IsNullOrEmpty(val)
                    ? defaultValue
                    : val;
        }

        private string GetString(XElement elem, XName tagName)
        {
            return GetString(elem, tagName, String.Empty);
        } 
        #endregion

        private void txtConsumerKey_TextChanged(object sender, EventArgs e)
        {
            btnApply.Enabled = true;
        }

        private void txtConsumerSecret_TextChanged(object sender, EventArgs e)
        {
            btnApply.Enabled = true;
        }

        private void txtToken_TextChanged(object sender, EventArgs e)
        {
            btnApply.Enabled = true;
        }

        private void txtTokenSecret_TextChanged(object sender, EventArgs e)
        {
            btnApply.Enabled = true;
        }


    }
}
