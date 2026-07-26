using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using KeePass.Plugins;
using KeePassLib;
using KeePassLib.Collections;

namespace KeePassIconDownloader {
    partial class KeePassIconDownloaderForm {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        private readonly IPluginHost _host;

        private PwObjectList<PwEntry> entries = [];
        private List<PwEntryItem> customEntries = [];

        public KeePassIconDownloaderForm(IPluginHost host) {
            InitializeComponent();
            _host = host;
        }

        protected override void OnLoad(EventArgs e) {
            base.OnLoad(e);
            LoadEntries();
        }

        private void LoadEntries() {
            var db = _host.Database;
            if (db == null || !db.IsOpen) {
                return;
            }

            dataGridView1.Rows.Clear();

            entries = db.RootGroup.GetEntries(true);
            foreach (PwEntry entry in entries) {
                customEntries.Add(new PwEntryItem(entry));
            }
                
            dataGridView1.DataSource = customEntries;
        }

        /// <summary>
        /// Custom class to display properties of <see cref="PwEntry"/> objects in a list.
        /// </summary>
        internal class PwEntryItem {
            private readonly PwEntry _entry;
            public PwEntryItem(PwEntry entry) {
                _entry = entry;
            }

            public string Title => _entry.Strings.ReadSafe("Title");
            public string URL => _entry.Strings.ReadSafe("URL");
            public string Group => _entry.ParentGroup?.Name ?? string.Empty;

            // Keep a reference to the original entry for later use (e.g. downloading icons)
            [Browsable(false)]
            public PwEntry Entry => _entry;
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.pwEntryBindingSource = new System.Windows.Forms.BindingSource(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pwEntryBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Location = new System.Drawing.Point(0, 0);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.Size = new System.Drawing.Size(1080, 419);
            this.dataGridView1.TabIndex = 0;
            // 
            // pwEntryBindingSource
            // 
            this.pwEntryBindingSource.DataSource = typeof(KeePassLib.PwEntry);
            // 
            // KeePassIconDownloaderForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1119, 450);
            this.Controls.Add(this.dataGridView1);
            this.Name = "KeePassIconDownloaderForm";
            this.Text = "KeePassIconDownloaderForm";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pwEntryBindingSource)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView1;
        private BindingSource pwEntryBindingSource;
    }
}