using System;
using System.IO;
using Windows.ApplicationModel.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Text;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Foundation.Metadata;
using Windows.UI.Core;
using System.Threading.Tasks;
using Windows.Storage.AccessCache;
using Windows.UI.Xaml.Media;
using Windows.UI.ViewManagement;
using Windows.ApplicationModel.Resources;
using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics.Printing;
using Windows.UI.Xaml.Printing;
using Windows.ApplicationModel;
using Windows.System;
using Windows.Foundation;
using System.Collections.ObjectModel;
using Windows.UI.Input;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.Storage.Streams;
using System.Text;
using System.Collections.Generic;
using Windows.Phone.UI.Input;
using Windows.Graphics.Display;
using Windows.ApplicationModel.Email;
using UnitedCodebase.Classes;
using UnitedCodebase.Brushes;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml.Media.Animation;

namespace Textie
{
    public sealed partial class MainPage : Page
    {
        SystemNavigationManager currentView = SystemNavigationManager.GetForCurrentView();

        ApplicationView appView = ApplicationView.GetForCurrentView();
        CoreApplicationViewTitleBar coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
        ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;

        public RichEditBoxCore currentEditBox;

        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        private PrintManager printMan;
        private PrintDocument printDoc;
        private IPrintDocumentSource printDocSource;

        private bool IsCachedFilesAreOpened = false;
        private Type NavigatedPageType;

        ObservableCollection<Item> dataList = new ObservableCollection<Item>();

        DispatcherTimer timer;
        DispatcherTimer unSavedTimer;

        StorageFolder unsavedFolder;

        RichEditBoxMenu EditMenu;

        public MainPage()
        {
            this.InitializeComponent();

            coreTitleBar.ExtendViewIntoTitleBar = true;

            string titleBarColor = localSettings.Values["titleBarColor"].ToString();
    
            Window.Current.CoreWindow.Activated += CoreWindow_Activated;
            Window.Current.CoreWindow.SizeChanged += CoreWindow_SizeChanged;

            InputPane pane = InputPane.GetForCurrentView();
            pane.Showing += Pane_Showing;
            pane.Hiding += Pane_Hiding;

            if (ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
            {
                HardwareButtons.CameraHalfPressed += HardwareButtons_CameraHalfPressed;
            }

            this.NavigationCacheMode = NavigationCacheMode.Required;

            //PC customization
            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.ApplicationView"))
            {
                if (titleBar != null)
                {
                    ContentGrid.Margin = new Thickness(0, coreTitleBar.Height, 0, 0);
                    FindName(nameof(LeftAppTitleBar));
                    Window.Current.SetTitleBar(MiddleAppTitleBar);
                }
            }

            coreTitleBar.LayoutMetricsChanged += coreTitleBar_LayoutMetricsChanged;

            //Mobile customization
            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                StatusBar statusBar = StatusBar.GetForCurrentView();
                if (statusBar != null)
                {
                    statusBar.BackgroundOpacity = 0;

                    string theme = localSettings.Values["theme"].ToString();
                    if (theme != "WD")
                    {
                        if (theme == "Dark")
                        {
                            statusBar.ForegroundColor = Colors.LightGray;
                        }
                        else if (theme == "Light")
                        {
                            statusBar.ForegroundColor = Colors.DarkSlateGray;
                        }
                    }

                    TitleTextBlock.FontSize = 13;
                    TitleTextBlock.Margin = new Thickness(0, 0.6, 1, 0);
                    MiddleAppTitleBar.Margin = new Thickness(0, -statusBar.OccludedRect.Height, 0, 0);
                    MiddleAppTitleBar.Height = statusBar.OccludedRect.Height;

                    ContentGrid.Margin = new Thickness(0, statusBar.OccludedRect.Top, 0, 0);
                }
            }

            appView.VisibleBoundsChanged += appView_VisibleBoundsChanged;

            TitleTextBlock.Text = ApplicationView.GetForCurrentView().Title;

            string TabBarPosition = localSettings.Values["TabBarPosition"].ToString();
            if (TabBarPosition == "1")
            {
                PivotMain.Style = Application.Current.Resources["PivotHeaderBottomStyle"] as Style;
            }
            else
            {
                PivotMain.Style = null;
            }

            EditMenu = new RichEditBoxMenu();

            timer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(2) };
            timer.Tick += PeriodicSave;

            unSavedTimer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(2) };
            unSavedTimer.Tick += CacheUnsavedFiles;

            Style EditMenuStyle = new Style { TargetType = typeof(MenuFlyoutPresenter) };
            EditMenuStyle.Setters.Add(new Setter(RequestedThemeProperty,
                MainGrid.RequestedTheme));
            EditMenu.ContextFlyout.MenuFlyoutPresenterStyle = EditMenuStyle;

            string highContrast = localSettings.Values["highContrast"].ToString();
            if (highContrast == "0")
            {
                MakeDesign();
            }
            /*else
            {
                Accessibility();
            }*/

            MakeKeyAccelerators();

            ApiResources.NotifyUpdate(new Uri("https://www.dropbox.com/s/61jtlb9dnwcx3h8/New-Version.txt?dl=1"));
        }

        private void coreTitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args)
        {
            if (!appView.IsFullScreenMode)
            {
                ContentGrid.Margin = new Thickness(0, sender.Height, 0, 0);
            }

            TitleTextBlock.Margin = new Thickness(0, 5.5, 64, 0);

            MiddleAppTitleBar.Margin = new Thickness(64, 0, 0, 0);
            MiddleAppTitleBar.Height = sender.Height;
        }

        private void appView_VisibleBoundsChanged(ApplicationView sender, object args)
        {
            MainCommandBar.IsOpen = false;

            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar") && !sender.IsFullScreenMode)
            {
                StatusBar statusBar = StatusBar.GetForCurrentView();

                MiddleAppTitleBar.Height = statusBar.OccludedRect.Height;
                MiddleAppTitleBar.Width = statusBar.OccludedRect.Width;

                TitleTextBlock.Visibility = Visibility.Collapsed;

                DisplayInformation displayInformation = DisplayInformation.GetForCurrentView();
                if (displayInformation.CurrentOrientation == DisplayOrientations.Landscape)
                {
                    MiddleAppTitleBar.HorizontalAlignment = HorizontalAlignment.Left;
                    MiddleAppTitleBar.Margin = new Thickness(-MiddleAppTitleBar.Width, -sender.VisibleBounds.Top, 0, -sender.VisibleBounds.Bottom);
                }
                else if (displayInformation.CurrentOrientation == DisplayOrientations.LandscapeFlipped)
                {
                    MiddleAppTitleBar.HorizontalAlignment = HorizontalAlignment.Right;
                    MiddleAppTitleBar.Margin = new Thickness(0, -sender.VisibleBounds.Top, -MiddleAppTitleBar.Width, -sender.VisibleBounds.Bottom);
                }
                else
                {
                    MiddleAppTitleBar.HorizontalAlignment = HorizontalAlignment.Stretch;
                    MiddleAppTitleBar.Margin = new Thickness(0, -statusBar.OccludedRect.Height, 0, 0);
                    TitleTextBlock.Visibility = Visibility.Visible;
                }

                ContentGrid.Margin = new Thickness(0, statusBar.OccludedRect.Top, 0, 0);
            }
        }

        private void CoreWindow_Activated(CoreWindow sender, WindowActivatedEventArgs e)
        {
            string TextBoxTheme = localSettings.Values["TextBoxTheme"].ToString();
            string FontType = localSettings.Values["FontFamily"].ToString();

            string highContrast = localSettings.Values["highContrast"].ToString();
            foreach (RichEditBoxPivotItem item in PivotMain.Items)
            {
                if (highContrast == "1")
                {
                    item.EditBox.RequestedTheme = ElementTheme.Dark;
                }
                else
                {
                    if (TextBoxTheme == "Light")
                    {
                        item.EditBox.RequestedTheme = ElementTheme.Light;
                    }
                    else if (TextBoxTheme == "Dark")
                    {
                        item.EditBox.RequestedTheme = ElementTheme.Dark;
                    }
                }

                item.EditBox.FontFamily = new FontFamily(FontType);
            }

            ShareButton.IsEnabled = true;

            if (currentEditBox != null)
            {
                if (!appView.IsFullScreenMode)
                {
                    PivotMain.SelectedRichEditBoxItem.Margin = new Thickness(
                        0, 0, 0, 0);
                }

                currentEditBox.Focus(FocusState.Programmatic);
            }
        }

        private void CoreWindow_SizeChanged(CoreWindow sender, WindowSizeChangedEventArgs e)
        {
            if (appView.IsFullScreenMode)
            {
                if(PivotMain.Style == null)
                {
                    ContentGrid.Margin = new Thickness(0, -48, 0, 0);
                }
                else
                {
                    ContentGrid.Margin = new Thickness(0, 0, 0, -48);
                }

                ApplicationView.GetForCurrentView().SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow);

                LeftAppTitleBar.Opacity = 0;
                MiddleAppTitleBar.Opacity = 0;
            }
            else
            {
                ApplicationView.GetForCurrentView().SetDesiredBoundsMode(ApplicationViewBoundsMode.UseVisible);

                if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.ApplicationView"))
                {
                    if (titleBar != null)
                    {
                        ContentGrid.Margin = new Thickness(0, coreTitleBar.Height, 0, 0);
                    }
                }

                if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
                {
                    StatusBar statusBar = StatusBar.GetForCurrentView();
                    if (statusBar != null)
                    {
                        ContentGrid.Margin = new Thickness(0, statusBar.OccludedRect.Top, 0, 0);
                    }
                }

                e.Handled = true;

                LeftAppTitleBar.Opacity = 1;
                MiddleAppTitleBar.Opacity = 1;
            }
        }

        private void CurrentView_BackRequested(object sender, BackRequestedEventArgs e)
        {
            InputPane pane = InputPane.GetForCurrentView();

            e.Handled = true;

            if (appView.IsFullScreenMode)
            {
                appView.ExitFullScreenMode();
            }
            else
            {
                e.Handled = false;
            }
        }

        private void CoreWindow_PointerPressed(CoreWindow sender, PointerEventArgs args)
        {
            double FullScreenOverride = ContentGrid.Margin.Top + 48;
            if (appView.IsFullScreenMode)
            {
                FullScreenOverride = 0;
            }

            if (args.CurrentPoint.Properties.IsXButton1Pressed)
            {
                args.Handled = true;

                if (appView.IsFullScreenMode)
                {
                    appView.ExitFullScreenMode();
                }
                else
                {
                    args.Handled = false;
                }
            }

            string SelectionText = currentEditBox.Document.Selection.Text;
            if (args.CurrentPoint.Properties.IsMiddleButtonPressed && currentEditBox.FocusState == FocusState.Pointer && !string.IsNullOrEmpty(SelectionText) && args.CurrentPoint.Position.Y > FullScreenOverride)
            {
                if ((SelectionText.StartsWith("http://") || SelectionText.StartsWith("https://")) && SelectionText.Contains("."))
                {
                    WebResources.Navigate(SelectionText);
                }
            }
        }

        private void HardwareButtons_CameraHalfPressed(object sender, CameraEventArgs e)
        {
            InputPane pane = InputPane.GetForCurrentView();
            if (appView.IsFullScreenMode)
            {
                appView.ExitFullScreenMode();
            }
        }

        private void Pane_Showing(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            if (currentEditBox != null)
            {
                PivotMain.Margin = new Thickness(
                    0, 0, 0, args.OccludedRect.Height);
            }
        }

        private void Pane_Hiding(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            if (currentEditBox != null)
            {
                PivotMain.Margin = new Thickness(
                    0, 0, 0, args.OccludedRect.Height);
            }
        }

        private void DataTransferManager_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            DataRequest request = args.Request;

            request.Data.Properties.Title = "Text Share";
            request.Data.Properties.Description = "Share text from file";
            request.Data.SetText(currentEditBox.CoreText.Text);
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.ApplicationView"))
            {
                if (titleBar != null)
                {
                    Window.Current.SetTitleBar(MiddleAppTitleBar);
                }
            }
            //Mobile customization
            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                StatusBar statusBar = StatusBar.GetForCurrentView();
                if (statusBar != null)
                {
                    statusBar.BackgroundOpacity = 0;
                }
            }

            if (await ApplicationData.Current.LocalFolder.TryGetItemAsync("UsavedFiles") == null)
                await ApplicationData.Current.LocalFolder.CreateFolderAsync("UsavedFiles");

            unsavedFolder = await ApplicationData.Current.LocalFolder.GetFolderAsync("UsavedFiles");

            string hotExit = localSettings.Values["hotExit"].ToString();

            if (e.Parameter != null)
            {
                string activationArgs = e.Parameter.ToString();
                if (activationArgs != "NewWindow")
                {
                    if(IsCachedFilesAreOpened == false && hotExit == "1")
                    {
                        IReadOnlyList<StorageFile> files = await unsavedFolder.GetFilesAsync();
                        if (files.Count > 0)
                        {
                            foreach (StorageFile file in files)
                            {
                                await OpenFile(file, true);
                                await Task.Delay(200);
                            }
                        }
                        else
                        {
                            if (PivotMain.Items.Count == 0)
                            {
                                AddTab("New Tab " + PivotMain.Items.Count);
                            }
                        }

                        IsCachedFilesAreOpened = true;
                    }
                    else
                    {
                        if (PivotMain.Items.Count == 0)
                        {
                            AddTab("New Tab " + PivotMain.Items.Count);
                        }
                    }
                }
                else
                {
                    if (PivotMain.Items.Count == 0)
                    {
                        string uniqueName = await ApiResources.GetUniqueName("New Tab " + PivotMain.Items.Count, unsavedFolder);
                        AddTab("New Tab " + PivotMain.Items.Count + " " + uniqueName);
                    }
                }

                timer.Start();
                unSavedTimer.Start();

                if (activationArgs == "OpenFile" && NavigatedPageType != typeof(SettingsPage))
                {
                    OpenFile();
                }

                if (!string.IsNullOrEmpty(activationArgs) && activationArgs != "Windows.ApplicationModel.Activation.FileActivatedEventArgs"
                && activationArgs != "Windows.ApplicationModel.Activation.ProtocolActivatedEventArgs" && activationArgs != "Windows.ApplicationModel.Activation.ToastNotificationActivatedEventArgs"
                && activationArgs != "OpenFile" && NavigatedPageType != typeof(SettingsPage))
                {
                    StorageLibrary documentsLibrary = await ApiResources.TryAccessLibraryAsync(KnownLibraryId.Documents);
                    StorageLibrary musicLibrary = await ApiResources.TryAccessLibraryAsync(KnownLibraryId.Music);
                    StorageLibrary videoLibrary = await ApiResources.TryAccessLibraryAsync(KnownLibraryId.Videos);
                    StorageLibrary pictureLibrary = await ApiResources.TryAccessLibraryAsync(KnownLibraryId.Pictures);
                
                    if (documentsLibrary != null && musicLibrary != null && videoLibrary != null && pictureLibrary != null)
                    {
                        string[] libraries = new string[]
                        {
                            documentsLibrary.SaveFolder.Path,
                            musicLibrary.SaveFolder.Path,
                            videoLibrary.SaveFolder.Path,
                            pictureLibrary.SaveFolder.Path
                        };
                  
                        object FileFound = null;
                        foreach (string libraryPath in libraries)
                        {
                            if (activationArgs.Replace("_", " ").StartsWith(libraryPath, StringComparison.CurrentCultureIgnoreCase))
                            {
                                if (libraryPath == libraries[0])
                                {
                                    FileFound = await KnownFolders.DocumentsLibrary.TryGetItemAsync(Path.GetFileName(activationArgs).Replace("_", " "));
                                }
                                else if (libraryPath == libraries[1])
                                {
                                    FileFound = await KnownFolders.MusicLibrary.TryGetItemAsync(Path.GetFileName(activationArgs).Replace("_", " "));
                                }
                                else if (libraryPath == libraries[2])
                                {
                                    FileFound = await KnownFolders.VideosLibrary.TryGetItemAsync(Path.GetFileName(activationArgs).Replace("_", " "));
                                }
                                else if (libraryPath == libraries[3])
                                {
                                    FileFound = await KnownFolders.PicturesLibrary.TryGetItemAsync(Path.GetFileName(activationArgs).Replace("_", " "));
                                }

                                if (FileFound != null)
                                {
                                    StorageFile file = await StorageFile.GetFileFromPathAsync(activationArgs.Replace("_", " "));
                                    await OpenFile(file, false);

                                    break;
                                }
                                else
                                {
                                    ContentDialog fileNotFoundDialog = new ContentDialog()
                                    {
                                        Title = "File is not found!",
                                        Content = "\r\nTextie can't find the file at " + "\"" + activationArgs.Replace("_", " ") + "\".",
                                        PrimaryButtonText = "OK"
                                    };

                                    fileNotFoundDialog.PrimaryButtonClick += async (o, ee) =>
                                    {
                                        await AppTile.RequestUnPinSecondaryTile(Path.GetFileName(activationArgs));
                                    };

                                    await fileNotFoundDialog.ShowAsync();
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                AddTab("New Tab " + PivotMain.Items.Count);
            }

            IActivatedEventArgs args = e.Parameter as IActivatedEventArgs;
            if (args != null && NavigatedPageType != typeof(SettingsPage))
            {
                if (args.Kind == ActivationKind.File)
                {
                    FileActivatedEventArgs fileArgs = args as FileActivatedEventArgs;
                    foreach(StorageFile file in fileArgs.Files)
                    {
                        await Task.WhenAll(OpenFile(file, false));
                    }
                }

                if(args.Kind == ActivationKind.Protocol)
                {
                    ProtocolActivatedEventArgs protocolArgs = args as ProtocolActivatedEventArgs;
                    if(protocolArgs.Uri.AbsoluteUri == "textie:OpenFile")
                    {
                        OpenFile();
                    }
                }

                if(args.Kind == ActivationKind.ToastNotification)
                {
                    ToastNotificationActivatedEventArgs protocolArgs = args as ToastNotificationActivatedEventArgs;

                    if (ApiResources.GetQueryVariable(protocolArgs.Argument, "action") == "selecttab")
                    {
                        foreach (RichEditBoxPivotItem item in PivotMain.Items)
                        {
                            if (item.Header.ToString() == ApiResources.GetQueryVariable(protocolArgs.Argument, "tab").Replace("_", " "))
                            {
                                await Task.Delay(100);
                                PivotMain.SelectedItem = item;
                                break;
                            }
                        }
                    }

                    if (ApiResources.GetQueryVariable(protocolArgs.Argument, "action") == "savefile")
                    {
                        foreach (RichEditBoxPivotItem item in PivotMain.Items)
                        {
                            if (item.Header.ToString() == ApiResources.GetQueryVariable(protocolArgs.Argument, "file").Replace("_", " "))
                            {
                                PivotMain.SelectedItem = item;
                                await Task.Delay(300);
                                SaveFileAs(item, false);
                                break;
                            }
                        }
                    }
                }
            }

            string TextBoxTheme = localSettings.Values["TextBoxTheme"].ToString();
            string FontType = localSettings.Values["FontFamily"].ToString();

            foreach(RichEditBoxPivotItem item in PivotMain.Items)
            {
                string highContrast = localSettings.Values["highContrast"].ToString();
                if (highContrast == "1")
                {
                    item.EditBox.RequestedTheme = ElementTheme.Dark;
                }
                else
                {
                    if (TextBoxTheme == "Light")
                    {
                        item.EditBox.RequestedTheme = ElementTheme.Light;
                    }
                    else if (TextBoxTheme == "Dark")
                    {
                        item.EditBox.RequestedTheme = ElementTheme.Dark;
                    }
                }
                
                item.EditBox.FontFamily = new FontFamily(FontType);
            }

            currentView.BackRequested += CurrentView_BackRequested;
            Window.Current.CoreWindow.PointerPressed += CoreWindow_PointerPressed;

            SettingsButton.IsEnabled = true;

            try
            {
                if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 3)
                    && PrintManager.IsSupported())
                {
                    // Register for PrintTaskRequested event
                    printMan = PrintManager.GetForCurrentView();
                    printMan.PrintTaskRequested += PrintTaskRequested;

                    // Build a PrintDocument and register for callbacks
                    printDoc = new PrintDocument();
                    printDocSource = printDoc.DocumentSource;
                    printDoc.Paginate += Paginate;
                    printDoc.GetPreviewPage += GetPreviewPage;
                    printDoc.AddPages += AddPages;
                }
            }
            catch (Exception)
            {
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            NavigatedPageType = e.SourcePageType;

            MainCommandBar.IsOpen = false;

            timer.Stop();
            
            unSavedTimer.Stop();

            try
            {
                if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 3)
                    && PrintManager.IsSupported())
                {
                    printMan.PrintTaskRequested -= PrintTaskRequested;

                    printDoc.Paginate -= Paginate;
                    printDoc.GetPreviewPage -= GetPreviewPage;
                    printDoc.AddPages -= AddPages;

                    printMan = null;
                    printDoc = null;
                    printDocSource = null;
                }
            }
            catch (Exception)
            {

            }
        }

        #region "Grids"
        private void MainGrid_Loading(FrameworkElement sender, object args)
        {
            AddTabButton.Label = string.Empty;
            CloseTabButton.Label = string.Empty;
            OpenButton.Label = string.Empty;
            SaveButton.Label = string.Empty;
            EditButton.Label = string.Empty;

            ResourceLoader loader = ResourceLoader.GetForCurrentView();

            string AllTabsText = loader.GetString("AllTabsButton/Label");
            ToolTipService.SetToolTip(AllTabsButton, AllTabsText);

            string AddTabText = loader.GetString("AddTabButton/Label");
            ToolTipService.SetToolTip(AddTabButton, AddTabText + " (Ctrl + T)");

            string CloseTabText = loader.GetString("CloseTabButton/Label");
            ToolTipService.SetToolTip(CloseTabButton, CloseTabText + " (Ctrl + W)");

            string OpenText = loader.GetString("OpenButton/Label");
            ToolTipService.SetToolTip(OpenButton, OpenText + " (Ctrl + O)");

            string SaveText = loader.GetString("SaveButton/Label");
            ToolTipService.SetToolTip(SaveButton, SaveText + " (Ctrl + S)");

            string EditText = loader.GetString("EditButton/Label");
            ToolTipService.SetToolTip(EditButton, EditText);

            if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 3)
                && PrintManager.IsSupported())
            {
                PrintButton.Visibility = Visibility.Visible;
            }
            else
            {
                PrintButton.Visibility = Visibility.Collapsed;
            }

            string autoSave = localSettings.Values["autoSave"].ToString();
            if (autoSave == "0")
            {
                AutoSaveToggle.IsChecked = false;
            }
            else
            {
                AutoSaveToggle.IsChecked = true;
            }

            AddRecentFiles();
        }

        private void ContentGrid_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.F11)
            {
                if (appView.IsFullScreenMode)
                {
                    appView.ExitFullScreenMode();
                }
                else
                {
                    ShowFullScreen();
                }
            }

            CoreVirtualKeyStates ctrl = Window.Current.CoreWindow.GetKeyState(VirtualKey.Control);
            if (ctrl.HasFlag(CoreVirtualKeyStates.Down) && e.Key == VirtualKey.F)
            {
                FindFlyout.ShowAt(MainCommandBar);
            }

            if (ctrl.HasFlag(CoreVirtualKeyStates.Down) && e.Key == VirtualKey.H)
            {
                ReplaceFlyout.ShowAt(MainCommandBar);
            }

            if (ctrl.HasFlag(CoreVirtualKeyStates.Down) && e.Key == VirtualKey.Subtract)
            {
                ZoomOut();
            }

            if (ctrl.HasFlag(CoreVirtualKeyStates.Down) && e.Key == VirtualKey.Add)
            {
                ZoomIn();
            }
        }

        private void ContentGrid_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            int delta = e.GetCurrentPoint(ContentGrid).Properties.MouseWheelDelta;
            if(delta >= 0)
            {
                if(e.KeyModifiers == VirtualKeyModifiers.Control)
                {
                    ZoomIn();
                }
            }
            else
            {
                if(e.KeyModifiers == VirtualKeyModifiers.Control)
                {
                    ZoomOut();
                }
            }
        }
        #endregion

        #region "Events"

        #region "Command bar"

        bool CommandBarPanelOpened = false;
        private void MainCommandBar_Opening(object sender, object e)
        {
            if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 4))
            {
                SecondaryCommands.OverlayInputPassThroughElement = PivotMain;
            }

            Button focusedButton = FocusManager.GetFocusedElement() as Button;
            if (CommandBarPanelOpened == false)
            {
                if (focusedButton != null && focusedButton.Name == "MoreButton")
                {
                    SecondaryCommands.ShowAt(focusedButton);
                }
                InputPane.GetForCurrentView().TryHide();
            }
        }

        private void MainCommandBar_Closing(object sender, object e)
        {
            if(CommandBarPanelOpened == true)
            {
                if (appView.IsFullScreenMode)
                {
                    currentEditBox.Focus(FocusState.Programmatic);
                }

                SecondaryCommands.Hide();
            }
        }

        private void SecondaryCommands_Opened(object sender, object e)
        {
            CommandBarPanelOpened = true;
        }

        private void SecondaryCommands_Closed(object sender, object e)
        {
            CommandBarPanelOpened = false;
            Button focusedButton = FocusManager.GetFocusedElement() as Button;
            if( focusedButton == null || (focusedButton != null && (focusedButton.Name != "MoreButton" || focusedButton.Name == "MoreButton" && !focusedButton.IsPressed)))
            {
                MainCommandBar.IsOpen = false;
            }
        }

        #region "Command bar buttons"

        private void NewWindowButton_Click(object sender, RoutedEventArgs e)
        {
            NewWindow();
        }

        private void AddTabButton_Click(object sender, RoutedEventArgs e)
        {
            AddTab("New Tab " + PivotMain.Items.Count);
        }

        private void CloseTabButton_Click(object sender, RoutedEventArgs e)
        {
            CloseOneTab(PivotMain.SelectedRichEditBoxItem);
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFile();
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentEditBox.Tag != null)
            {
                await SaveFile(PivotMain.SelectedRichEditBoxItem, (StorageFile)currentEditBox.Tag);
            }
            else
            {
                SaveFileAs(PivotMain.SelectedRichEditBoxItem, false);
            }
        }

        private void RedoButton_Click(object sender, RoutedEventArgs e)
        {
            currentEditBox.Document.Redo();
        }

        private void UndoButton_Click(object sender, RoutedEventArgs e)
        {
            currentEditBox.Document.Undo();
        }

        private void InsertButton_Click(object sender, RoutedEventArgs e)
        {
            InsertSelection.ShowAt(MainCommandBar);
        }

        private void RecentFilesButton_Click(object sender, RoutedEventArgs e)
        {
            RecentFilesFlyout.ShowAt(MainCommandBar);
        }

        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            Print();
        }

        private void ShareButton_Click(object sender, RoutedEventArgs e)
        {
            ShareButton.IsEnabled = false;

            ShareText();
        }

        private void SaveAsButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileAs(PivotMain.SelectedRichEditBoxItem, false);
        }

        private async void SaveAllButton_Click(object sender, RoutedEventArgs e)
        {
            ItemCollection pages = PivotMain.Items;
            foreach (RichEditBoxPivotItem page in pages)
            {
                RichEditBoxCore richEdit = page.EditBox;
                if (richEdit.Tag != null)
                {
                    await SaveFile(page, (StorageFile)richEdit.Tag);
                }
                else
                {
                    UCNotification notification = new UCNotification("Save file", string.Format("\"{0}\" is can't pin because is not have a saved file.", page.Header));
                    notification.ToastLaunchArguments = "action=selecttab&tab=" + page.ListViewItem.CopiedFileName.Replace(" ", "_");
                    notification.ToastButtonContent = "Save file";
                    notification.ToastButtonArguments = "action=savefile&file=" + page.ListViewItem.CopiedFileName.Replace(" ", "_");
                    notification.ShowNotification();
                }
            }
        }

        private void AutoSaveToggle_Checked(object sender, RoutedEventArgs e)
        {
            localSettings.Values["autoSave"] = "1";
        }

        private void AutoSaveToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            localSettings.Values["autoSave"] = "0";
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            EditMenu.ContextFlyout.ShowAt(MainCommandBar);
        }

        private void FindButton_Click(object sender, RoutedEventArgs e)
        {
            FindFlyout.ShowAt(MainCommandBar);
        }

        private void ReplaceButton_Click(object sender, RoutedEventArgs e)
        {
            ReplaceFlyout.ShowAt(MainCommandBar);
        }

        private void FormatButton_Click(object sender, RoutedEventArgs e)
        {
            FormatSelection.ShowAt(MainCommandBar);
        }

        private void LaunchButton_Click(object sender, RoutedEventArgs e)
        {
            WebResources.Launch((StorageFile)currentEditBox.Tag);
        }

        private void ZoomOutButton_Click(object sender, RoutedEventArgs e)
        {
            ZoomOut();
        }

        private void ZoomInButton_Click(object sender, RoutedEventArgs e)
        {
            ZoomIn();
        }

        private void FullScreenButton_Click(object sender, RoutedEventArgs e)
        {
            ShowFullScreen();
        }

        private void PinButton_Click(object sender, RoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout(sender as FrameworkElement);
        }

        private void ExtensionsButton_Click(object sender, RoutedEventArgs e)
        {
            Extensions.IsPaneOpen = true;
            SecondaryCommands.Hide();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            SettingsButton.IsEnabled = false;

            this.Frame.Navigate(typeof(SettingsPage), null, new SuppressNavigationTransitionInfo());
        }
        #endregion

        #endregion

        #region "Items context flyout"

        private void HeaderTextBlock_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout(sender as FrameworkElement);
        }

        private void AddTabItem_Click(object sender, RoutedEventArgs e)
        {
            AddTab("New Tab " + PivotMain.Items.Count);
        }

        private void CloseTabItem_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem button = sender as MenuFlyoutItem;
            foreach (RichEditBoxPivotItem item in PivotMain.Items)
            {
                if (item.Header.ToString() == button.DataContext.ToString())
                {
                    CloseOneTab(item);
                    break;
                }
            }
        }

        private void CloseAllTabsItem_Click(object sender, RoutedEventArgs e)
        {
            CloseAllTabs();
        }

        private void CloseOtherTabsItem_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem button = sender as MenuFlyoutItem;
            foreach (RichEditBoxPivotItem item in PivotMain.Items)
            {
                PivotMain.SelectedItem = item;

                if (PivotMain.SelectedRichEditBoxItem.Header.ToString() != button.DataContext.ToString())
                {
                    PivotMain.CloseTab(PivotMain.SelectedRichEditBoxItem);
                }
                else
                {
                    continue;
                }

                if(PivotMain.Items.Count == 1)
                {
                    break;
                }
            }
        }

        #endregion

        #region "All Tabs Flyout"

        private void AllTabsButton_Click(object sender, RoutedEventArgs e)
        {
            AllTabsFlyout.ShowAt(AllTabsButton);
        }

        private void AllTabsFlyout_Opening(object sender, object e)
        {
            FindName(nameof(AllTabsList));

            AllTabsList.ItemsSource = dataList;
            AllTabsList.SelectedIndex = PivotMain.SelectedIndex;
        }

        private void AllTabsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AllTabsList.SelectedIndex != -1)
            {
                Item item = e.AddedItems[0] as Item;

                PivotMain.SelectedItem = item.PivotItem;

                AllTabsList.SelectedIndex = PivotMain.SelectedIndex;
            }
        }

        private void TextBlock_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            TextBlock senderText = sender as TextBlock;
            SolidColorBrush redBrush = new SolidColorBrush(Colors.Red);
            senderText.Foreground = redBrush;
        }

        private void TextBlock_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            TextBlock senderText = sender as TextBlock;
            SolidColorBrush whiteBrush = new SolidColorBrush(Colors.White);
            senderText.Foreground = whiteBrush;
        }

        Item senderItem;
        private async void ListViewItemGrid_Holding(object sender, HoldingRoutedEventArgs e)
        {
            if (e.HoldingState == HoldingState.Completed)
            {
                await Task.Delay(100);
                FlyoutBase.GetAttachedFlyout(sender as FrameworkElement).ShowAt(AllTabsButton);
                senderItem = (sender as Grid).DataContext as Item;
                ApiResources.Vibrate(50);
            }
        }

        private void ListViewItemGrid_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout(sender as FrameworkElement);
            e.Handled = true;
        }

        private void CloseTabAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            AppBarButton button = sender as AppBarButton;
            Item item = button.DataContext as Item;
            CloseOneTab(item.PivotItem);
        }

        private async void SaveFileMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem button = sender as MenuFlyoutItem;
            Item item = button.DataContext as Item;
            if (item == null)
            {
                item = senderItem;
            }

            string autoSave = localSettings.Values["autoSave"].ToString();
            if (autoSave == "0")
            {
                if (item.File != null && item.CopiedFile == null)
                {
                    await SaveFile(item.PivotItem, item.File);
                }
                else
                {
                    SaveFileAs(item.PivotItem, false);
                }
            }
            else
            {
                if (item.File == null && item.CopiedFile != null)
                {
                    SaveFileAs(item.PivotItem, false);
                }
            }
        }

        private void CloseTabMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem button = sender as MenuFlyoutItem;
            Item item = button.DataContext as Item;
            if (item == null)
            {
                item = senderItem;
            }

            CloseOneTab(item.PivotItem);
        }

        private void SendFileEmailMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem button = sender as MenuFlyoutItem;
            Item item = button.DataContext as Item;
            if (item == null)
            {
                item = senderItem;
            }

            if (item.File != null)
            {
                SendEmail(item.File);
            }
            else
            {
                UCNotification notification = new UCNotification("Save file", string.Format("\"{0}\" is can't pin because is not have a saved file.", item.Title));
                notification.ToastLaunchArguments = "action=selecttab&tab=" + item.CopiedFileName.Replace(" ", "_");
                notification.ToastButtonContent = "Save file";
                notification.ToastButtonArguments = "action=savefile&file=" + item.CopiedFileName.Replace(" ", "_");
                notification.ShowNotification();
            }
        }

        private void FileInfoMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem button = sender as MenuFlyoutItem;
            Item item = button.DataContext as Item;
            if(item == null)
            {
                item = senderItem;
            }

            if (item.File != null)
            {
                item.ShowFileInfoDialog();
            }
        }

        #endregion

        private async void mruMenuFlyout_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem selectedItem = sender as MenuFlyoutItem;
            StorageItemMostRecentlyUsedList mru = StorageApplicationPermissions.MostRecentlyUsedList;
            IAsyncOperation<StorageFile> asyncOperation = mru.GetFileAsync(selectedItem.Tag.ToString());
            try
            {
                if (mru.CheckAccess(await asyncOperation))
                {
                    StorageFile file = await asyncOperation;
                    await OpenFile(file, false);
                }
            }
            catch (FileNotFoundException)
            {
                ContentDialog fileNotFoundDialog = new ContentDialog()
                {
                    Title = "File error",
                    Content = "\r\nFile is not found!",
                    PrimaryButtonText = "OK"
                };

                await fileNotFoundDialog.ShowAsync();

                StorageApplicationPermissions.MostRecentlyUsedList.Remove(selectedItem.Tag.ToString());

                AddRecentFiles();
            }
        }

        private void TrimLeadingSpaceItem_Click(object sender, RoutedEventArgs e)
        {
            currentEditBox.Trim(TrimingOptions.TrimStart);
        }

        private void TrimTrailingSpaceItem_Click(object sender, RoutedEventArgs e)
        {
            currentEditBox.Trim(TrimingOptions.TrimEnd);
        }

        private void TrimLeadingnTrailingSpaceItem_Click(object sender, RoutedEventArgs e)
        {
            currentEditBox.Trim(TrimingOptions.Trim);
        }

        private void WordWrapItem_Click(object sender, RoutedEventArgs e)
        {
            if (WordWrapItem.IsChecked == true)
            {
                currentEditBox.TextWrapping = TextWrapping.Wrap;
            }
            else
            {
                currentEditBox.TextWrapping = TextWrapping.NoWrap;
            }
        }

        private void TabItem_Click(object sender, RoutedEventArgs e)
        {
            currentEditBox.Document.Selection.SetText(TextSetOptions.None, "\t");
        }

        private void DateAndTimeItem_Click(object sender, RoutedEventArgs e)
        {
            currentEditBox.Document.Selection.SetText(TextSetOptions.None, 
                DateTime.Now.ToString());
        }

        private void FileNameItem_Click(object sender, RoutedEventArgs e)
        {
            currentEditBox.Document.Selection.SetText(TextSetOptions.None,
                ((StorageFile)currentEditBox.Tag).Name);
        }

        private void FilePathItem_Click(object sender, RoutedEventArgs e)
        {
            currentEditBox.Document.Selection.SetText(TextSetOptions.None,
                Path.GetDirectoryName(((StorageFile)currentEditBox.Tag).Path));
        }

        private async void HomeItem_Click(object sender, RoutedEventArgs e)
        {
            currentEditBox.Document.Selection.SetRange(0, 0);
            await Task.Delay(100);
            currentEditBox.Focus(FocusState.Programmatic);
        }

        private async void EndItem_Click(object sender, RoutedEventArgs e)
        {
            currentEditBox.Document.Selection.SetRange(currentEditBox.CoreText.Text.Length, currentEditBox.CoreText.Text.Length);
            await Task.Delay(100);
            currentEditBox.Focus(FocusState.Programmatic);
        }

        private void PinFileStartMenuItem_Click(object sender, RoutedEventArgs e)
        {
            PinFileToStartMenu();
        }

        private void PivotMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (appView.IsFullScreenMode)
            {
                appView.ExitFullScreenMode();
            }

            if (PivotMain.SelectedRichEditBoxItem != null)
            {
                currentEditBox = PivotMain.SelectedRichEditBox;

                EditMenu.Core = currentEditBox;

                currentEditBox.Margin = new Thickness(0, 0, 0, 0);

                StorageFile file = (StorageFile)currentEditBox.Tag;
                if (currentEditBox.Tag == null)
                {
                    if (PivotMain.SelectedRichEditBoxItem.Header != null)
                    {
                        string ItemHeader = PivotMain.SelectedRichEditBoxItem.Header.ToString();
                        appView.Title = ItemHeader;
                    }
                }
                else
                {
                    string ItemHeader = Path.GetFileName(file.Path);
                    DisplayInformation displayInformation = DisplayInformation.GetForCurrentView();
                    if (displayInformation.DiagonalSizeInInches < 5.0 || displayInformation.ResolutionScale > ResolutionScale.Scale400Percent)
                    {
                        if (ItemHeader.Length < 11)
                        {
                            appView.Title = ItemHeader;
                        }
                        else
                        {
                            appView.Title = string.Format("{0} ... {1}", ItemHeader.Substring(0, 6), ItemHeader.Substring(ItemHeader.Length - 3));
                        }
                    }
                    else
                    {
                        if (ItemHeader.Length < 18)
                        {
                            appView.Title = ItemHeader;
                        }
                        else
                        {
                            appView.Title = string.Format("{0} ... {1}", ItemHeader.Substring(0, 13), ItemHeader.Substring(ItemHeader.Length - 3));
                        }
                    }

                    if (file.Path.EndsWith(".html") || file.Path.EndsWith(".htm"))
                    {
                        LaunchButton.IsEnabled = true;
                    }
                }

                string autoSave = localSettings.Values["autoSave"].ToString();
                if (autoSave == "0" || (currentEditBox.Tag == null && autoSave == "1"))
                {
                    SaveButton.IsEnabled = currentEditBox.TextSaveState == RichEditBoxCore.SaveState.TextIsChanged;
                }
                else
                {
                    SaveButton.IsEnabled = false;
                }
            }

            TitleTextBlock.Text = string.Format("{0} – {1}", appView.Title,
                Package.Current.DisplayName);

            if (PivotMain.Items.Count <= 1)
            {
                CloseTabButton.IsEnabled = false;
                CloseTabButton2.IsEnabled = false;
            }
            else
            {
                CloseTabButton.IsEnabled = true;
                CloseTabButton2.IsEnabled = true;
            }

            bool CanUndo = currentEditBox.Document.CanUndo();
            UndoButton.IsEnabled = CanUndo;

            bool CanRedo = currentEditBox.Document.CanRedo();
            RedoButton.IsEnabled = CanRedo;

            if (!ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                if (CanUndo)
                {
                    UndoTitleBarButton.Visibility = Visibility.Visible;
                }
                else
                {
                    UndoTitleBarButton.Visibility = Visibility.Collapsed;
                }

                if (CanRedo)
                {
                    RedoTitleBarButton.Visibility = Visibility.Visible;
                }
                else
                {
                    RedoTitleBarButton.Visibility = Visibility.Collapsed;
                }
            }

            if(currentEditBox.Tag == null)
            {
                FileNameItem.IsEnabled = false;
                FilePathItem.IsEnabled = false;
            }
            else
            {
                FileNameItem.IsEnabled = true;
                FilePathItem.IsEnabled = true;
            }

            LaunchButton.IsEnabled = false;

            if(currentEditBox.TextWrapping == TextWrapping.Wrap)
            {
                WordWrapItem.IsChecked = true;
            }
            else
            {
                WordWrapItem.IsChecked = false;
            }

            if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 3))
            {
                currentEditBox.ContextFlyout = EditMenu.ContextFlyout;
            }

            if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 7) && EditMenu.SelectionFlyout != null)
            {
                currentEditBox.SelectionFlyout = EditMenu.SelectionFlyout as RichEditBoxMenu.RichEditBoxSelectionFlyout;
            }

            ZoomOutButton.IsEnabled = true;
            ZoomInButton.IsEnabled = true;

            switch (currentEditBox.FontSize)
            {
                case 15:
                    ZoomPercentTextBlock.Text = "100%";
                    break;
                case 1.5:
                    ZoomPercentTextBlock.Text = "10%";
                    ZoomOutButton.IsEnabled = false;
                    break;
                case 3.75:
                    ZoomPercentTextBlock.Text = "25%";
                    break;
                case 7.5:
                    ZoomPercentTextBlock.Text = "50%";
                    break;
                case 30:
                    ZoomPercentTextBlock.Text = "200%";
                    break;
                case 45:
                    ZoomPercentTextBlock.Text = "300%";
                    break;
                case 60:
                    ZoomPercentTextBlock.Text = "400%";
                    break;
                case 75:
                    ZoomPercentTextBlock.Text = "500%";
                    ZoomInButton.IsEnabled = false;
                    break;
            }
        }

        #region "currentEditBox"
        private void currentEditBox_TextChanged(object sender, RoutedEventArgs e)
        {
            bool CanUndo = currentEditBox.Document.CanUndo();
            UndoButton.IsEnabled = CanUndo;

            bool CanRedo = currentEditBox.Document.CanRedo();
            RedoButton.IsEnabled = CanRedo;

            if (!ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                if (CanUndo)
                {
                    UndoTitleBarButton.Visibility = Visibility.Visible;
                }
                else
                {
                    UndoTitleBarButton.Visibility = Visibility.Collapsed;
                }

                if (CanRedo)
                {
                    RedoTitleBarButton.Visibility = Visibility.Visible;
                }
                else
                {
                    RedoTitleBarButton.Visibility = Visibility.Collapsed;
                }
            }

            if (currentEditBox.Tag == null)
            {
                SaveButton.IsEnabled = currentEditBox.TextSaveState == RichEditBoxCore.SaveState.TextIsChanged;
            }
            else
            {
                string autoSave = localSettings.Values["autoSave"].ToString();
                if(autoSave == "0")
                {
                    SaveButton.IsEnabled = currentEditBox.TextSaveState == RichEditBoxCore.SaveState.TextIsChanged;
                }
                else
                {
                    SaveButton.IsEnabled = false;
                }
            }

            if (currentEditBox.Tag != null &&
                (((StorageFile)currentEditBox.Tag).Path.EndsWith(".html") ||
                ((StorageFile)currentEditBox.Tag).Path.EndsWith(".htm")))
            {
                LaunchButton.IsEnabled = true;
            }
            else
            {
                LaunchButton.IsEnabled = false;
            }
        }

        private void currentEditBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            string SelectionText = currentEditBox.Document.Selection.Text;
            if (SelectionText != string.Empty)
            {
                FindAutoSuggestBox.Text = SelectionText;
                WhatAutoSuggestBox.Text = SelectionText;
            }
            else
            {
                if (currentEditBox.Document.Selection.CharacterFormat.BackgroundColor != Colors.White)
                {
                    currentEditBox.Document.Selection.CharacterFormat.BackgroundColor =
                        Colors.White;
                }
            }
        }

        private void currentEditBox_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            UIElement senderUI = sender as UIElement;
            e.Handled = true;
            EditMenu.ContextFlyout.ShowAt(senderUI, e.GetPosition(senderUI));
        }

        private void currentEditBox_Holding(object sender, HoldingRoutedEventArgs e)
        {
            UIElement senderUI = sender as UIElement;
            e.Handled = true;
            if(e.HoldingState == HoldingState.Started)
            {
                EditMenu.ContextFlyout.ShowAt(senderUI, e.GetPosition(senderUI));
            }
        }

        private void currentEditBox_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if(EditMenu.SelectionFlyout != null)
            {
                e.Handled = false;
            }
            else
            {
                currentEditBox.ContextFlyout = EditMenu.ContextFlyout;
                e.Handled = true;
            }
        }

        private void currentEditBox_ContextCanceled(UIElement sender, RoutedEventArgs args)
        {
            EditMenu.ContextFlyout.Hide();
        }

        private void currentEditBox_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            ZoomOutButton.IsEnabled = true;
            ZoomInButton.IsEnabled = true;

            switch (currentEditBox.FontSize)
            {
                case 15:
                    currentEditBox.FontSize = 30;
                    ZoomPercentTextBlock.Text = "200%";
                    break;
                case 1.5:
                    currentEditBox.FontSize = 3.75;
                    ZoomPercentTextBlock.Text = "25%";
                    break;
                case 3.75:
                    currentEditBox.FontSize = 7.5;
                    ZoomPercentTextBlock.Text = "50%";
                    break;
                case 7.5:
                    currentEditBox.FontSize = 15;
                    ZoomPercentTextBlock.Text = "100%";
                    break;
                case 30:
                    currentEditBox.FontSize = 15;
                    ZoomPercentTextBlock.Text = "100%";
                    break;
                case 45:
                    currentEditBox.FontSize = 30;
                    ZoomPercentTextBlock.Text = "200%";
                    break;
                case 60:
                    currentEditBox.FontSize = 45;
                    ZoomPercentTextBlock.Text = "300%";
                    break;
                case 75:
                    currentEditBox.FontSize = 60;
                    ZoomPercentTextBlock.Text = "400%";
                    break;
            }
        }
        #endregion

        #region "MessageBox Commands"
        private void yesAllTabs_CommandInvoked(IUICommand command)
        {
            ClearTabs();
        }
        #endregion

        #region "Find / Replace"
        private void FindAutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            int find;
            if(MatchCaseFindCheckBox.IsChecked == true)
            {
                find = currentEditBox.Find(FindAutoSuggestBox.Text, true);
            }
            else
            {
                find = currentEditBox.Find(FindAutoSuggestBox.Text, false);
            }

            if (currentEditBox.TextSelection != null)
            {
                currentEditBox.TextSelection.CharacterFormat.BackgroundColor = Colors.White;
            }

            if(find > 0)
            {
                FindResultsTextBlock.Text = "1/" + find;
            }
            else
            {
                FindResultsTextBlock.Text = "0/0";
            }

            MainGrid.UpdateLayout();
            currentEditBox.UpdateLayout();
        }

        private void WhatAutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            WithAutoSuggestBox.Focus(FocusState.Programmatic);
        }

        private void WithAutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            int replace;
            if (MatchCaseReplaceCheckBox.IsChecked == true)
            {
                replace = currentEditBox.Replace(WhatAutoSuggestBox.Text, WithAutoSuggestBox.Text, true);
            }
            else
            {
                replace = currentEditBox.Replace(WhatAutoSuggestBox.Text, WithAutoSuggestBox.Text, false);
            }

            if (currentEditBox.TextSelection != null)
            {
                currentEditBox.TextSelection.CharacterFormat.BackgroundColor = Colors.White;
            }

            if (replace > 0)
            {
                ReplaceResultsTextBlock.Text = "1/" + replace;
            }
            else
            {
                ReplaceResultsTextBlock.Text = "0/0";
            }
        }

        private void FindFlyout_Opened(object sender, object e)
        {
            FindAutoSuggestBox.Focus(FocusState.Pointer);
        }

        private void ReplaceFlyout_Opened(object sender, object e)
        {
            WhatAutoSuggestBox.Focus(FocusState.Pointer);
        }
        #endregion

        #endregion

        #region "Print events"
        private void PrintTaskRequested(PrintManager sender, PrintTaskRequestedEventArgs args)
        {
            // Create the PrintTask.
            // Defines the title and delegate for PrintTaskSourceRequested
            PrintTask printTask = args.Request.CreatePrintTask("Print", PrintTaskSourceRequrested);

            // Handle PrintTask.Completed to catch failed print jobs
            printTask.Completed += PrintTaskCompleted;
        }

        private void PrintTaskSourceRequrested(PrintTaskSourceRequestedArgs args)
        {
            // Set the document source.
            args.SetSource(printDocSource);
        }

        private void Paginate(object sender, PaginateEventArgs e)
        {
            // As I only want to print one Rectangle, so I set the count to 1
            printDoc.SetPreviewPageCount(1, PreviewPageCountType.Final);
        }

        private void GetPreviewPage(object sender, GetPreviewPageEventArgs e)
        {
            // Provide a UIElement as the print preview.
            printDoc.SetPreviewPage(e.PageNumber, currentEditBox);
        }

        private void AddPages(object sender, AddPagesEventArgs e)
        {
            printDoc.AddPage(currentEditBox);

            // Indicate that all of the print pages have been provided
            printDoc.AddPagesComplete();
        }

        private async void PrintTaskCompleted(PrintTask sender, PrintTaskCompletedEventArgs args)
        {
            // Notify the user when the print operation fails.
            if (args.Completion == PrintTaskCompletion.Failed)
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    ContentDialog noPrintingDialog = new ContentDialog()
                    {
                        Title = "Printing error",
                        Content = "\nSorry, failed to print.",
                        PrimaryButtonText = "OK"
                    };
                    await noPrintingDialog.ShowAsync();
                });
            }
        }
        #endregion

        #region "Methods"
        private void AddTab(string Header)
        {
            RichEditBoxPivotItem newTab = PivotMain.AddTab();
            newTab.Name = "Tab" + PivotMain.Items.Count;

            PivotMain.SelectedRichEditBoxItem.Header = Header;

            newTab.Tag = newTab;

            //Sets titlebar text
            appView.Title = Header;

            TitleTextBlock.Text = string.Format("{0} – {1}", appView.Title,
                Package.Current.DisplayName);

            //Adds items in all tabs list
            newTab.ListViewItem.Title = newTab.Header.ToString();
            dataList.Add(newTab.ListViewItem);

            //Events
            currentEditBox.TextChanged += new RoutedEventHandler(currentEditBox_TextChanged);
            currentEditBox.SelectionChanged += new RoutedEventHandler(currentEditBox_SelectionChanged);
            currentEditBox.DoubleTapped += new DoubleTappedEventHandler(currentEditBox_DoubleTapped);

            //Until Windows 10 version 1607 for context flyout works only Holding and RightTapped events 
            if (!ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 3))
            {
                currentEditBox.RightTapped += currentEditBox_RightTapped;
                currentEditBox.Holding += currentEditBox_Holding;
            }
            else
            {
                currentEditBox.ContextMenuOpening += currentEditBox_ContextMenuOpening;
            }
        }

        private async void AddRecentFiles()
        {
            //It is for testing, if delete some file from Recent Files list, this will be throw FileNotFoundException
            RecentFilesFlyout.Items.Clear();
            StorageItemMostRecentlyUsedList mru = StorageApplicationPermissions.MostRecentlyUsedList;
            foreach (AccessListEntry entry in mru.Entries)
            {
                string mruToken = entry.Token;
                string mruMetadata = entry.Metadata;
                IAsyncOperation<StorageFile> asyncOperation = mru.GetFileAsync(mruToken);
                try
                {
                    if (RecentFilesFlyout.Items.Count <= 9)
                    {
                        if (mru.CheckAccess(await asyncOperation))
                        {
                            MenuFlyoutItem mruMenuFlyout = new MenuFlyoutItem();
                            mruMenuFlyout.Click += mruMenuFlyout_Click;
                            mruMenuFlyout.Tag = mruToken;
                            StorageFile file = await asyncOperation;
                            mruMenuFlyout.Text = file.Path;
                            RecentFilesFlyout.Items.Add(mruMenuFlyout);
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                catch (FileNotFoundException)
                {
                    StorageApplicationPermissions.MostRecentlyUsedList.Remove(mruToken);
                }
            }
        }

        private async void NewWindow() 
        {
            CoreApplicationView newAV = CoreApplication.CreateNewView();
            await newAV.Dispatcher.RunAsync(
                CoreDispatcherPriority.Normal,
                async () =>
                {
                    Window newWindow = Window.Current;
                    ApplicationView newAppView = ApplicationView.GetForCurrentView();

                    Frame frame = new Frame();
                    frame.Navigate(typeof(MainPage), "NewWindow");
                    newWindow.Content = frame;
                    newWindow.Activate();

                    //Shows new window of same app
                    await ApplicationViewSwitcher.TryShowAsStandaloneAsync(
                        newAppView.Id,
                        ViewSizePreference.UseMinimum,
                        appView.Id,
                        ViewSizePreference.UseMinimum);
                });
        }

        private async void CloseOneTab(RichEditBoxPivotItem item)
        {
            await Task.Delay(200);

            string autoSave = localSettings.Values["autoSave"].ToString();
            if (PivotMain.Items.Count > 1)
            {
                if (item.EditBox.TextSaveState == RichEditBoxCore.SaveState.TextIsChanged && item.EditBox.Tag != null && autoSave == "0")
                {
                    //Shows message box when changes of file is not saved
                    ApiResources.ShowMessageBox("Save changes", "Do you want to save changes?",
                       async (command) =>
                        {
                            await SaveFile(item, (StorageFile)item.EditBox.Tag);
                            CloseTab(item);
                        },

                        (command) =>
                        {
                            CloseTab(item);
                        });
                }
                else if (item.EditBox.TextSaveState == RichEditBoxCore.SaveState.TextIsChanged && item.EditBox.Tag == null)
                {
                    //Shows message box when file is not saved
                    ApiResources.ShowMessageBox("Save file", "Do you want to save \"" +
                        item.Header +
                        "\"?",
                        (command) =>
                        {
                            SaveFileAs(item, true);
                        },

                        (command) =>
                        {
                            CloseTab(item);
                        });
                }
                else if (item.EditBox.TextSaveState != RichEditBoxCore.SaveState.TextIsChanged || autoSave == "1" && item.EditBox.Tag != null)
                {
                    CloseTab(item);
                }
            }
        }

        private async void CloseTab(RichEditBoxPivotItem item)
        {
            item.Tag = null;
            PivotMain.CloseTab(item);
            dataList.Remove(item.ListViewItem);

            if (AllTabsList != null)
            {
                AllTabsList.SelectedIndex = PivotMain.SelectedIndex;
            }

            if(item.ListViewItem.File == null && item.ListViewItem.CopiedFile != null)
            {
                await item.ListViewItem.CopiedFile.DeleteAsync(StorageDeleteOption.PermanentDelete);
            }
        }

        private void CloseAllTabs()
        {
            if (PivotMain.Items.Count == 1 && currentEditBox.TextSaveState == RichEditBoxCore.SaveState.TextIsChanged)
            {
                ClearTabs();
            }
            else
            {
                ApiResources.ShowMessageBoxTwoButton("Warning",
                    "Datas will lose if unsaved.\r\nDo you want clear tabpages?",
                    yesAllTabs_CommandInvoked);
            }
        }

        private async void ClearTabs()
        {
            //Code will not working normally when add only one tab
            unSavedTimer.Stop();
            await Task.Delay(100);
            AddTab("");
            foreach(RichEditBoxPivotItem item in PivotMain.Items)
            {
                if (item.ListViewItem.File == null && item.ListViewItem.CopiedFile != null)
                {
                    if (await unsavedFolder.TryGetItemAsync(item.ListViewItem.CopiedFile.Name) != null)
                        await item.ListViewItem.CopiedFile.DeleteAsync(StorageDeleteOption.PermanentDelete);
                }
            }
            PivotMain.Items.Clear();
            dataList.Clear();
            AddTab("New Tab " + PivotMain.Items.Count);
            await Task.Delay(100);
            unSavedTimer.Start();
        }

        private async void CacheUnsavedFiles(object sender, object e)
        {
            string hotExit = localSettings.Values["hotExit"].ToString();
            if(hotExit == "1")
            {
                foreach (RichEditBoxPivotItem item in PivotMain.Items)
                {
                    if (item.ListViewItem.File == null)
                    {
                        if (item.ListViewItem.CopiedFile == null)
                        {
                            if (await unsavedFolder.TryGetItemAsync(item.Header.ToString()) == null)
                                await unsavedFolder.CreateFileAsync(item.Header.ToString(), CreationCollisionOption.OpenIfExists);

                            item.ListViewItem.CopiedFile = await unsavedFolder.GetFileAsync(item.Header.ToString());

                            item.Header = item.ListViewItem.CopiedFile.Name;

                            PivotMain.SelectedRichEditBoxItem.ListViewItem.Title = item.Header.ToString();
                        }

                        if (await unsavedFolder.TryGetItemAsync(item.Header.ToString()) != null)
                            await FileIO.WriteTextAsync(item.ListViewItem.CopiedFile, item.EditBox.CoreText.Text);
                    }
                }
            }
        }

        private async void OpenFile()
        {
            /*
             * On mobile when double click on open button or click on save button
             * Will be throw exception, because of two async show UI operations in same time
            */
            OpenButton.IsEnabled = false;
            SaveButton.IsEnabled = false;
            SaveAsButton.IsEnabled = false;

            FileOpenPicker picker = new FileOpenPicker();
            picker.ViewMode = PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            picker.FileTypeFilter.Add(".txt");
            picker.FileTypeFilter.Add(".text");
            picker.FileTypeFilter.Add(".rtf");
            picker.FileTypeFilter.Add(".log");
            picker.FileTypeFilter.Add(".ini");
            picker.FileTypeFilter.Add(".reg");
            picker.FileTypeFilter.Add(".htm");
            picker.FileTypeFilter.Add(".html");
            picker.FileTypeFilter.Add(".js");
            picker.FileTypeFilter.Add(".css");
            picker.FileTypeFilter.Add(".xml");
            picker.FileTypeFilter.Add(".xaml");
            picker.FileTypeFilter.Add(".php");
            picker.FileTypeFilter.Add(".c");
            picker.FileTypeFilter.Add(".cs");
            picker.FileTypeFilter.Add(".cpp");
            picker.FileTypeFilter.Add(".h");
            picker.FileTypeFilter.Add(".d");
            picker.FileTypeFilter.Add(".vb");
            picker.FileTypeFilter.Add(".vbs");

            IAsyncAction asyncAction = MainPagePG.Dispatcher.RunAsync(
                CoreDispatcherPriority.Low,
                async () =>
                {
                    unSavedTimer.Stop();
                    timer.Stop();

                    //Shows picker
                    IAsyncOperation<StorageFile> pickSingleFile = picker.PickSingleFileAsync();
                    StorageFile file = await pickSingleFile;

                    StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                    if (file != null)
                    {
                        await OpenFile(file, false);
                    }

                    OpenButton.IsEnabled = true;
                    SaveAsButton.IsEnabled = true;

                    pickSingleFile.Close();

                    unSavedTimer.Start();
                    timer.Start();
                });

            await asyncAction;
            asyncAction.Close();
        }

        private async Task OpenFile(StorageFile file, bool IsCachedFile)
        {
            unSavedTimer.Stop();
            timer.Stop();

            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            if (file != null)
            {
                if (GetFileInTabs(file) == false)
                {
                    string FileName = Path.GetFileName(file.Path); 
                    if (PivotMain.Items.Count == 0 || currentEditBox.Document.CanUndo() || currentEditBox.CoreText.Text != "" && currentEditBox.Tag != null || IsCachedFile)
                    {
                        AddTab(FileName);
                    }
                    else
                    {
                        PivotMain.SelectedRichEditBoxItem.Header = FileName;

                        PivotMain.SelectedRichEditBoxItem.ListViewItem.Title = PivotMain.SelectedRichEditBoxItem.Header.ToString();
                    }

                    DisplayInformation displayInformation = DisplayInformation.GetForCurrentView();
                    if (displayInformation.DiagonalSizeInInches < 5.0 || displayInformation.ResolutionScale > ResolutionScale.Scale400Percent)
                    {
                        if (FileName.Length < 11)
                        {
                            appView.Title = FileName;
                        }
                        else
                        {
                            appView.Title = string.Format("{0} ... {1}", FileName.Substring(0, 6), FileName.Substring(FileName.Length - 3));
                        }
                    }
                    else
                    {
                        if (FileName.Length < 18)
                        {
                            appView.Title = FileName;
                        }
                        else
                        {
                            appView.Title = string.Format("{0} ... {1}", FileName.Substring(0, 13), FileName.Substring(FileName.Length - 3));
                        }
                    }

                    TitleTextBlock.Text = string.Format("{0} – {1}", appView.Title,
                        Package.Current.DisplayName);

                    IAsyncAction asyncAction = MainPagePG.Dispatcher.RunAsync(
                        CoreDispatcherPriority.Low,
                        () =>
                        {
                            string text = "";

                            Task.Run(
                                async () =>
                                {
                                    try
                                    {
                                        Stream stream = await file.OpenStreamForReadAsync();
                                        using (IRandomAccessStream inputStream = stream.AsRandomAccessStream())
                                        {
                                            using (DataReader dataReader = new DataReader(inputStream))
                                            {
                                                ulong size = inputStream.Size;
                                                uint numBytesLoaded = await dataReader.LoadAsync((uint)size);
                                                byte[] fileContent = new byte[dataReader.UnconsumedBufferLength];
                                                dataReader.ReadBytes(fileContent);
                                                text = Encoding.UTF8.GetString(fileContent, 0, fileContent.Length);
                                            }
                                        }
                                        stream.Dispose();
                                    }
                                    catch (Exception) { }
                                }).Wait();

                            if (file.Path.EndsWith(".rtf"))
                            {
                                currentEditBox.CoreText.TextRtf = text;
                            }
                            else
                            {
                                currentEditBox.CoreText.Text = text;
                            }
                        });

                    await asyncAction;
                    asyncAction.Close();

                    LaunchButton.IsEnabled = false;

                    if (file.Path.EndsWith(".html") ||
                        file.Path.EndsWith(".htm"))
                    {
                        LaunchButton.IsEnabled = true;
                    }

                    if (IsCachedFile == false)
                    {
                        StorageItemMostRecentlyUsedList mru = StorageApplicationPermissions.MostRecentlyUsedList;
                        string mruToken = mru.Add(file, file.Path, RecentStorageItemVisibility.AppAndSystem);

                        if (PivotMain.SelectedRichEditBoxItem.ListViewItem.CopiedFile != null)
                        {
                            await PivotMain.SelectedRichEditBoxItem.ListViewItem.CopiedFile.DeleteAsync(StorageDeleteOption.PermanentDelete);
                        }

                        if (file.Path.EndsWith(".txt") || file.Path.EndsWith(".text") || file.Path.EndsWith(".rtf"))
                        {
                            currentEditBox.IsSpellCheckEnabled = true;
                        }
                        else
                        {
                            currentEditBox.IsSpellCheckEnabled = false;
                        }

                        currentEditBox.TextSaveState = RichEditBoxCore.SaveState.OpenedFile;

                        currentEditBox.Tag = file;

                        PivotMain.SelectedRichEditBoxItem.ListViewItem.File = file;
                        PivotMain.SelectedRichEditBoxItem.ListViewItem.CopiedFile = null;

                        FileNameItem.IsEnabled = true;
                        FilePathItem.IsEnabled = true;

                    }
                    else
                    {
                        currentEditBox.TextSaveState = RichEditBoxCore.SaveState.TextIsChanged;

                        currentEditBox.Tag = null;

                        PivotMain.SelectedRichEditBoxItem.ListViewItem.CopiedFile = file;
                        PivotMain.SelectedRichEditBoxItem.ListViewItem.File = null;
                    }
                }
                else
                {
                    foreach (RichEditBoxPivotItem item in PivotMain.Items)
                    {
                        if (item.ListViewItem.CopiedFile != null && item.ListViewItem.CopiedFile.Path == file.Path
                            || item.ListViewItem.File != null && item.ListViewItem.File.Path == file.Path)
                        {
                            PivotMain.SelectedItem = item;
                        }
                    }
                }

                unSavedTimer.Start();
                timer.Start();

                MainGrid.UpdateLayout();
                currentEditBox.UpdateLayout();
            }
        }

        private async Task SaveFile(RichEditBoxPivotItem item, StorageFile file)
        {
            unSavedTimer.Stop();
            timer.Stop();

            SaveButton.IsEnabled = false;

            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            string FileName = Path.GetFileName(file.Path);

            item.Header = FileName;

            item.ListViewItem.Title = item.Header.ToString();

            DisplayInformation displayInformation = DisplayInformation.GetForCurrentView();
            if (displayInformation.DiagonalSizeInInches < 5.0 || displayInformation.ResolutionScale > ResolutionScale.Scale400Percent)
            {
                if (FileName.Length < 11)
                {
                    appView.Title = FileName;
                }
                else
                {
                    appView.Title = string.Format("{0} ... {1}", FileName.Substring(0, 6), FileName.Substring(FileName.Length - 3));
                }
            }
            else
            {
                if (FileName.Length < 18)
                {
                    appView.Title = FileName;
                }
                else
                {
                    appView.Title = string.Format("{0} ... {1}", FileName.Substring(0, 13), FileName.Substring(FileName.Length - 3));
                }
            }

            TitleTextBlock.Text = string.Format("{0} – {1}", appView.Title,
                Package.Current.DisplayName);

            IAsyncAction asyncAction = MainPagePG.Dispatcher.RunAsync(
                CoreDispatcherPriority.Low,
                () =>
                {
                    string text = "";

                    if (file.Path.EndsWith(".rtf"))
                    {
                        text = item.EditBox.CoreText.TextRtf;
                    }
                    else
                    {
                        text = item.EditBox.CoreText.Text;
                    }

                    Task.Run(
                        async () =>
                        {
                            try
                            {
                                using (StorageStreamTransaction transaction = await file.OpenTransactedWriteAsync())
                                {
                                    using (DataWriter dataWriter = new DataWriter(transaction.Stream))
                                    {
                                        dataWriter.WriteString(text);
                                        transaction.Stream.Size = await dataWriter.StoreAsync(); // reset stream size to override the file
                                        await transaction.CommitAsync();
                                    }
                                }
                            }
                            catch (Exception) { }
                        }).Wait();
                });

            await asyncAction;
            asyncAction.Close();

            StorageItemMostRecentlyUsedList mru = StorageApplicationPermissions.MostRecentlyUsedList;
            string mruToken = mru.Add(file, file.Path, RecentStorageItemVisibility.AppAndSystem);

            item.EditBox.TextSaveState = RichEditBoxCore.SaveState.TextIsUnchanged;

            LaunchButton.IsEnabled = false;

            if (file.Path.EndsWith(".html") ||
                file.Path.EndsWith(".htm"))
            {
                LaunchButton.IsEnabled = true;
            }

            if (file.Path.EndsWith(".txt") || file.Path.EndsWith(".text") || file.Path.EndsWith(".rtf"))
            {
                item.EditBox.IsSpellCheckEnabled = true;
            }
            else
            {
                item.EditBox.IsSpellCheckEnabled = false;
            }

            item.EditBox.Tag = file;
            item.ListViewItem.File = file;
            item.ListViewItem.CopiedFile = null;

            FileNameItem.IsEnabled = true;
            FilePathItem.IsEnabled = true;

            unSavedTimer.Start();
            timer.Start();
        }

        private async void PeriodicSave(object sender, object e)
        {
            string autoSave = localSettings.Values["autoSave"].ToString();
            if (autoSave == "1")
            {
                foreach(RichEditBoxPivotItem item in PivotMain.Items)
                {
                    if (item.ListViewItem.CopiedFile == null && item.ListViewItem.File != null)
                    {
                        if (PivotMain.SelectedRichEditBoxItem == item)
                        {
                            SaveButton.IsEnabled = false;
                        }

                        item.EditBox.TextSaveState = RichEditBoxCore.SaveState.TextIsUnchanged;

                        await FileIO.WriteTextAsync(item.ListViewItem.File, item.EditBox.CoreText.Text);
                    }
                }
            }
        }

        private async void SaveFileAs(RichEditBoxPivotItem item, bool SaveCloseTab)
        {
            OpenButton.IsEnabled = false;
            SaveButton.IsEnabled = false;
            SaveAsButton.IsEnabled = false;

            FileSavePicker picker = new FileSavePicker();
            picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            picker.FileTypeChoices.Add("Text Files", new string[] { ".txt", ".text" });
            picker.FileTypeChoices.Add("RTF File", new string[] { ".rtf" });
            picker.FileTypeChoices.Add("Log File", new string[] { ".log" });
            picker.FileTypeChoices.Add("INI file", new string[] { ".ini" });
            picker.FileTypeChoices.Add("REG file", new string[] { ".reg" });
            picker.FileTypeChoices.Add("HTML Files", new string[] { ".htm", ".html" });
            picker.FileTypeChoices.Add("CSS File", new string[] { ".css" });
            picker.FileTypeChoices.Add("JavaScript File", new string[] { ".js" });
            picker.FileTypeChoices.Add("XML Files", new string[] { ".xml", ".xaml" });
            picker.FileTypeChoices.Add("PHP File", new string[] { ".php" });
            picker.FileTypeChoices.Add("C source file", new string[] { ".c" });
            picker.FileTypeChoices.Add("C# source file", new string[] { ".cs" });
            picker.FileTypeChoices.Add("C++ source files", new string[] { ".cpp", ".h" });
            picker.FileTypeChoices.Add("D source file", new string[] { ".d" });
            picker.FileTypeChoices.Add("VB source files", new string[] { ".vb", ".vbs" });
            picker.FileTypeChoices.Add("All files", new string[] { "." });
            picker.SuggestedFileName = "New File " + PivotMain.Items.Count;

            IAsyncAction asyncAction = MainPagePG.Dispatcher.RunAsync(
                CoreDispatcherPriority.Low,
                async () =>
                {
                    unSavedTimer.Stop();
                    timer.Stop();

                    IAsyncOperation<StorageFile> pickSaveFile = picker.PickSaveFileAsync();
                    StorageFile file = await pickSaveFile;

                    StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                    if (file != null)
                    {
                        string hotExit = localSettings.Values["hotExit"].ToString();
                        if (item.ListViewItem.File == null && hotExit == "1")
                        {
                            await item.ListViewItem.CopiedFile.DeleteAsync(StorageDeleteOption.PermanentDelete);
                        }

                        await SaveFile(item, file);

                        StorageItemMostRecentlyUsedList mru = StorageApplicationPermissions.MostRecentlyUsedList;
                        string mruToken = mru.Add(file, file.Path, RecentStorageItemVisibility.AppAndSystem);

                        if (SaveCloseTab == true)
                        {
                            CloseTab(item);
                        }
                    }

                    OpenButton.IsEnabled = true;
                    SaveAsButton.IsEnabled = true;

                    pickSaveFile.Close();

                    unSavedTimer.Start();
                    timer.Start();
                });

            await asyncAction;
            asyncAction.Close();
        }
         
        private bool GetFileInTabs(StorageFile file)
        {
            foreach(RichEditBoxPivotItem item in PivotMain.Items)
            {
                if(item.ListViewItem.CopiedFile != null && item.ListViewItem.CopiedFile.Path == file.Path || item.ListViewItem.File != null && item.ListViewItem.File.Path == file.Path)
                {
                    return true;
                }
            }
            return false;
        }

        private async void Print()
        {
            if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 3)
                && PrintManager.IsSupported())
            {
                try
                {
                    // Show print UI
                    await PrintManager.ShowPrintUIAsync();
                }
                catch
                {
                    // Printing cannot proceed at this time
                    ContentDialog noPrintingDialog = new ContentDialog()
                    {
                        Title = "Printing error",
                        Content = "\r\nSorry, printing can't proceed at this time.",
                        PrimaryButtonText = "OK"
                    };

                    await noPrintingDialog.ShowAsync();
                }
            }
        }

        private async void PinFileToStartMenu()
        {
            StorageFile file = ((StorageFile)currentEditBox.Tag);

            if (file != null)
            {
                StorageLibrary documentsLibrary = await ApiResources.TryAccessLibraryAsync(KnownLibraryId.Documents);
                StorageLibrary musicLibrary = await ApiResources.TryAccessLibraryAsync(KnownLibraryId.Music);
                StorageLibrary videoLibrary = await ApiResources.TryAccessLibraryAsync(KnownLibraryId.Videos);
                StorageLibrary pictureLibrary = await ApiResources.TryAccessLibraryAsync(KnownLibraryId.Pictures);

                if (documentsLibrary != null && musicLibrary != null && videoLibrary != null && pictureLibrary != null)
                {
                    string path = ((StorageFile)currentEditBox.Tag).Path;

                    // Test these endings.
                    string[] libraries = new string[]
                    {
                        documentsLibrary.SaveFolder.Path,
                        musicLibrary.SaveFolder.Path,
                        videoLibrary.SaveFolder.Path,
                        pictureLibrary.SaveFolder.Path
                    };

                    bool IsStoredRightFolder = false;

                    foreach (string libraryPath in libraries)
                    {
                        if (file.Path.StartsWith(libraryPath, StringComparison.CurrentCultureIgnoreCase))
                        {
                            IsStoredRightFolder = true;

                            // Use a display name you like
                            string displayName = ((StorageFile)currentEditBox.Tag).Name;

                            await AppTile.RequestPinSecondaryTile(path, displayName);
                            break;
                        }
                    }

                    if (IsStoredRightFolder == false)
                    {
                        new UCNotification("Error", "You can pin a file, stored in Documents, Music, Videos, Pictures folders only.").ShowNotification();
                    }
                }
            }
            else if (currentEditBox.Tag == null)
            {
                UCNotification notification = new UCNotification("Save file", string.Format("\"{0}\" is can't pin because is not have a saved file.", PivotMain.SelectedRichEditBoxItem.Header));
                notification.ToastLaunchArguments = "action=selecttab&tab=" + PivotMain.SelectedRichEditBoxItem.ListViewItem.CopiedFileName.Replace(" ", "_");
                notification.ToastButtonContent = "Save file";
                notification.ToastButtonArguments = "action=savefile&file=" + PivotMain.SelectedRichEditBoxItem.ListViewItem.CopiedFileName.Replace(" ", "_");
                notification.ShowNotification();
            }
        }

        private void ShareText()
        {
            DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += DataTransferManager_DataRequested;
            DataTransferManager.ShowShareUI();
        }

        private async void SendEmail(StorageFile file)
        {
            EmailMessage emailMessage = new EmailMessage();
            emailMessage.Subject = "Attachment: " + file.Name;
            emailMessage.Body = "(sent by Textie Editor)";
            StorageFile attachmentFile = file;
            if (attachmentFile != null)
            {
                RandomAccessStreamReference stream = RandomAccessStreamReference.CreateFromFile(attachmentFile);
                EmailAttachment attachment = new EmailAttachment(
                         attachmentFile.Name,
                         stream);
                emailMessage.Attachments.Add(attachment);
            }
            await EmailManager.ShowComposeNewEmailAsync(emailMessage);
        }

        private void ZoomOut()
        {
            switch (currentEditBox.FontSize)
            {
                case 15:
                    currentEditBox.FontSize = 7.5;
                    ZoomPercentTextBlock.Text = "50%";
                    break;
                case 3.75:
                    currentEditBox.FontSize = 1.5;
                    ZoomPercentTextBlock.Text = "10%";
                    ZoomOutButton.IsEnabled = false;
                    break;
                case 7.5:
                    currentEditBox.FontSize = 3.75;
                    ZoomPercentTextBlock.Text = "25%";
                    break;
                case 30:
                    currentEditBox.FontSize = 15;
                    ZoomPercentTextBlock.Text = "100%";
                    break;
                case 45:
                    currentEditBox.FontSize = 30;
                    ZoomPercentTextBlock.Text = "200%";
                    break;
                case 60:
                    currentEditBox.FontSize = 45;
                    ZoomPercentTextBlock.Text = "300%";
                    break;
                case 75:
                    currentEditBox.FontSize = 60;
                    ZoomPercentTextBlock.Text = "400%";
                    ZoomInButton.IsEnabled = true;
                    break;
            }
        }

        private void ZoomIn()
        {
            switch (currentEditBox.FontSize)
            {
                case 15:
                    currentEditBox.FontSize = 30;
                    ZoomPercentTextBlock.Text = "200%";
                    break;
                case 1.5:
                    currentEditBox.FontSize = 3.75;
                    ZoomPercentTextBlock.Text = "25%";
                    ZoomOutButton.IsEnabled = true;
                    break;
                case 3.75:
                    currentEditBox.FontSize = 7.5;
                    ZoomPercentTextBlock.Text = "50%";
                    break;
                case 7.5:
                    currentEditBox.FontSize = 15;
                    ZoomPercentTextBlock.Text = "100%";
                    break;
                case 30:
                    currentEditBox.FontSize = 45;
                    ZoomPercentTextBlock.Text = "300%";
                    break;
                case 45:
                    currentEditBox.FontSize = 60;
                    ZoomPercentTextBlock.Text = "400%";
                    break;
                case 60:
                    currentEditBox.FontSize = 75;
                    ZoomPercentTextBlock.Text = "500%";
                    ZoomInButton.IsEnabled = false;
                    break;
            }
        }

        private void ShowFullScreen()
        {
            ApplicationView applicationView = ApplicationView.GetForCurrentView();
            applicationView.TryEnterFullScreenMode();

            SecondaryCommands.Hide();
        }

        #endregion

        #region "Key Accelerators"
        private void MakeKeyAccelerators()
        {
            //Windows.UI.Xaml.Input.KeyboardAccelerator is introduced in Windows 10 version 1709
            if (ApiInformation.IsTypePresent("Windows.UI.Xaml.Input.KeyboardAccelerator"))
            {
                KeyboardAccelerator CtrlT = new KeyboardAccelerator();
                CtrlT.Key = VirtualKey.T;
                CtrlT.Modifiers = VirtualKeyModifiers.Control;
                AddTabButton.AccessKey = "T";
                AddTabButton.KeyboardAccelerators.Add(CtrlT);

                KeyboardAccelerator CtrlW = new KeyboardAccelerator();
                CtrlW.Key = VirtualKey.W;
                CtrlW.Modifiers = VirtualKeyModifiers.Control;
                CloseTabButton.AccessKey = "W";
                CloseTabButton.KeyboardAccelerators.Add(CtrlW);

                KeyboardAccelerator CtrlO = new KeyboardAccelerator();
                CtrlO.Key = VirtualKey.O;
                CtrlO.Modifiers = VirtualKeyModifiers.Control;
                OpenButton.AccessKey = "O";
                OpenButton.KeyboardAccelerators.Add(CtrlO);

                KeyboardAccelerator CtrlS = new KeyboardAccelerator();
                CtrlS.Key = VirtualKey.S;
                CtrlS.Modifiers = VirtualKeyModifiers.Control;
                SaveButton.AccessKey = "S";
                SaveButton.KeyboardAccelerators.Add(CtrlS);

                KeyboardAccelerator CtrlF = new KeyboardAccelerator();
                CtrlF.Key = VirtualKey.F;
                CtrlF.Modifiers = VirtualKeyModifiers.Control;
                FindButton.AccessKey = "F";
                FindButton.KeyboardAccelerators.Add(CtrlF);

                KeyboardAccelerator CtrlH = new KeyboardAccelerator();
                CtrlH.Key = VirtualKey.H;
                CtrlH.Modifiers = VirtualKeyModifiers.Control;
                ReplaceButton.AccessKey = "H";
                ReplaceButton.KeyboardAccelerators.Add(CtrlH);

                KeyboardAccelerator CtrlMinus = new KeyboardAccelerator();
                CtrlMinus.Key = VirtualKey.Subtract;
                CtrlMinus.Modifiers = VirtualKeyModifiers.Control;
                ZoomOutButton.AccessKey = "-";
                ZoomOutButton.KeyboardAccelerators.Add(CtrlMinus);

                KeyboardAccelerator CtrlPlus = new KeyboardAccelerator();
                CtrlPlus.Key = VirtualKey.Add;
                CtrlPlus.Modifiers = VirtualKeyModifiers.Control;
                ZoomInButton.AccessKey = "+";
                ZoomInButton.KeyboardAccelerators.Add(CtrlPlus);

                KeyboardAccelerator F11 = new KeyboardAccelerator();
                F11.Key = VirtualKey.F11;
                FullScreenButton.KeyboardAccelerators.Add(F11);

                KeyboardAccelerator Alt = new KeyboardAccelerator();
                Alt.Key = VirtualKey.Menu;
                Alt.Invoked += Alt_Invoked;
                MainCommandBar.KeyboardAccelerators.Add(Alt);

            }
        }

        private void Mid_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            ApplicationDataContainer localSettings =
                ApplicationData.Current.LocalSettings;
            string SelectionText = currentEditBox.Document.Selection.Text;
            string SearchEngine = localSettings.Values["SearchEngine"].ToString();
            if ((SelectionText.StartsWith("http://") || SelectionText.StartsWith("https://")) && SelectionText.Contains("."))
            {
                WebResources.Navigate(SelectionText);
            }
            else
            {
                if (SearchEngine == "Bing")
                {
                    WebResources.Navigate("https://www.bing.com/search?q=" + SelectionText);
                }
                else if (SearchEngine == "Google")
                {
                    WebResources.Navigate("https://www.google.com/search?q=" + SelectionText);
                }
                else if (SearchEngine == "Yahoo")
                {
                    WebResources.Navigate("https://search.yahoo.com/search?p=" + SelectionText);
                }
            }
        }

        private void Alt_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            SecondaryCommands.ShowAt(EditButton);
        }
        #endregion

        #region Design Methods
        private void MakeDesign()
        {
            string theme = localSettings.Values["theme"].ToString();
            string titleBarColor = localSettings.Values["titleBarColor"].ToString();

            Brush BasicBackBrush =
                Resources["ApplicationPageBackgroundThemeBrush"] as Brush;

            if (titleBarColor == "0")
            {
                LeftAppTitleBar.Background = BasicBackBrush;
                MiddleAppTitleBar.Background = BasicBackBrush;
            }
            else
            {
                BasicAccentBrush();
            }

            Style FlyoutStyle = new Style { TargetType = typeof(FlyoutPresenter) };

            FlyoutStyle.Setters.Add(new Setter(PaddingProperty,
                0));
            FlyoutStyle.Setters.Add(new Setter(RequestedThemeProperty,
                MainGrid.RequestedTheme));

            if (theme == "WD")
            {
                //Adds reveal highlight
                if (ApiInformation.IsTypePresent("Windows.UI.Xaml.Media.RevealBrush"))
                {
                    Style AppBarButtonReveal =
                        Resources["AppBarButtonRevealStyle"] as Style;
                    Style AppBarButtonOverflowReveal =
                        Application.Current.Resources["RightAlignRevealAppBarButton"] as Style;
                    Style AppBarToggleOverflowReveal =
                        Application.Current.Resources["RightAlignRevealAppBarToggle"] as Style;

                    Style ButtonReveal = Resources["ButtonRevealStyle"] as Style;
                    UndoTitleBarButton.Style = ButtonReveal;
                    RedoTitleBarButton.Style = ButtonReveal;

                    NewWindowButton.Style = AppBarButtonReveal;
                    AllTabsButton.Style = AppBarButtonReveal;
                    AddTabButton2.Style = AppBarButtonReveal;
                    AllTabsButton2.Style = AppBarButtonReveal;
                    CloseTabButton2.Style = AppBarButtonReveal;
                    EditButton2.Style = AppBarButtonOverflowReveal;
                    UndoButton.Style = AppBarButtonOverflowReveal;
                    RedoButton.Style = AppBarButtonOverflowReveal;
                    InsertButton.Style = AppBarButtonOverflowReveal;
                    FindButton.Style = AppBarButtonOverflowReveal;
                    ReplaceButton.Style = AppBarButtonOverflowReveal;
                    FormatButton.Style = AppBarButtonOverflowReveal;
                    RecentFilesButton.Style = AppBarButtonOverflowReveal;
                    SaveAsButton.Style = AppBarButtonOverflowReveal;
                    SaveAllButton.Style = AppBarButtonOverflowReveal;
                    AutoSaveToggle.Style = AppBarToggleOverflowReveal;
                    PrintButton.Style = AppBarButtonOverflowReveal;
                    ShareButton.Style = AppBarButtonOverflowReveal;
                    PinButton.Style = AppBarButtonOverflowReveal;
                    LaunchButton.Style = AppBarButtonOverflowReveal;
                    ZoomOutButton.Style = AppBarButtonOverflowReveal;
                    ZoomInButton.Style = AppBarButtonOverflowReveal;
                    FullScreenButton.Style = AppBarButtonOverflowReveal;
                    SettingsButton.Style = AppBarButtonOverflowReveal;

                    NewWindowButtonStyle.Value = AppBarButtonOverflowReveal;
                    NewWindowButtonStyleX.Value = AppBarButtonOverflowReveal;
                }

                //Adds transparency on flyouts
                if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 4))
                {
                    UCAcrylicBrush AcrylicSystemBrush = new UCAcrylicBrush()
                    {
                        BackgroundSource = CustomAcrylicBackgroundSource.Hostbackdrop,
                        TintColor = Color.FromArgb(255, 31, 31, 31),
                        FallbackColor = Color.FromArgb(255, 50, 50, 50)
                    };

                    UCAcrylicBrush AcrylicElementBrush = new UCAcrylicBrush()
                    {
                        BackgroundSource = CustomAcrylicBackgroundSource.Backdrop
                    };

                    object SystemAccentColor = Resources["SystemAccentColor"];

                    byte[] AccentRGB = { ((Color)SystemAccentColor).R, ((Color)SystemAccentColor).G, ((Color)SystemAccentColor).B };

                    UCAcrylicBrush AcrylicAccentBrush = new UCAcrylicBrush()
                    {
                        BackgroundSource = CustomAcrylicBackgroundSource.Hostbackdrop,
                        TintColor = Color.FromArgb(255, AccentRGB[0], AccentRGB[1], AccentRGB[2]),
                        FallbackColor = Color.FromArgb(255, AccentRGB[0], AccentRGB[1], AccentRGB[2])
                    };

                    UISettings DefaultTheme = new UISettings();
                    string uiTheme = DefaultTheme.GetColorValue(UIColorType.Background).ToString();
                    if (uiTheme == "#FFFFFFFF")
                    {
                        AcrylicSystemBrush.TintColor = Color.FromArgb(255, 255, 255, 255);
                        AcrylicSystemBrush.FallbackColor = Color.FromArgb(255, 230, 230, 230);
                    }

                    AcrylicElementBrush.TintColor = AcrylicSystemBrush.TintColor;
                    AcrylicElementBrush.FallbackColor = AcrylicSystemBrush.FallbackColor;

                    Style AcrylicCommandBarStyle = new Style { TargetType = typeof(CommandBar) };
                    AcrylicCommandBarStyle.Setters.Add(new Setter(BackgroundProperty,
                        AcrylicElementBrush));

                    MainCommandBar.Style = AcrylicCommandBarStyle;

                    FlyoutStyle.Setters.Add(new Setter(BackgroundProperty,
                        AcrylicElementBrush));

                    SolidColorBrush transparentBrush = new SolidColorBrush(Colors.Transparent);
                    Extensions.PaneBackground = AcrylicElementBrush;

                    titleBar.BackgroundColor = Colors.Transparent;
                    titleBar.ButtonBackgroundColor = Colors.Transparent;
                    if (titleBarColor == "0")
                    {
                        LeftAppTitleBar.Background = AcrylicSystemBrush;
                        MiddleAppTitleBar.Background = AcrylicSystemBrush;
                    }
                    else
                    {
                        titleBar.ButtonForegroundColor = Colors.White;
                        titleBar.BackgroundColor = Resources["SystemAccentColor"] as Color?;
                        LeftAppTitleBar.RequestedTheme = ElementTheme.Dark;
                        LeftAppTitleBar.Background = AcrylicAccentBrush;
                        MiddleAppTitleBar.RequestedTheme = ElementTheme.Dark;
                        MiddleAppTitleBar.Background = AcrylicAccentBrush;
                    }

                    string TransparencyBool = localSettings.Values["transparency"].ToString();
                    if (TransparencyBool == "1")
                    {
                        MainGrid.Background = AcrylicSystemBrush;
                    }

                    Style AcrylicMenuFlyoutStyle = new Style { TargetType = typeof(MenuFlyoutPresenter) };

                    AcrylicMenuFlyoutStyle.Setters.Add(new Setter(BackgroundProperty,
                        AcrylicElementBrush));
                    AcrylicMenuFlyoutStyle.Setters.Add(new Setter(RequestedThemeProperty,
                        MainGrid.RequestedTheme));

                    InsertSelection.MenuFlyoutPresenterStyle = AcrylicMenuFlyoutStyle;
                    FormatSelection.MenuFlyoutPresenterStyle = AcrylicMenuFlyoutStyle;
                    RecentFilesFlyout.MenuFlyoutPresenterStyle = AcrylicMenuFlyoutStyle;
                }
            }

            AllTabsFlyout.FlyoutPresenterStyle = FlyoutStyle;
            SecondaryCommands.FlyoutPresenterStyle = FlyoutStyle;
            FindFlyout.FlyoutPresenterStyle = FlyoutStyle;
            ReplaceFlyout.FlyoutPresenterStyle = FlyoutStyle;
        }

        private void BasicAccentBrush()
        {            
            Brush BasicAccentBrush =
                Resources["SystemControlBackgroundAccentBrush"] as Brush;

            titleBar.ButtonBackgroundColor = Colors.Transparent;

            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                StatusBar statusBar = StatusBar.GetForCurrentView();
                statusBar.ForegroundColor = Colors.White;
            }

            LeftAppTitleBar.RequestedTheme = ElementTheme.Dark;
            LeftAppTitleBar.Background = BasicAccentBrush;
            MiddleAppTitleBar.RequestedTheme = ElementTheme.Dark;
            MiddleAppTitleBar.Background = BasicAccentBrush;
        }
        /*
        private void Accessibility()
        {
            var BackgroundHighContrastBrush =
                Resources["SystemColorWindowColor"];
            titleBar.BackgroundColor = BackgroundHighContrastBrush as Color?;

            Style HighContrastCommandBarStyle = new Style { TargetType = typeof(CommandBar) };
            HighContrastCommandBarStyle.Setters.Add(new Setter(BackgroundProperty,
                Colors.Black));
            HighContrastCommandBarStyle.Setters.Add(new Setter(ForegroundProperty,
                Colors.White));

            Style FlyoutStyle = new Style { TargetType = typeof(FlyoutPresenter) };

            FlyoutStyle.Setters.Add(new Setter(PaddingProperty,
                1));
            FlyoutStyle.Setters.Add(new Setter(RequestedThemeProperty,
                ElementTheme.Light));
            FlyoutStyle.Setters.Add(new Setter(BackgroundProperty,
                Colors.Black));
            FlyoutStyle.Setters.Add(new Setter(ForegroundProperty,
                Colors.White));

            Style HighContrastMenuFlyoutStyle = new Style { TargetType = typeof(MenuFlyoutPresenter) };

            HighContrastMenuFlyoutStyle.Setters.Add(new Setter(RequestedThemeProperty,
                ApplicationTheme.Light));
            HighContrastMenuFlyoutStyle.Setters.Add(new Setter(BackgroundProperty,
                Colors.Black));

            AllTabsFlyout.FlyoutPresenterStyle = FlyoutStyle;
            SecondaryCommands.FlyoutPresenterStyle = FlyoutStyle;
            FindFlyout.FlyoutPresenterStyle = FlyoutStyle;
            ReplaceFlyout.FlyoutPresenterStyle = FlyoutStyle;

            EditMenu.ContextFlyout.MenuFlyoutPresenterStyle = HighContrastMenuFlyoutStyle;
            InsertSelection.MenuFlyoutPresenterStyle = HighContrastMenuFlyoutStyle;
            FormatSelection.MenuFlyoutPresenterStyle = HighContrastMenuFlyoutStyle;
        }*/
        #endregion
    }
}
