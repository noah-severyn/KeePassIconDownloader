using System.ComponentModel;
using System.Drawing;
using KeePass.Plugins;
using KeePassLib;

namespace KeePassIconDownloader.Data {
    /// <summary>
    /// Custom class to display properties of <see cref="PwEntry"/> objects in a list.
    /// </summary>
    internal class PwEntryItem {
        private readonly PwEntry _entry;
        private readonly IPluginHost _host;
        private bool _hasCustomIcon;
        public PwEntryItem(PwEntry entry, IPluginHost host) {
            _entry = entry;
            _host = host;

            Title =  _entry.Strings.ReadSafe("Title");
            Group = _entry.ParentGroup?.Name ?? string.Empty;
            Url = _entry.Strings.ReadSafe("URL");
            CurrentIcon = GetIcon();
        }

        /// <summary>
        /// Gets the icon for this password entry.
        /// </summary>
        /// <returns>The custom icon if set; <see langword="null"/> if not set.</returns>
        private Image? GetIcon() {
            try {
                var customIcon = _host.Database.CustomIcons.Find(icon => icon.Uuid.Equals(_entry.CustomIconUuid));
                if (!_entry.CustomIconUuid.Equals(PwUuid.Zero) && customIcon != null) {
                    _hasCustomIcon = true;
                    return customIcon.GetImage();
                }

                //If no custom icon is found, use the default icon for this entry
                var icons = _host.MainWindow.ClientIcons;
                if (icons != null) {
                    return icons.Images[(int) _entry.IconId];
                }
            }
            catch { }
            return null;
        }
        public bool Selected { get; set; } = false;
        public string Title { get; private set; }

        [Browsable(false)]
        public Image? CurrentIcon { get; private set; }
        /// <summary>
        /// Defines the sort order for the <see cref="CurrentSize"/> column, equal to the image's width in pixels.
        /// </summary>
        [Browsable(false)]
        public int CurrentSizeSort {
            get {
                return _hasCustomIcon ? CurrentIcon?.Width ?? 0 : 0;
            }
        }
        /// <summary>
        /// Image size of the currently set icon, or '<c>Not set</c>' if no current icon has been set.
        /// </summary>
        public string CurrentSize {
            get {
                if (CurrentIcon is null) {
                    return "Unknown";
                } else if (!_hasCustomIcon) {
                    return "Not set";
                } else {
                    return $"{CurrentIcon.Width}x{CurrentIcon.Height}";
                }
            }
        }
        public string Group { get; private set; }
        public string Url { get; private set; }

        // Keep a reference to the original entry for later use (e.g. downloading icons)
        [Browsable(false)]
        public PwEntry Entry => _entry;
    }
}
