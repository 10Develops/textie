using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Core;
using Windows.Storage;
using Windows.Foundation.Metadata;
using Windows.ApplicationModel;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.ApplicationModel.Core;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Textie
{
    public sealed partial class SettingsPage : Page
    {
        SystemNavigationManager currentView = SystemNavigationManager.GetForCurrentView();
        ApplicationDataContainer localSettings =
            ApplicationData.Current.LocalSettings;

        DispatcherTimer timer;

        CoreApplicationViewTitleBar coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
        ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;

        int ClickCounter = 0;

        public SettingsPage()
        {
            this.InitializeComponent();

            coreTitleBar.LayoutMetricsChanged += coreTitleBar_LayoutMetricsChanged;

            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.ApplicationView"))
            {
                if (titleBar != null)
                {
                    SettingsContentGrid.Margin = new Thickness(0, 32.4, 0, 0);
                    Window.Current.SetTitleBar(MiddleAppTitleBar);
                    MiddleAppTitleBar.Visibility = Visibility.Visible;
                    LeftAppTitleBar.Visibility = Visibility.Visible;
                }
            }

            this.NavigationCacheMode = NavigationCacheMode.Required;

            MakeDesign();

            timer = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 4) };
            timer.Tick += (s, o) =>
            {
                ClickCounter = 0;
                timer.Stop();
            };
        }

        private void CurrentView_BackRequested(object sender, BackRequestedEventArgs e)
        {
            On_BackRequested();
            e.Handled = true;
        }

        private void coreTitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args)
        {
            MiddleAppTitleBar.Margin = new Thickness(64, 0, 0, 0);
            MiddleAppTitleBar.Height = sender.Height;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.ApplicationView"))
            {
                if (titleBar != null)
                {
                    Window.Current.SetTitleBar(MiddleAppTitleBar);
                }
            }

            currentView.BackRequested += CurrentView_BackRequested;
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            On_BackRequested();
        }

        private void SettingsGridMain_Loaded(object sender, RoutedEventArgs e)
        {
            string theme = localSettings.Values["theme"].ToString();
            if (theme == "WD")
            {
                WindowsDefaultRadioButton.IsChecked = true;

                if (ApiInformation.IsTypePresent("Windows.UI.Xaml.Media.AcrylicBrush"))
                {
                    TransparencyToggleSwitch.Visibility = Visibility.Visible;
                }
            }
            else if (theme == "Dark")
            {
                DarkRadioButton.IsChecked = true;

                TransparencyToggleSwitch.Visibility = Visibility.Collapsed;
            }
            else if (theme == "Light")
            {
                LightRadioButton.IsChecked = true;

                TransparencyToggleSwitch.Visibility = Visibility.Collapsed;
            }

            string TransparencyBool = localSettings.Values["transparency"].ToString();
            if (ApiInformation.IsTypePresent("Windows.UI.Xaml.Media.AcrylicBrush"))
            {
                if(TransparencyBool == "1")
                {
                    TransparencyToggleSwitch.IsOn = true;
                }
                else
                {
                    TransparencyToggleSwitch.IsOn = false;
                }
            }
            else
            {
                TransparencyToggleSwitch.Visibility = Visibility.Collapsed;
            }

            string TextBoxTheme = localSettings.Values["TextBoxTheme"].ToString();
            if(TextBoxTheme == "Light")
            {
                TextBoxThemeComboBox.SelectedItem = LightTextBoxThemeComboBoxItem;
            }
            else if(TextBoxTheme == "Dark")
            {
                TextBoxThemeComboBox.SelectedItem = DarkTextBoxThemeComboBoxItem;
            }

            string titleBarColor = localSettings.Values["titleBarColor"].ToString();
            if (titleBarColor == "0")
            {
                ThemeColorRadioButton.IsChecked = true;
            }
            else
            {
                AccentColorRadioButton.IsChecked = true;
            }

            /*
            string highContrast = localSettings.Values["highContrast"].ToString();
            if (highContrast == "1")
            {
                HighContrastToggleSwitch.IsOn = true;
                TextBoxThemeComboBox.IsEnabled = false;
            }
            else
            {
                HighContrastToggleSwitch.IsOn = false;
                TextBoxThemeComboBox.IsEnabled = true;
            }
            */

            string[] fonts = Microsoft.Graphics.Canvas.Text.CanvasTextFormat.GetSystemFontFamilies();
            for (int i = 0; i < fonts.Length; i++)
            {
                ComboBoxItem item = new ComboBoxItem();
                item.Name = fonts.GetValue(i).ToString();
                item.FontFamily = new FontFamily(fonts.GetValue(i).ToString());
                item.Content = fonts.GetValue(i);
                FontTypeComboBox.Items.Add(item);
            }

            string FontType = localSettings.Values["FontFamily"].ToString();
            FontTypeComboBox.PlaceholderText = FontType;

            string SearchEngine = localSettings.Values["SearchEngine"].ToString();
            if (SearchEngine == "Bing")
            {
                BingRadioButton.IsChecked = true;
            }
            else if (SearchEngine == "Google")
            {
                GoogleRadioButton.IsChecked = true;
            }
            else if (SearchEngine == "Yahoo")
            {
                YahooRadioButton.IsChecked = true;
            }

            if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 3)
                && ApiInformation.IsTypePresent("Windows.Phone.Devices.Notification.VibrationDevice"))
            {
                string VibrateBool = localSettings.Values["vibrate"].ToString();
                if (VibrateBool == "1")
                {
                    VibrateToggleSwitch.IsOn = true;
                }
                else
                {
                    VibrateToggleSwitch.IsOn = false;
                }
            }
            else
            {
                VibrateToggleSwitch.Visibility = Visibility.Collapsed;
                VibrateNotAvailableTextBlock.Visibility = Visibility.Visible;
            }

            string trimNewLines = localSettings.Values["trimNewLines"].ToString();
            if (trimNewLines == "0")
            {
                TrimNewLinesComboBox.SelectedItem = NoneTrimNewLinesComboBoxItem;
            }
            else if (trimNewLines == "1")
            {
                TrimNewLinesComboBox.SelectedItem = TrimLeadingTrimNewLinesComboBoxItem;
            }
            else if (trimNewLines == "2")
            {
                TrimNewLinesComboBox.SelectedItem = TrimTrailingTrimNewLinesComboBoxItem;
            }
            else if (trimNewLines == "3")
            {
                TrimNewLinesComboBox.SelectedItem = TrimBothTrimNewLinesComboBoxItem;
            }

            string hotExit = localSettings.Values["hotExit"].ToString();
            if (hotExit == "0")
            {
                HotExitComboBox.SelectedItem = NoneHotExitComboBoxItem;
            }
            else if (hotExit == "1")
            {
                HotExitComboBox.SelectedItem = UnsavedOnlyHotExitComboBoxItem;
            }

            Package package = Package.Current;
            PackageId packageId = package.Id;
            PackageVersion version = packageId.Version;
            ProgramVersionTextBlock.Text = string.Format("{0} {1} {2}.{3}.{4}.{5}", package.DisplayName, 
                packageId.Architecture, version.Major, version.Minor, version.Build, version.Revision);
            CopyrightTextBlock.Text = string.Format("© 2018-2019 {0}", package.PublisherDisplayName);

            string FlagsBool = localSettings.Values["flagsEnabled"].ToString();
            if(FlagsBool == "1")
            {
                FindName(nameof(ForDevelopersButton));
            }

            if (Microsoft.Services.Store.Engagement.StoreServicesFeedbackLauncher.IsSupported())
            {
                this.FeedbackButton.Visibility = Visibility.Visible;
            }
        }

        private void WindowsDefaultRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            string theme = localSettings.Values["theme"].ToString();
            if (theme == "Dark" || theme == "Light")
            {
                NoteChangeTextBlock.Visibility = Visibility.Visible;
            }

            if (ApiInformation.IsTypePresent("Windows.UI.Xaml.Media.AcrylicBrush"))
            {
                TransparencyToggleSwitch.Visibility = Visibility.Visible;
            }

            localSettings.Values["theme"] = "WD";
        }

        private void TransparencyToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            if (TransparencyToggleSwitch.IsOn == true)
            {
                localSettings.Values["transparency"] = "1";
            }
            else
            {
                localSettings.Values["transparency"] = "0";
            }
        }

        private void LightRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            string theme = localSettings.Values["theme"].ToString();
            if (theme == "WD" || theme == "Dark")
            {
                NoteChangeTextBlock.Visibility = Visibility.Visible;
            }

            TransparencyToggleSwitch.Visibility = Visibility.Collapsed;

            localSettings.Values["theme"] = "Light";
        }

        private void DarkRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            string theme = localSettings.Values["theme"].ToString();
            if (theme == "WD" || theme == "Light")
            {
                NoteChangeTextBlock.Visibility = Visibility.Visible;
            }

            TransparencyToggleSwitch.Visibility = Visibility.Collapsed;

            localSettings.Values["theme"] = "Dark";
        }

        private void TextBoxThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(LightTextBoxThemeComboBoxItem.IsSelected == true)
            {
                localSettings.Values["TextBoxTheme"] = "Light";
            }
            else if(DarkTextBoxThemeComboBoxItem.IsSelected == true)
            {
                localSettings.Values["TextBoxTheme"] = "Dark";
            }
        }

        private void ThemeColorRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            string titleBarColor = localSettings.Values["titleBarColor"].ToString();
            if(titleBarColor == "1")
            {
                NoteChangeTextBlock.Visibility = Visibility.Visible;
            }

            localSettings.Values["titleBarColor"] = "0";
        }

        private void AccentColorRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            string titleBarColor = localSettings.Values["titleBarColor"].ToString();
            if (titleBarColor == "0")
            {
                NoteChangeTextBlock.Visibility = Visibility.Visible;
            }

            localSettings.Values["titleBarColor"] = "1";
        }

        private void HighContrastToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            if (HighContrastToggleSwitch.IsOn == true)
            {
                localSettings.Values["highContrast"] = "1";
                TextBoxThemeComboBox.IsEnabled = false;
            }
            else
            {
                localSettings.Values["highContrast"] = "0";
                TextBoxThemeComboBox.IsEnabled = true;
            }
        }

        private void FontTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            localSettings.Values["FontFamily"] = ((ComboBoxItem)FontTypeComboBox.SelectedItem).Content;
        }

        private void BingRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            localSettings.Values["SearchEngine"] = "Bing";
        }

        private void GoogleRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            localSettings.Values["SearchEngine"] = "Google";
        }

        private void YahooRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            localSettings.Values["SearchEngine"] = "Yahoo";
        }

        private void VibrateToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            if (VibrateToggleSwitch.IsOn == true)
            {
                localSettings.Values["vibrate"] = "1";
            }
            else
            {
                localSettings.Values["vibrate"] = "0";
            }
        }

        private void TrimNewLinesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NoneTrimNewLinesComboBoxItem.IsSelected == true)
            {
                localSettings.Values["trimNewLines"] = "0";
            }
            else if (TrimLeadingTrimNewLinesComboBoxItem.IsSelected == true)
            {
                localSettings.Values["trimNewLines"] = "1";
            }
            else if (TrimTrailingTrimNewLinesComboBoxItem.IsSelected == true)
            {
                localSettings.Values["trimNewLines"] = "2";
            }
            else if (TrimBothTrimNewLinesComboBoxItem.IsSelected == true)
            {
                localSettings.Values["trimNewLines"] = "3";
            }
        }

        private void HotExitComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NoneHotExitComboBoxItem.IsSelected == true)
            {
                localSettings.Values["hotExit"] = "0";
            }
            else if (UnsavedOnlyHotExitComboBoxItem.IsSelected == true)
            {
                localSettings.Values["hotExit"] = "1";
            }
        }

        private async void FeedbackButton_Click(object sender, RoutedEventArgs e)
        {
            var launcher = Microsoft.Services.Store.Engagement.StoreServicesFeedbackLauncher.GetDefault();
            await launcher.LaunchAsync();
        }

        private void ProgramVersionTextBlock_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            /*
            string FlagsBool = localSettings.Values["flagsEnabled"].ToString();
            if (FlagsBool == "0")
            {
                if (ClickCounter <= 4)
                {
                    timer.Start();
                    ClickCounter++;
                }
                else
                {
                    Frame.Navigate(typeof(FlagsPage));
                    localSettings.Values["flagsEnabled"] = "1";
                }
            }*/
        }

        private void ForDevelopersButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(FlagsPage));
        }

        #region "Methods"
        private bool On_BackRequested()
        {
            if (this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
                return true;
            }
            return false;
        }
        #endregion

        #region Design Methods
        private void MakeDesign()
        {
            var titleBar = ApplicationView.GetForCurrentView().TitleBar;
            string theme = localSettings.Values["theme"].ToString();
            string titleBarColor = localSettings.Values["titleBarColor"].ToString();

            var BasicBackBrush = 
                Resources["ApplicationPageBackgroundThemeBrush"] as Brush;

            if (titleBarColor == "0")
            {
                MiddleAppTitleBar.Background = BasicBackBrush;
            }
            else
            {
                BasicAccentBrush();
            }

            SettingsContentGrid.Background = BasicBackBrush;

            if (theme == "WD")
            {
                if (ApiInformation.IsTypePresent("Windows.UI.Xaml.Media.AcrylicBrush"))
                {
                    var AcrylicSystemBrush =
                        Resources["SystemControlChromeMediumAcrylicWindowMediumBrush"] as AcrylicBrush;

                    var AcrylicAccentBrush =
                        Resources["SystemControlAccentAcrylicWindowAccentMediumHighBrush"] as AcrylicBrush;

                    if (titleBarColor == "0")
                    {
                        LeftAppTitleBar.Background = AcrylicSystemBrush;
                        MiddleAppTitleBar.Background = AcrylicSystemBrush;
                    }
                    else
                    {
                        titleBar.BackgroundColor = Resources["SystemAccentColor"] as Color?;
                        MiddleAppTitleBar.RequestedTheme = ElementTheme.Dark;
                        MiddleAppTitleBar.Background = AcrylicAccentBrush;
                    }

                    string TransparencyBool = localSettings.Values["transparency"].ToString();
                    if (TransparencyBool == "1")
                    {
                        SettingsContentGrid.Background = AcrylicSystemBrush;
                    }
                }
            }
        }

        private void BasicAccentBrush()
        {
            var titleBar = ApplicationView.GetForCurrentView().TitleBar;

            var BasicAccentBrush = Resources["SystemControlBackgroundAccentBrush"] as Brush;

            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.BackgroundColor = Resources["SystemAccentColor"] as Color?;
            LeftAppTitleBar.RequestedTheme = ElementTheme.Dark;
            LeftAppTitleBar.Background = BasicAccentBrush;
            MiddleAppTitleBar.RequestedTheme = ElementTheme.Dark;
            MiddleAppTitleBar.Background = BasicAccentBrush;
        }
        #endregion
    }
}
