using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LibVLCSharp.Shared;

namespace DougVIdeoPlayer
{
    public partial class FormOpenUrl : Form
    {
        public bool UrlSelected { get; set; } = false;
        public string UrlInitial { get; set; }
        public string UrlFinal { get; set; }

        public FormOpenUrl()
        {
            InitializeComponent();
        }

        private async void BtnOpenURL_Click(object sender, EventArgs e)
        {
            Uri uriResult;
            bool result = Uri.TryCreate(TUrl.Text, UriKind.Absolute, out uriResult)
                          && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

            if (!result)
            {
                return;
            }

            UrlInitial = TUrl.Text;

            using (var libVLC = new LibVLC())
            {
                var media = new Media(libVLC, TUrl.Text, FromType.FromLocation);
                try
                {
                    await media.Parse(MediaParseOptions.ParseNetwork);
                    if (media.SubItems.Count > 0)
                    {
                        UrlFinal = media.SubItems.First().Mrl;
                        UrlSelected = true;
                    }
                    else
                    {
                        MessageBox.Show("Unable to get video link.", "Error while processing URL");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error while processing URL");
                }

            }

            DialogResult = DialogResult.OK;
        }

        private void FormOpenUrl_Shown(object sender, EventArgs e)
        {
            TUrl.Text = "";
            UrlSelected = false;
            UrlInitial = "";
            UrlFinal = "";
        }
    }
}
