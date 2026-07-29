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
                filteredEntryItems.Add(new PwEntryItem(entry, _host));
            }

            var groups = _host.Database.RootGroup.Groups;
            GroupSelection.Items.Clear();
            //All groups except recycle bin are checked by default
            foreach (var group in groups) {
                GroupSelection.Items.Add(group.Name, group.Name != "Recycle Bin");
            }
            //BeginInvoke is needed because ItemCheck fires before the check state updates, so deferring to the next message loop tick ensures ApplyFilter sees the new state.
            GroupSelection.ItemCheck += (s, e) => BeginInvoke((Action) ApplyFilter);
            ApplyFilter();

            EntryGrid.DataSource = filteredEntryItems;
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
                filteredEntryItems.Clear();
                foreach (var entry in allEntryItems) {
                    if (selectedGroups.Contains(entry.Group)) {
                        filteredEntryItems.Add(entry);
                    }
                }
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
            } else if (columnName == "Selected") {
                bool allSelected = !filteredEntryItems.All(e => e.Selected);
                foreach (var item in filteredEntryItems) {
                    item.Selected = allSelected;
                }
                EntryGrid.Refresh();
                return;
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

            var item = filteredEntryItems[e.RowIndex];
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
            this.Favicon128Image = new System.Windows.Forms.PictureBox();
            this.Favicon64Image = new System.Windows.Forms.PictureBox();
            this.Favicon32Image = new System.Windows.Forms.PictureBox();
            this.Favicon16Image = new System.Windows.Forms.PictureBox();
            this.Favicon128Label = new System.Windows.Forms.Label();
            this.Favicon64Label = new System.Windows.Forms.Label();
            this.Favicon32Label = new System.Windows.Forms.Label();
            this.Favicon16Label = new System.Windows.Forms.Label();
            this.FetchFaviconsButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.EntryGrid)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pwEntryBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Favicon128Image)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Favicon64Image)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Favicon32Image)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Favicon16Image)).BeginInit();
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
            // Favicon128Image
            // 
            this.Favicon128Image.Location = new System.Drawing.Point(379, 476);
            this.Favicon128Image.Name = "Favicon128Image";
            this.Favicon128Image.Size = new System.Drawing.Size(128, 128);
            this.Favicon128Image.TabIndex = 3;
            this.Favicon128Image.TabStop = false;
            // 
            // Favicon64Image
            // 
            this.Favicon64Image.Location = new System.Drawing.Point(513, 476);
            this.Favicon64Image.Name = "Favicon64Image";
            this.Favicon64Image.Size = new System.Drawing.Size(64, 64);
            this.Favicon64Image.TabIndex = 4;
            this.Favicon64Image.TabStop = false;
            // 
            // Favicon32Image
            // 
            this.Favicon32Image.Location = new System.Drawing.Point(583, 476);
            this.Favicon32Image.Name = "Favicon32Image";
            this.Favicon32Image.Size = new System.Drawing.Size(32, 32);
            this.Favicon32Image.TabIndex = 5;
            this.Favicon32Image.TabStop = false;
            // 
            // Favicon16Image
            // 
            this.Favicon16Image.Location = new System.Drawing.Point(621, 476);
            this.Favicon16Image.Name = "Favicon16Image";
            this.Favicon16Image.Size = new System.Drawing.Size(16, 16);
            this.Favicon16Image.TabIndex = 6;
            this.Favicon16Image.TabStop = false;
            // 
            // Favicon128Label
            // 
            this.Favicon128Label.AutoSize = true;
            this.Favicon128Label.Location = new System.Drawing.Point(379, 607);
            this.Favicon128Label.Name = "Favicon128Label";
            this.Favicon128Label.Size = new System.Drawing.Size(48, 13);
            this.Favicon128Label.TabIndex = 7;
            this.Favicon128Label.Text = "128x128";
            this.Favicon128Label.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // Favicon64Label
            // 
            this.Favicon64Label.AutoSize = true;
            this.Favicon64Label.Location = new System.Drawing.Point(513, 543);
            this.Favicon64Label.Name = "Favicon64Label";
            this.Favicon64Label.Size = new System.Drawing.Size(36, 13);
            this.Favicon64Label.TabIndex = 8;
            this.Favicon64Label.Text = "64x64";
            this.Favicon64Label.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // Favicon32Label
            // 
            this.Favicon32Label.AutoSize = true;
            this.Favicon32Label.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Favicon32Label.Location = new System.Drawing.Point(583, 511);
            this.Favicon32Label.Name = "Favicon32Label";
            this.Favicon32Label.Size = new System.Drawing.Size(36, 13);
            this.Favicon32Label.TabIndex = 9;
            this.Favicon32Label.Text = "32x32";
            this.Favicon32Label.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // Favicon16Label
            // 
            this.Favicon16Label.AutoSize = true;
            this.Favicon16Label.Location = new System.Drawing.Point(621, 495);
            this.Favicon16Label.Name = "Favicon16Label";
            this.Favicon16Label.Size = new System.Drawing.Size(36, 13);
            this.Favicon16Label.TabIndex = 10;
            this.Favicon16Label.Text = "16x16";
            this.Favicon16Label.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // FetchFaviconsButton
            // 
            this.FetchFaviconsButton.Location = new System.Drawing.Point(164, 476);
            this.FetchFaviconsButton.Name = "FetchFaviconsButton";
            this.FetchFaviconsButton.Size = new System.Drawing.Size(104, 23);
            this.FetchFaviconsButton.TabIndex = 11;
            this.FetchFaviconsButton.Text = "Fetch Favicons";
            this.FetchFaviconsButton.UseVisualStyleBackColor = true;
            this.FetchFaviconsButton.Click += new System.EventHandler(this.FetchFaviconsButton_Click);
            // 
            // KeePassIconDownloaderForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1004, 665);
            this.Controls.Add(this.FetchFaviconsButton);
            this.Controls.Add(this.Favicon16Label);
            this.Controls.Add(this.Favicon32Label);
            this.Controls.Add(this.Favicon64Label);
            this.Controls.Add(this.Favicon128Label);
            this.Controls.Add(this.Favicon16Image);
            this.Controls.Add(this.Favicon32Image);
            this.Controls.Add(this.Favicon64Image);
            this.Controls.Add(this.Favicon128Image);
            this.Controls.Add(this.GroupSelection);
            this.Controls.Add(this.EntryGrid);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "KeePassIconDownloaderForm";
            this.Text = "KeePassIconDownloaderForm";
            ((System.ComponentModel.ISupportInitialize)(this.EntryGrid)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pwEntryBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Favicon128Image)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Favicon64Image)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Favicon32Image)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Favicon16Image)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView EntryGrid;
        private BindingSource pwEntryBindingSource;
        private CheckedListBox GroupSelection;
        private PictureBox Favicon128Image;
        private PictureBox Favicon64Image;
        private PictureBox Favicon32Image;
        private PictureBox Favicon16Image;
        private Label Favicon128Label;
        private Label Favicon64Label;
        private Label Favicon32Label;
        private Label Favicon16Label;
        private Button FetchFaviconsButton;
    }
}