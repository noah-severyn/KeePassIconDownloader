using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using KeePass.Plugins;
using KeePassLib;
using KeePassLib.Collections;
using KeePassIconDownloader.Data;

namespace KeePassIconDownloader {
    partial class KeePassIconDownloaderForm {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        private readonly IPluginHost _host;

        private PwObjectList<PwEntry> entries = [];
        private BindingList<PwEntryItem> customEntries = [];

        private ListSortDirection _sortDir = ListSortDirection.Ascending;
        private int _sortColIdx = -1;

        public KeePassIconDownloaderForm(IPluginHost host) {
            InitializeComponent();
            _host = host;
        }

        protected override void OnLoad(EventArgs e) {
            base.OnLoad(e);
            EntryGrid.ColumnHeaderMouseClick += EntryGrid_ColumnHeaderMouseClick;
            EntryGrid.CellPainting += EntryGrid_CellPainting;
            LoadEntries();
        }

        private void LoadEntries() {
            var db = _host.Database;
            if (db == null || !db.IsOpen) {
                return;
            }

            EntryGrid.Rows.Clear();

            entries = db.RootGroup.GetEntries(true);
            foreach (PwEntry entry in entries) {
                customEntries.Add(new PwEntryItem(entry, _host));
            }

            EntryGrid.DataSource = customEntries;

            EntryGrid.Columns["Title"].Width = 200;
            EntryGrid.Columns["CurrentSize"].Width = 80;
            EntryGrid.Columns["Group"].Width = 120;
            EntryGrid.Columns["Url"].Width = 250;
        }

       


        

        private void EntryGrid_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e) {
            _sortDir = (_sortColIdx == e.ColumnIndex && _sortDir == ListSortDirection.Ascending) ? ListSortDirection.Descending : ListSortDirection.Ascending;
            _sortColIdx = e.ColumnIndex;

            var columnName = EntryGrid.Columns[e.ColumnIndex].DataPropertyName;

            if (columnName == "CurrentSize") {
                columnName = "CurrentSizeSort";
            }

            var prop = typeof(PwEntryItem).GetProperty(columnName);
            if (prop == null) return;

            var sorted = _sortDir == ListSortDirection.Ascending
                ? customEntries.OrderBy(x => prop.GetValue(x)).ToList()
                : customEntries.OrderByDescending(x => prop.GetValue(x)).ToList();

            customEntries = new BindingList<PwEntryItem>(sorted);
            EntryGrid.DataSource = customEntries;
        }

        //DataGridView doesn't natively support image+text in the same cell, so you need to handle the CellPainting event and draw them manually
        private void EntryGrid_CellPainting(object sender, DataGridViewCellPaintingEventArgs e) {
            if (e.ColumnIndex != EntryGrid.Columns["Title"].Index || e.RowIndex < 0) return;

            e.Paint(e.CellBounds, DataGridViewPaintParts.Background | DataGridViewPaintParts.Border);

            var item = customEntries[e.RowIndex];
            var icon = item.CurrentIcon;

            const int padding = 3;
            const int iconSize = 16;
            int textX = e.CellBounds.X + padding;

            if (icon != null) {
                var iconRect = new Rectangle(
                    e.CellBounds.X + padding,
                    e.CellBounds.Y + (e.CellBounds.Height - iconSize) / 2,
                    iconSize,
                    iconSize);
                e.Graphics.DrawImage(icon, iconRect);
                textX = iconRect.Right + padding;
            }

            var textRect = new Rectangle(textX, e.CellBounds.Y, e.CellBounds.Right - textX - padding, e.CellBounds.Height);

            var flags = TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis;
            var textColor = (e.State & DataGridViewElementStates.Selected) != 0
                ? e.CellStyle.SelectionForeColor
                : e.CellStyle.ForeColor;

            TextRenderer.DrawText(e.Graphics, item.Title, e.CellStyle.Font, textRect, textColor, flags);

            e.Handled = true;
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
            this.EntryGrid = new System.Windows.Forms.DataGridView();
            this.pwEntryBindingSource = new System.Windows.Forms.BindingSource(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.EntryGrid)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pwEntryBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // EntryGrid
            // 
            this.EntryGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.EntryGrid.Location = new System.Drawing.Point(0, 0);
            this.EntryGrid.Name = "EntryGrid";
            this.EntryGrid.Size = new System.Drawing.Size(783, 437);
            this.EntryGrid.TabIndex = 0;
            // 
            // pwEntryBindingSource
            // 
            this.pwEntryBindingSource.DataSource = typeof(KeePassLib.PwEntry);
            // 
            // KeePassIconDownloaderForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1029, 682);
            this.Controls.Add(this.EntryGrid);
            this.Name = "KeePassIconDownloaderForm";
            this.Text = "KeePassIconDownloaderForm";
            ((System.ComponentModel.ISupportInitialize)(this.EntryGrid)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pwEntryBindingSource)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView EntryGrid;
        private BindingSource pwEntryBindingSource;
    }
}