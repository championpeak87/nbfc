using StagWare.Settings;
using System.Windows.Media;

namespace NbfcClient
{
    public class AppSettings : SettingsBase
    {
        public AppSettings()
        {
            SettingsVersion = 0;
        }

        [DefaultValue(typeof(XmlWrapper<Color>), "White")]
        public XmlWrapper<Color> TrayIconForegroundColor { get; set; }

        [DefaultValue(false)]
        public bool CloseToTray { get; set; }

        [DefaultValue(true)]
        public bool ShowTrayIcon { get; set; }
    }
}
