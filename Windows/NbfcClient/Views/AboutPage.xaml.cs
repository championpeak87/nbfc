using NLog.Targets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Wpf.Ui.Controls;

namespace NbfcClient.Views
{
    /// <summary>
    /// Interakční logika pro AboutPage.xaml
    /// </summary>
    public partial class AboutPage : UiPage
    {
        public AboutPage()
        {
            InitializeComponent();
        }

        private void ContributeClickHandler(object sender, RoutedEventArgs e)
        {
            String target = "https://github.com/championpeak87/nbfc.git";
            System.Diagnostics.Process.Start(target);
        }

        private void DonateClickHandler(object sender, RoutedEventArgs e)
        {
            String target = "https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&amp;hosted_button_id=HUALCC9HY9MKC";
            System.Diagnostics.Process.Start(target);
        }
    }
}
