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
        private BindingList<PwEntryItem> allEntryItems = [];
        private BindingList<PwEntryItem> filteredEntryItems = [];

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
            GroupSelection.SelectedIndexChanged += GroupSelection_SelectedIndexChanged;
            LoadEntries();
        }

        private void LoadEntries() {
            var db = _host.Database;
            if (db == null || !db.IsOpen) {
                return;
            }

            EntryGrid.Rows.Clear();

            var entries = db.RootGroup.GetEntries(true);
            foreach (PwEntry entry in entries) {
                allEntryItems.Add(new PwEntryItem(entry, _host));
            }

            var groups = _host.Database.RootGroup.Groups;
            //var groups = entries.Select(e => e.ParentGroup).Distinct().OrderBy(g => g.Name).ToList();
            GroupSelection.Items.Clear();
            //All groups except recycle bin are checked by default
            foreach (var group in groups) {
                GroupSelection.Items.Add(group.Name, group.Name != "Recycle Bin");
            }
            //BeginInvoke is needed because ItemCheck fires before the check state updates, so deferring to the next message loop tick ensures ApplyFilter sees the new state.
            GroupSelection.ItemCheck += (s, e) => BeginInvoke((Action) ApplyFilter);
            ApplyFilter();

            EntryGrid.DataSource = allEntryItems;
            EntryGrid.Columns["Selected"].Width = 30;
            EntryGrid.Columns["Selected"].HeaderText = string.Empty;
            EntryGrid.Columns["Title"].Width = 200;
            EntryGrid.Columns["CurrentSize"].Width = 80;
            EntryGrid.Columns["Group"].Width = 120;
            EntryGrid.Columns["Url"].Width = 250;
        }


        private void GroupSelection_SelectedIndexChanged(object sender, EventArgs e) {
            ApplyFilter();
        }

        private void ApplyFilter() {
            var selectedGroups = GroupSelection.CheckedItems.Cast<string>().ToHashSet();

            if (selectedGroups.Count == GroupSelection.Items.Count) {
                filteredEntryItems = allEntryItems;
            } else {
                foreach (var entry in allEntryItems) {
                    if (selectedGroups.Contains(entry.Group)) {
                        filteredEntryItems.Add(entry);
                    }
                }
                //filteredEntryItems = (BindingList<PwEntryItem>) allEntryItems.Where(e => selectedGroups.Contains(e.Group)).ToList();
            }

            EntryGrid.DataSource = filteredEntryItems;
            // ... column setup
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
                ? allEntryItems.OrderBy(x => prop.GetValue(x)).ToList()
                : allEntryItems.OrderByDescending(x => prop.GetValue(x)).ToList();

            allEntryItems = new BindingList<PwEntryItem>(sorted);
            EntryGrid.DataSource = allEntryItems;
        }

        //DataGridView doesn't natively support image+text in the same cell, so you need to handle the CellPainting event and draw them manually
        private void EntryGrid_CellPainting(object sender, DataGridViewCellPaintingEventArgs e) {
            if (e.ColumnIndex != EntryGrid.Columns["Title"].Index || e.RowIndex < 0) return;

            e.Paint(e.CellBounds, DataGridViewPaintParts.Background | DataGridViewPaintParts.Border);

            var item = allEntryItems[e.RowIndex];
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
            this.GroupSelection = new System.Windows.Forms.CheckedListBox();
            ((System.ComponentModel.ISupportInitialize)(this.EntryGrid)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pwEntryBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // EntryGrid
            // 
            this.EntryGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.EntryGrid.Location = new System.Drawing.Point(164, 45);
            this.EntryGrid.Name = "EntryGrid";
            this.EntryGrid.Size = new System.Drawing.Size(828, 425);
            this.EntryGrid.TabIndex = 0;
            // 
            // pwEntryBindingSource
            // 
            this.pwEntryBindingSource.DataSource = typeof(KeePassLib.PwEntry);
            // 
            // GroupSelection
            // 
            this.GroupSelection.CheckOnClick = true;
            this.GroupSelection.FormattingEnabled = true;
            this.GroupSelection.Location = new System.Drawing.Point(13, 45);
            this.GroupSelection.Name = "GroupSelection";
            this.GroupSelection.Size = new System.Drawing.Size(145, 394);
            this.GroupSelection.TabIndex = 2;
            // 
            // KeePassIconDownloaderForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1004, 561);
            this.Controls.Add(this.GroupSelection);
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
        private CheckedListBox GroupSelection;
    }
}