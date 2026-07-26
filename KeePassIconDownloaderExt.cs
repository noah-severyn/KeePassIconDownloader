using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;

using KeePass.Forms;
using KeePass.Plugins;
using KeePass.Resources;
using KeePass.UI;

using KeePassLib;
using KeePassLib.Security;
using KeePassLib.Utility;



namespace KeePassIconDownloader {
    public sealed class KeePassIconDownloaderExt: Plugin {
        private IPluginHost _host = null;
        private ToolStripMenuItem? menuItem;

        /// <summary>
		/// Called by KeePass when you should initialize your plugin.
		/// </summary>
		/// <param name="host">Plugin host interface. Through this interface you can access the KeePass main window, the currently opened database, etc.</param>
		/// <returns>You must return <see langword="true"/> in order to signal successful initialization. If you return <see langword="false"/>, KeePass unloads your plugin (without calling the <see cref="Terminate"/> method of your plugin).</returns>
        public override bool Initialize(IPluginHost host) {
            if (host == null) {
                return false;
            }
            _host = host;

            var menu = _host.MainWindow.ToolsMenu.DropDownItems;
            menuItem = new ToolStripMenuItem("Download Icons", null, OpenForm);
            menu.Add(menuItem);


            return true;
        }


        /// <summary>
        /// Called by KeePass when you should free all resources, close files/streams, remove event handlers, etc.
        /// </summary>
        public override void Terminate() {
            var menu = _host.MainWindow.ToolsMenu.DropDownItems;
            menu.Remove(menuItem);
        }

        private void OpenForm(object sender, EventArgs e) {
            KeePassIconDownloaderForm form = new KeePassIconDownloaderForm(_host);
            form.ShowDialog();
        }
    }
}
