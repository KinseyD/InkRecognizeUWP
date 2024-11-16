using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace InkRecognizeUWP.View
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingPage : Page
    {
        public SettingPage()
        {
            this.InitializeComponent();
        }

        private void OnLightButtonClicked(object sender, RoutedEventArgs e)
        {
            var window = (FrameworkElement)Window.Current.Content;
            window.RequestedTheme = ElementTheme.Light;
        }
        private void OnDarkButtonClicked(object sender, RoutedEventArgs e)
        {
            var window = (FrameworkElement)Window.Current.Content;
            window.RequestedTheme = ElementTheme.Dark;
        }

        private void OnDefaultButtonClicked(object sender, RoutedEventArgs e)
        {
            var window = (FrameworkElement)Window.Current.Content;
            window.RequestedTheme = ElementTheme.Default;
        }
    }
}
