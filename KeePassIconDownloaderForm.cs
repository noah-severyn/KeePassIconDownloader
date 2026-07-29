using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KeePassIconDownloader {
    public partial class KeePassIconDownloaderForm : Form {
        public KeePassIconDownloaderForm() {
            InitializeComponent();
        }

        private async void FetchFaviconsButton_Click(object sender, EventArgs e) {
            var provider = IconDownloader.GetProvider("Favicon Kit");

            var selectedEntry = filteredEntryItems.FirstOrDefault(e => e.Selected);
            if (selectedEntry is null) return;

            //The favicon apis typically want just the host, so prefer providing that if possible
            string hostUrl;
            if (Uri.TryCreate(selectedEntry.Url, UriKind.Absolute, out var parsedUrl)) {
                hostUrl = parsedUrl.Host;
            } else {
                hostUrl = selectedEntry.Url;
            }

            var results = await IconDownloader.FetchFaviconsAsync(provider, hostUrl);
            
            if (results[128] != null) {
                Favicon128Image.Image = results[128];
            }
            if (results[64] != null) {
                Favicon64Image.Image = results[64];
            }
            if (results[32] != null) {
                Favicon32Image.Image = results[32];
            }
            if (results[16] != null) {
                Favicon16Image.Image = results[16];
            }
        }
    }
}
