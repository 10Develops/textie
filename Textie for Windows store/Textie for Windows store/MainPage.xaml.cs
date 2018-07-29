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
using Windows.Phone.Devices.Notification;
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
using Windows.UI.Notifications;
using Microsoft.Toolkit.Uwp.Notifications;
using System.Collections.ObjectModel;
using Windows.UI.Input;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.StartScreen;
// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x419

namespace Textie_for_Windows_store
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        SystemNavigationManager currentView = SystemNavigationManager.GetForCurrentView();

        public RichEditBoxCore currentEditBox;

        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        private PrintManager printMan;
        private PrintDocument printDoc;
        private IPrintDocumentSource printDocSource;

        ObservableCollection<Item> dataList = new ObservableCollection<Item>();
        TileCollection colllaunch = new TileCollection();

        DispatcherTimer timer;

        public MainPage()
        {
            this.InitializeComponent();

            var AppView = ApplicationView.GetForCurrentView();

            TitleTextBlock.Text = Package.Current.DisplayName;

            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;

            //PC customization
            var titleBar = AppView.TitleBar;

            string titleBarColor = localSettings.Values["titleBarColor"].ToString();

            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.ApplicationView"))
            {
                if (titleBar != null)
                {
                    ContentGrid.Margin = new Thickness(0, 32.4, 0, 0);
                    Window.Current.SetTitleBar(MiddleAppTitleBar);
                    AppTitleBar.Visibility = Visibility.Visible;
                }
            }

            coreTitleBar.LayoutMetricsChanged += coreTitleBar_LayoutMetricsChanged;

            //Mobile customization
            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                var statusBar = StatusBar.GetForCurrentView();
                if (statusBar != null)
                {
                    statusBar.BackgroundOpacity = 100;

                    if (titleBarColor == "1")
                    {
                        statusBar.BackgroundColor = Resources["SystemAccentColor"] as Color?;
                        statusBar.ForegroundColor = Colors.White;
                    }

                    ContentGrid.Margin = new Thickness(0, 0, 0, 0);
                    AppTitleBar.Visibility = Visibility.Collapsed;
                }
            }

            Window.Current.CoreWindow.Activated += CoreWindow_Activated;

            Window.Current.CoreWindow.SizeChanged += CoreWindow_SizeChanged;

            InputPane pane = InputPane.GetForCurrentView();
            pane.Showing += Pane_Showing;
            pane.Hiding += Pane_Hiding;
            currentView.BackRequested += CurrentView_BackRequested;

            this.NavigationCacheMode = NavigationCacheMode.Enabled;

            MakeKeyAccelerators();

            MakeDesign();

            AllTabsList.ItemsSource = dataList;

            AddTab("New Tab " + PivotMain.Items.Count);

            timer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(10) };
            timer.Tick += PeriodicSave;
            timer.Start();

            Style s = new Style { TargetType = typeof(MenuFlyoutPresenter) };
            s.Setters.Add(new Setter(RequestedThemeProperty,
                MainGrid.RequestedTheme));
            MenuFlyoutMain.MenuFlyoutPresenterStyle = s;
        }

        private void coreTitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args)
        {
            MiddleAppTitleBar.Margin = new Thickness(0, 8, 64, 0);
        }

        private void CoreWindow_Activated(CoreWindow sender, WindowActivatedEventArgs e)
        {
            ShareButton.IsEnabled = true;

            if (currentEditBox != null)
            {
                var appView = ApplicationView.GetForCurrentView();
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
            var appView = ApplicationView.GetForCurrentView();
            if (appView.IsFullScreenMode)
            {
                //maximized
                ContentGrid.Margin = new Thickness(0, -48, 0, 0);
               
                ApplicationView.GetForCurrentView().SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow);

                AppTitleBar.Visibility = Visibility.Collapsed;
            }
            else
            {
                ApplicationView.GetForCurrentView().SetDesiredBoundsMode(ApplicationViewBoundsMode.UseVisible);

                if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.ApplicationView"))
                {
                    var titleBar = appView.TitleBar;
                    if (titleBar != null)
                    {
                        ContentGrid.Margin = new Thickness(0, 32.4, 0, 0);

                        AppTitleBar.Visibility = Visibility.Visible;
                    }
                }

                if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
                {
                    var statusBar = StatusBar.GetForCurrentView();
                    if (statusBar != null)
                    {
                        ContentGrid.Margin = new Thickness(0, 0, 0, 0);

                        AppTitleBar.Visibility = Visibility.Collapsed;
                    }
                }

                e.Handled = true;
            }
        }

        private void CurrentView_BackRequested(object sender, BackRequestedEventArgs e)
        {
            InputPane pane = InputPane.GetForCurrentView();
            var appView = ApplicationView.GetForCurrentView();
            if (appView.IsFullScreenMode)
            {
                appView.ExitFullScreenMode();
                e.Handled = true;
            }
            else
            {
                e.Handled = false;
            }
        }

        private void Pane_Showing(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            if (currentEditBox != null)
            {
                currentEditBox.Margin = new Thickness(
                    0, 0, 0,
                    sender.OccludedRect.Height);
            }
        }

        private void Pane_Hiding(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            if (currentEditBox != null)
            {
                currentEditBox.Margin = new Thickness(
                    0, 0, 0, 0);
            }
        }

        private void DataTransferManager_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            DataRequest request = args.Request;

            string textStr;
            currentEditBox.Document.GetText(TextGetOptions.None, out textStr);

            request.Data.Properties.Title = "Text Share";
            request.Data.Properties.Description = "Share text from file";
            request.Data.SetText(textStr);
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var args = e.Parameter as Windows.ApplicationModel.Activation.IActivatedEventArgs;
            if (args != null)
            {
                if (args.Kind == Windows.ApplicationModel.Activation.ActivationKind.File)
                {
                    var fileArgs = args as Windows.ApplicationModel.Activation.FileActivatedEventArgs;
                    string strFilePath = fileArgs.Files[0].Path;
                    var file = (StorageFile)fileArgs.Files[0];
                    await OpenFile(file);
                }
            }

            SettingsButton.IsEnabled = true;
            try
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
            catch (Exception)
            {
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            MainCommandBar.IsOpen = false;

            try
            {
                printMan.PrintTaskRequested -= PrintTaskRequested;

                printDoc.Paginate -= Paginate;
                printDoc.GetPreviewPage -= GetPreviewPage;
                printDoc.AddPages -= AddPages;

                printMan = null;
                printDoc = null;
                printDocSource = null;
            }
            catch (Exception)
            {

            }
        }

        #region "Grids"
        private void MainGrid_Loading(FrameworkElement sender, object args)
        {
            AddTabButton.Label = "";
            CloseTabButton.Label = "";
            OpenButton.Label = "";
            SaveButton.Label = "";
            EditButton.Label = "";

            ResourceLoader loader = ResourceLoader.GetForCurrentView();

            string AllTabsText = loader.GetString("AllTabsToolTip");
            ToolTipService.SetToolTip(AllTabsButton, AllTabsText);

            string AddTabText = loader.GetString("AddTabToolTip");
            ToolTipService.SetToolTip(AddTabButton, AddTabText + " (Ctrl + T)");

            string CloseTabText = loader.GetString("CloseTabToolTip");
            ToolTipService.SetToolTip(CloseTabButton, CloseTabText + " (Ctrl + W)");

            string OpenText = loader.GetString("OpenToolTip");
            ToolTipService.SetToolTip(OpenButton, OpenText + " (Ctrl + O)");

            string SaveText = loader.GetString("SaveToolTip");
            ToolTipService.SetToolTip(SaveButton, SaveText + " (Ctrl + S)");

            string EditText = loader.GetString("EditToolTip");
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

            var CanPaste = currentEditBox.Document.CanPaste();
            PasteButton.IsEnabled = CanPaste;

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
                var appView = ApplicationView.GetForCurrentView();
                if (appView.IsFullScreenMode)
                {
                    appView.ExitFullScreenMode();
                }
                else
                {
                    ShowFullScreen();
                }
            }

            var ctrl = Window.Current.CoreWindow.GetKeyState(VirtualKey.Control);
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

        private void MainCommandBar_Opening(object sender, object e)
        {
            if (EditButton.Visibility == Visibility.Visible)
            {
                SecondaryCommands.ShowAt(EditButton);
            }
            else
            {
                SecondaryCommands.ShowAt(SaveButton);
            }
        }


        private void SecondaryCommands_Closed(object sender, object e)
        {
            if (MainCommandBar.IsOpen == true)
            {
                MainCommandBar.IsOpen = false;
            }

            var appView = ApplicationView.GetForCurrentView();
            if (appView.IsFullScreenMode)
            {
                currentEditBox.Focus(FocusState.Programmatic);
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

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFile(PivotMain.SelectedRichEditBoxItem, true);
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

        private void SaveAllButton_Click(object sender, RoutedEventArgs e)
        {
            ItemCollection pages = PivotMain.Items;
            foreach (RichEditBoxPivotItem page in pages)
            {
                RichEditBoxCore richEdit = page.EditBox;
                if (richEdit.Tag != null)
                {
                    SaveFile(page, false);
                }
                else
                {
                    NotifySave(page.HeaderTextBlock.Text);
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
            MenuFlyoutMain.ShowAt(MainCommandBar);
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
            Launch((StorageFile)currentEditBox.Tag);
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

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            SettingsButton.IsEnabled = false;

            this.Frame.Navigate(typeof(SettingsPage));
        }
        #endregion

        #endregion

        #region "Edit menu"
        private void CutButton_Click(object sender, RoutedEventArgs e)
        {
            currentEditBox.Document.Selection.Cut();
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            currentEditBox.Document.Selection.Copy();
        }

        private void PasteButton_Click(object sender, RoutedEventArgs e)
        {
            currentEditBox.Document.Selection.Paste(1);
        }

        private void SelectAllButton_Click(object sender, RoutedEventArgs e)
        {
            string textStr;
            currentEditBox.Document.GetText(TextGetOptions.None, out textStr);
            var myRichEditLength = textStr.Length;
            currentEditBox.Document.Selection.SetRange(0, myRichEditLength);
        }

        private void HighlightButton_Click(object sender, RoutedEventArgs e)
        {
            currentEditBox.Document.Selection.CharacterFormat.BackgroundColor = 
                Colors.LimeGreen;
        }

        private void RemoveHighlightButton_Click(object sender, RoutedEventArgs e)
        {
            currentEditBox.Document.Selection.CharacterFormat.BackgroundColor = 
                Colors.White;
        }

        private void UppercaseButton_Click(object sender, RoutedEventArgs e)
        {
            currentEditBox.Document.Selection.ChangeCase(LetterCase.Upper);
        }

        private void LowercaseButton_Click(object sender, RoutedEventArgs e)
        {
            currentEditBox.Document.Selection.ChangeCase(LetterCase.Lower);
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        { 
            ApplicationDataContainer localSettings =
                ApplicationData.Current.LocalSettings;
            string SelectionText = currentEditBox.Document.Selection.Text;
            string SearchEngine = localSettings.Values["SearchEngine"].ToString();
            if ((SelectionText.StartsWith("http://") || SelectionText.StartsWith("https://")) && SelectionText.Contains("."))
            {
                Navigate(SelectionText);
            }
            else
            {
                if (SearchEngine == "Bing")
                {
                    Navigate("https://www.bing.com/search?q=" + SelectionText);
                }
                else if (SearchEngine == "Google")
                {
                    Navigate("https://www.google.com/search?q=" + SelectionText);
                }
                else if (SearchEngine == "Yahoo")
                {
                    Navigate("https://search.yahoo.com/search?p=" + SelectionText);
                }
            }
        }

        private void MenuFlyoutMain_Opened(object sender, object e)
        {
            if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 3))
            {
                if (MenuFlyoutMain.Target == currentEditBox)
                {
                    Vibrate(60);
                }
            }

            currentEditBox.Focus(FocusState.Programmatic);
        }

        private void MenuFlyoutMain_Closed(object sender, object e)
        {
            currentEditBox.Focus(FocusState.Programmatic);
        }
        #endregion

        #region "Items context flyout"

        private void HeaderTextBlock_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            UIElement senderUI = sender as UIElement;
            TabMenuFlyout.ShowAt(senderUI, e.GetPosition(senderUI));
        }

        private void AddTabItem_Click(object sender, RoutedEventArgs e)
        {
            AddTab("New Tab " + PivotMain.Items.Count);
        }

        private void CloseAllTabsItem_Click(object sender, RoutedEventArgs e)
        {
            CloseAllTabs();
        }

        private void CloseOtherTabsItem_Click(object sender, RoutedEventArgs e)
        {
            string TabName = PivotMain.SelectedRichEditBoxItem.Name;
            foreach (RichEditBoxPivotItem Item in PivotMain.Items)
            {
                PivotMain.SelectedItem = Item;
                if (Item.Name != TabName)
                {
                    PivotMain.Items.Remove(Item);
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

        private void CloseTabMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem button = sender as MenuFlyoutItem;
            Item item = button.DataContext as Item;
            CloseOneTab(item.PivotItem);
        }

        private void FileInfoMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem button = sender as MenuFlyoutItem;
            Item item = button.DataContext as Item;
            if (item.File != null)
            {
                item.ShowFileInfoDialog();
            }
        }

        private void SaveFileMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem button = sender as MenuFlyoutItem;
            Item item = button.DataContext as Item;
            SaveFile(item.PivotItem, true);
        }

        #endregion

        private async void mruMenuFlyout_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = sender as MenuFlyoutItem;
            var mru = StorageApplicationPermissions.MostRecentlyUsedList;
            await OpenFile(await mru.GetFileAsync(selectedItem.Tag.ToString()));
        }

        private async void RecentFilesFlyout_Opening(object sender, object e)
        {
            StorageFile file = null;
            try
            {
                foreach (MenuFlyoutItem items in RecentFilesFlyout.Items)
                {
                    var mru = StorageApplicationPermissions.MostRecentlyUsedList;
                    StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                    foreach (AccessListEntry entry in mru.Entries)
                    {
                        string mruToken = entry.Token;
                        file = await mru.GetFileAsync(mruToken);
                        items.Text = file.Name;
                    }
                }
            }
            catch (FileNotFoundException)
            {
                file = null;
            }
        }

        private void TrimLeadingSpaceItem_Click(object sender, RoutedEventArgs e)
        {
            Trim(1);
        }

        private void TrimTrailingSpaceItem_Click(object sender, RoutedEventArgs e)
        {
            Trim(2);
        }

        private void TrimLeadingnTrailingSpaceItem_Click(object sender, RoutedEventArgs e)
        {
            Trim(0);
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

        private void PivotMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var appView = ApplicationView.GetForCurrentView();
            if (appView.IsFullScreenMode)
            {
                appView.ExitFullScreenMode();
            }

            if(PivotMain.SelectedRichEditBoxItem != null)
            {
                PivotMain.SelectedRichEditBoxItem.Margin = new Thickness(0, 0, 0, 0);

                currentEditBox = PivotMain.SelectedRichEditBox;
                currentEditBox.Margin = new Thickness(
                    0, 0, 0, 0);

                currentEditBox.Focus(FocusState.Programmatic);

                var file = (StorageFile)currentEditBox.Tag;
                if (currentEditBox.Tag == null)
                {
                    if(PivotMain.SelectedRichEditBoxItem.Header
                        != null)
                    {
                        appView.Title = 
                            ((TextBlock)PivotMain.SelectedRichEditBoxItem.Header).Text;
                    }
                }
                else
                {
                    appView.Title = Path.GetFileName(file.Path);

                    if (file.Path.EndsWith(".html") || file.Path.EndsWith(".htm"))
                    {
                        LaunchButton.IsEnabled = true;
                    }
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

            SaveButton.IsEnabled = currentEditBox.ReadyToSave;

            var CanUndo = currentEditBox.Document.CanUndo();
            UndoButton.IsEnabled = CanUndo;
            if (CanUndo)
            {
                UndoTitleBarButton.Visibility = Visibility.Visible;
            }
            else
            {
                UndoTitleBarButton.Visibility = Visibility.Collapsed;
            }

            var CanRedo = currentEditBox.Document.CanRedo();
            RedoButton.IsEnabled = CanRedo;
            if (CanRedo)
            {
                RedoTitleBarButton.Visibility = Visibility.Visible;
            }
            else
            {
                RedoTitleBarButton.Visibility = Visibility.Collapsed;
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

            if (currentEditBox.Document.Selection.Text == "")
            {
                CutButton.IsEnabled = false;
                CopyButton.IsEnabled = false;
            }
            else
            {
                CutButton.IsEnabled = true;
                CopyButton.IsEnabled = true;
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
                currentEditBox.ContextFlyout = MenuFlyoutMain;
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
            var CanUndo = currentEditBox.Document.CanUndo();
            UndoButton.IsEnabled = CanUndo;
            if (CanUndo)
            {
                UndoTitleBarButton.Visibility = Visibility.Visible;
            }
            else
            {
                UndoTitleBarButton.Visibility = Visibility.Collapsed;
            }

            var CanRedo = currentEditBox.Document.CanRedo();
            RedoButton.IsEnabled = CanRedo;
            if (CanRedo)
            {
                RedoTitleBarButton.Visibility = Visibility.Visible;
            }
            else
            {
                RedoTitleBarButton.Visibility = Visibility.Collapsed;
            }

            string autoSave = localSettings.Values["autoSave"].ToString();
            if (currentEditBox.ReadyToSave == false || currentEditBox.Tag != null && autoSave == "1")
            {
                SaveButton.IsEnabled = false;
            }
            else
            {
                SaveButton.IsEnabled = true;
                currentEditBox.ReadyToSave = true;
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
                SearchSeparator.Visibility = Visibility.Visible;
                SearchButton.Visibility = Visibility.Visible;

                if (currentEditBox.Document.Selection.CharacterFormat.BackgroundColor ==
                    Colors.LimeGreen)
                {
                    HighlightButton.IsEnabled = false;
                    RemoveHighlightButton.IsEnabled = true;
                }
                else
                {
                    HighlightButton.IsEnabled = true;
                    RemoveHighlightButton.IsEnabled = false;
                }

                if (SelectionText.StartsWith("http://") && SelectionText.Contains("."))
                {
                    Uri selectionUri = new Uri(SelectionText);
                    string uriWithoutScheme = selectionUri.Host + selectionUri.PathAndQuery + selectionUri.Fragment;
                    if (uriWithoutScheme.Length > 20)
                    {
                        SearchButton.Text = string.Format("Go to \"{0}...\"",
                            uriWithoutScheme.Substring(0, 20));
                    }
                    else
                    {
                        SearchButton.Text = string.Format("Go to \"{0}\"",
                            uriWithoutScheme.TrimEnd('/'));
                    }
                }
                else if (SelectionText.StartsWith("https://") && SelectionText.Contains("."))
                {
                    Uri selectionUri = new Uri(SelectionText);
                    string uriWithoutScheme = selectionUri.Host + selectionUri.PathAndQuery + selectionUri.Fragment;
                    if (uriWithoutScheme.Length > 20)
                    {
                        SearchButton.Text = string.Format("Go to \"{0}...\"",
                            uriWithoutScheme.Substring(0, 20));
                    }
                    else
                    {
                        SearchButton.Text = string.Format("Go to \"{0}\"",
                            uriWithoutScheme.TrimEnd('/'));
                    }
                }
                else if (SelectionText.Length > 10)
                {
                    SearchButton.Text = string.Format("Search \"{0}...\" in The Web",
                        SelectionText.Substring(0, 10));
                }
                else
                {
                    SearchButton.Text = string.Format("Search \"{0}\" in The Web",
                    SelectionText);
                }

                CutButton.IsEnabled = true;
                CopyButton.IsEnabled = true;

                UppercaseButton.IsEnabled = true;
                LowercaseButton.IsEnabled = true;

                FindAutoSuggestBox.Text = SelectionText;
                WhatAutoSuggestBox.Text = SelectionText;
            }
            else
            {
                SearchSeparator.Visibility = Visibility.Collapsed;
                SearchButton.Visibility = Visibility.Collapsed;

                CutButton.IsEnabled = false;
                CopyButton.IsEnabled = false;

                UppercaseButton.IsEnabled = false;
                LowercaseButton.IsEnabled = false;

                HighlightButton.IsEnabled = false;
                RemoveHighlightButton.IsEnabled = false;

                if (currentEditBox.Document.Selection.CharacterFormat.BackgroundColor ==
                        Colors.LimeGreen ||
                        currentEditBox.Document.Selection.CharacterFormat.BackgroundColor ==
                        Colors.Yellow)
                {
                    currentEditBox.Document.Selection.CharacterFormat.BackgroundColor =
                        Colors.White;
                }
            }

            var CanPaste = currentEditBox.Document.CanPaste();
            PasteButton.IsEnabled = CanPaste;

            currentEditBox.ReadyToSave = currentEditBox.Document.CanUndo();
        }

        private void currentEditBox_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            UIElement senderUI = sender as UIElement;
            MenuFlyoutMain.ShowAt(senderUI, e.GetPosition(senderUI));
        }

        private void currentEditBox_Holding(object sender, HoldingRoutedEventArgs e)
        {
            UIElement senderUI = sender as UIElement;
            if (e.HoldingState == HoldingState.Started)
            {
                MenuFlyoutMain.ShowAt(senderUI, e.GetPosition(senderUI));
            }
        }

        private void currentEditBox_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            e.Handled = true;
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
            if(MatchCaseFindCheckBox.IsChecked == true)
            {
                currentEditBox.Find(FindAutoSuggestBox.Text, true);
            }
            else
            {
                currentEditBox.Find(FindAutoSuggestBox.Text, false);
            }

            if (currentEditBox.TextSelection != null)
            {
                currentEditBox.TextSelection.CharacterFormat.BackgroundColor = Colors.White;
            }
        }

        private void WhatAutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            WithAutoSuggestBox.Focus(FocusState.Programmatic);
        }

        private void WithAutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if(MatchCaseReplaceCheckBox.IsChecked == true)
            {
                currentEditBox.Replace(WhatAutoSuggestBox.Text, WithAutoSuggestBox.Text, true);
            }
            else
            {
                currentEditBox.Replace(WhatAutoSuggestBox.Text, WithAutoSuggestBox.Text, false);
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
            var printTask = args.Request.CreatePrintTask("Print", PrintTaskSourceRequrested);

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
            var newTab = PivotMain.AddTab();
            newTab.Name = "Tab" + PivotMain.Items.Count;

            if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 3))
            {
                newTab.HeaderTextBlock.ContextFlyout = TabMenuFlyout;
            }
            else
            {
                newTab.HeaderTextBlock.RightTapped += HeaderTextBlock_RightTapped;
            }

            if (Header.Length <= 15)
            {
                newTab.HeaderTextBlock.Text = Header;
            }
            else
            {
                newTab.HeaderTextBlock.Text = 
                    string.Format("{0}...", Header.Substring(0, 15));
            }

            newTab.Tag = newTab;

            var appView = ApplicationView.GetForCurrentView();
            appView.Title = Header;

            TitleTextBlock.Text = string.Format("{0} – {1}", appView.Title,
                Package.Current.DisplayName);

            newTab.ListViewItem.Title = newTab.HeaderTextBlock.Text;

            dataList.Add(newTab.ListViewItem);

            AllTabsList.SelectedItem = newTab.ListViewItem;

            ToolTipService.SetToolTip(newTab.HeaderTextBlock, Header);

            currentEditBox.TextChanged += new RoutedEventHandler(currentEditBox_TextChanged);
            currentEditBox.SelectionChanged += new RoutedEventHandler(currentEditBox_SelectionChanged);
            currentEditBox.DoubleTapped += new DoubleTappedEventHandler(currentEditBox_DoubleTapped);

            if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 3))
            {
                currentEditBox.ContextFlyout = MenuFlyoutMain;
                currentEditBox.ContextMenuOpening += currentEditBox_ContextMenuOpening;
            }
            else
            {
                currentEditBox.RightTapped += currentEditBox_RightTapped;
                currentEditBox.Holding += currentEditBox_Holding;
            }

        }

        private void AddRecentFiles()
        {
            RecentFilesFlyout.Items.Clear();
            var mru = StorageApplicationPermissions.MostRecentlyUsedList;
            foreach (AccessListEntry entry in mru.Entries)
            {
                string mruToken = entry.Token;
                string mruMetadata = entry.Metadata;
                if (RecentFilesFlyout.Items.Count <= 10)
                {
                    MenuFlyoutItem mruMenuFlyout = new MenuFlyoutItem();
                    mruMenuFlyout.Text = mruToken;
                    mruMenuFlyout.Click += mruMenuFlyout_Click;
                    mruMenuFlyout.Tag = mruToken;
                    RecentFilesFlyout.Items.Add(mruMenuFlyout);
                }
                else
                {
                    break;
                }
            }
        }

        private async void NewWindow()
        {
            var currentAV = ApplicationView.GetForCurrentView();
            var newAV = CoreApplication.CreateNewView();
            await newAV.Dispatcher.RunAsync(
                CoreDispatcherPriority.Normal,
                async () =>
                {
                    var newWindow = Window.Current;
                    var newAppView = ApplicationView.GetForCurrentView();

                    var frame = new Frame();
                    frame.Navigate(typeof(MainPage), null);
                    newWindow.Content = frame;
                    newWindow.Activate();

                    await ApplicationViewSwitcher.TryShowAsStandaloneAsync(
                        newAppView.Id,
                        ViewSizePreference.UseMinimum,
                        currentAV.Id,
                        ViewSizePreference.UseMinimum);
                });
        }

        private void CloseOneTab(RichEditBoxPivotItem item)
        {
            if(PivotMain.Items.Count != 1)
            {
                if (item.EditBox.ReadyToSave == true)
                {
                    if (item.EditBox.Tag != null)
                    {
                        ShowMessageBox("Save changes", "Do you want to save changes?",
                            (command) => 
                            {
                                SaveFile(item, true);
                                CloseTab(item);
                            },

                            (command) => 
                            {
                                CloseTab(item);
                            });
                    }
                    else
                    {
                        ShowMessageBox("Save file", "Do you want to save \"" +
                            item.HeaderTextBlock.Text +
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
                }
                else
                {
                    CloseTab(item);
                }
            }
        }

        private void CloseTab(RichEditBoxPivotItem item)
        {
            dataList.Remove(item.ListViewItem);
            item.Tag = null;
            PivotMain.CloseTab(item);
            AllTabsList.SelectedIndex = PivotMain.SelectedIndex;
        }

        private void CloseAllTabs()
        {
            if (PivotMain.Items.Count == 1 && currentEditBox.ReadyToSave == false)
            {
                ClearTabs();
            }
            else
            {
                ShowMessageBoxTwoButton("Warning",
                    "Datas will lose if unsaved. \r\nDo you want clear tabpages?",
                    yesAllTabs_CommandInvoked);
            }
        }

        private void ClearTabs()
        {
            AddTab("");
            PivotMain.Items.Clear();
            dataList.Clear();
            AddTab("New Tab " + PivotMain.Items.Count);
        }

        private async void OpenFile()
        {
            OpenButton.IsEnabled = false;
            SaveButton.IsEnabled = false;

            var picker = new FileOpenPicker();
            picker.ViewMode = PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            picker.FileTypeFilter.Add(".txt");
            picker.FileTypeFilter.Add(".text");
            picker.FileTypeFilter.Add(".rtf");
            picker.FileTypeFilter.Add(".log");
            picker.FileTypeFilter.Add(".htm");
            picker.FileTypeFilter.Add(".html");
            picker.FileTypeFilter.Add(".js");
            picker.FileTypeFilter.Add(".css");
            picker.FileTypeFilter.Add(".xml");
            picker.FileTypeFilter.Add(".d");
            picker.FileTypeFilter.Add(".c");
            picker.FileTypeFilter.Add(".cs");
            picker.FileTypeFilter.Add(".cpp");
            picker.FileTypeFilter.Add(".h");

            IAsyncOperation<StorageFile> pickSingleFile = picker.PickSingleFileAsync();
            StorageFile file = await pickSingleFile;

            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            if (file != null)
            {
                string FileName = Path.GetFileName(file.Path);
                if (currentEditBox.Document.CanUndo())
                {
                    AddTab(FileName);
                }
                else
                {
                    var header =
                        PivotMain.SelectedRichEditBoxItem.HeaderTextBlock;

                    if (FileName.Length <= 15)
                    {
                        header.Text = FileName;
                    }
                    else
                    {
                        header.Text = string.Format("{0}...", FileName.Substring(0, 15));
                    }

                    PivotMain.SelectedRichEditBoxItem.ListViewItem.Title = header.Text;

                    ToolTipService.SetToolTip(header, FileName);
                }

                var appView = ApplicationView.GetForCurrentView();
                appView.Title = FileName;

                TitleTextBlock.Text = string.Format("{0} – {1}", appView.Title,
                    Package.Current.DisplayName);

                string textToOpen = await FileIO.ReadTextAsync(file);
                if (file.Path.EndsWith(".rtf"))
                {
                    currentEditBox.TextRtf = textToOpen;
                }
                else
                {
                    currentEditBox.Text = textToOpen;
                }

                StorageFile copiedFile = await localFolder.CreateFileAsync(FileName, 
                    CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteTextAsync(copiedFile, textToOpen);

                var mru = StorageApplicationPermissions.MostRecentlyUsedList;
                string mruToken = mru.Add(file, copiedFile.Path, RecentStorageItemVisibility.AppAndSystem);

                LaunchButton.IsEnabled = false;

                if (file.Path.EndsWith(".html") ||
                    file.Path.EndsWith(".htm"))
                {
                    LaunchButton.IsEnabled = true;
                }

                currentEditBox.ReadyToSave = false;
                currentEditBox.Tag = file;

                PivotMain.SelectedRichEditBoxItem.ListViewItem.File = file;
                FileNameItem.IsEnabled = true;
                FilePathItem.IsEnabled = true;
            }
            else
            {
                SaveButton.IsEnabled = currentEditBox.ReadyToSave;
            }

            OpenButton.IsEnabled = true;

            pickSingleFile.Close();
        }

        private async Task OpenFile(StorageFile file)
        {
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            if (file != null)
            {
                string FileName = Path.GetFileName(file.Path);
                if (currentEditBox.Document.CanUndo())
                {
                    AddTab(FileName);
                }
                else
                {
                    var header =
                        PivotMain.SelectedRichEditBoxItem.HeaderTextBlock;

                    if (FileName.Length <= 15)
                    {
                        header.Text = FileName;
                    }
                    else
                    {
                        header.Text = string.Format("{0}...", FileName.Substring(0, 15));
                    }

                    PivotMain.SelectedRichEditBoxItem.ListViewItem.Title = header.Text;

                    ToolTipService.SetToolTip(header, FileName);
                }

                var appView = ApplicationView.GetForCurrentView();
                appView.Title = FileName;

                TitleTextBlock.Text = string.Format("{0} – {1}", appView.Title,
                    Package.Current.DisplayName);

                string textToOpen = await FileIO.ReadTextAsync(file);
                if (file.Path.EndsWith(".rtf"))
                {
                    currentEditBox.TextRtf = textToOpen;
                }
                else
                {
                    currentEditBox.Text = textToOpen;
                }

                StorageFile copiedFile = await localFolder.CreateFileAsync(FileName,
                    CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteTextAsync(copiedFile, textToOpen);

                var mru = StorageApplicationPermissions.MostRecentlyUsedList;
                string mruToken = mru.Add(file, copiedFile.Path, RecentStorageItemVisibility.AppAndSystem);

                LaunchButton.IsEnabled = false;

                if (file.Path.EndsWith(".html") ||
                    file.Path.EndsWith(".htm"))
                {
                    LaunchButton.IsEnabled = true;
                }

                currentEditBox.ReadyToSave = false;
                currentEditBox.Tag = file;

                PivotMain.SelectedRichEditBoxItem.ListViewItem.File = file;
                FileNameItem.IsEnabled = true;
                FilePathItem.IsEnabled = true;

                SaveButton.IsEnabled = false;
            }
        }

        private async void SaveFile(RichEditBoxPivotItem item, bool SaveAs)
        {
            if (item.EditBox.Tag != null)
            {
                StorageFile file = (StorageFile)item.EditBox.Tag;
                string FileName = Path.GetFileName(file.Path);
                StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                IAsyncOperation<StorageFile> copiedFileOperation = localFolder.CreateFileAsync(FileName,
                    CreationCollisionOption.ReplaceExisting);
                StorageFile copiedFile = await copiedFileOperation;

                IAsyncAction writeText = FileIO.WriteTextAsync(copiedFile, item.EditBox.Text);
                await writeText;
                IAsyncAction writeCopiedText = copiedFile.CopyAndReplaceAsync(file);
                await writeCopiedText;

                string textToRefresh = await FileIO.ReadTextAsync(file);
                if (file.Path.EndsWith(".rtf"))
                {
                    item.EditBox.TextRtf = textToRefresh;
                }
                else
                {
                    item.EditBox.Text = textToRefresh;
                }

                SaveButton.IsEnabled = false;
                item.EditBox.ReadyToSave = false;

                copiedFileOperation.Close();
                writeText.Close();
                writeCopiedText.Close();
            }
            else
            {
                if(SaveAs == true)
                {
                    SaveFileAs(item, false);
                }
            }

        }

        private void PeriodicSave(object sender, object e)
        {
            string autoSave = localSettings.Values["autoSave"].ToString();
            if (autoSave == "1")
            {
                foreach(RichEditBoxPivotItem item in PivotMain.Items)
                {
                    SaveFile(item, false);
                }
            }
        }

        private async void SaveFileAs(RichEditBoxPivotItem item, bool SaveCloseTab)
        {
            OpenButton.IsEnabled = false;

            SaveButton.IsEnabled = false;
            SaveAsButton.IsEnabled = false;

            var picker = new FileSavePicker();
            picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            picker.FileTypeChoices.Add("Text Files", new string[] { ".txt", ".text" });
            picker.FileTypeChoices.Add("RTF File", new string[] { ".rtf" });
            picker.FileTypeChoices.Add("Log File", new string[] { ".log"});
            picker.FileTypeChoices.Add("HTML Files", new string[] { ".htm", ".html" });
            picker.FileTypeChoices.Add("CSS File", new string[] { ".css" });
            picker.FileTypeChoices.Add("JavaScript File", new string[] { ".js" });
            picker.FileTypeChoices.Add("XML File", new string[] { ".xml" });
            picker.FileTypeChoices.Add("D source file", new string[] { ".d" });
            picker.FileTypeChoices.Add("C source file", new string[] { ".c" });
            picker.FileTypeChoices.Add("C# source file", new string[] { ".cs" });
            picker.FileTypeChoices.Add("C++ source files", new string[] { ".cpp", ".h" });
            picker.FileTypeChoices.Add("All files", new string[] { "." });
            picker.SuggestedFileName = "New File " + PivotMain.Items.Count;

            IAsyncOperation<StorageFile> pickSaveFile = picker.PickSaveFileAsync();
            StorageFile file = await pickSaveFile;

            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            if (file != null)
            {
                string FileName = Path.GetFileName(file.Path);

                var appView = ApplicationView.GetForCurrentView();
                appView.Title = FileName;

                TitleTextBlock.Text = string.Format("{0} – {1}", appView.Title,
                    Package.Current.DisplayName);

                if (file.Path.EndsWith(".rtf"))
                {
                    await FileIO.WriteTextAsync(file, item.EditBox.TextRtf);
                }
                else
                {
                    await FileIO.WriteTextAsync(file, item.EditBox.Text);
                }

                var header = item.HeaderTextBlock;

                if (FileName.Length <= 15)
                {
                    header.Text = FileName;
                }
                else
                {
                    header.Text = string.Format("{0}...", FileName.Substring(0, 15));
                }

                item.ListViewItem.Title = header.Text;

                ToolTipService.SetToolTip(header, FileName);

                StorageFile copiedFile = await localFolder.CreateFileAsync(FileName,
                    CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteTextAsync(copiedFile, item.EditBox.Text);

                var mru = StorageApplicationPermissions.MostRecentlyUsedList;
                string mruToken = mru.Add(file, copiedFile.Path, RecentStorageItemVisibility.AppAndSystem);

                string textToRefresh = await FileIO.ReadTextAsync(file);
                if (file.Path.EndsWith(".rtf"))
                {
                    item.EditBox.TextRtf = textToRefresh;
                }
                else
                {
                    item.EditBox.Text = textToRefresh;
                }

                LaunchButton.IsEnabled = false;

                if (file.Path.EndsWith(".html") ||
                    file.Path.EndsWith(".htm"))
                {
                    LaunchButton.IsEnabled = true;
                }

                item.EditBox.Tag = file;
                item.EditBox.ReadyToSave = false;

                item.ListViewItem.File = file;
                FileNameItem.IsEnabled = true;
                FilePathItem.IsEnabled = true;

                if (SaveCloseTab == true)
                {
                    CloseTab(item);
                }
            }
            else
            {
                SaveButton.IsEnabled = item.EditBox.ReadyToSave;
            }

            OpenButton.IsEnabled = true;

            SaveAsButton.IsEnabled = true;

            pickSaveFile.Close();
        }

        private void NotifySave(string Header)
        {
            // In a real app, these would be initialized with actual data
            string title = "Save file";
            string content = string.Format("\"{0}\" is not saved because is not have a saved file.", 
                Header);
            // Construct the visuals of the toast
            ToastVisual visual = new ToastVisual()
            {
                BindingGeneric = new ToastBindingGeneric()
                {
                    Children =
                    {
                        new AdaptiveText()
                        {
                            Text = title
                        },

                        new AdaptiveText()
                        {
                            Text = content
                        },
                    },
                }
            };

            // Now we can construct the final toast content
            ToastContent toastContent = new ToastContent()
            {
                Visual = visual
            };

            // And create the toast notification
            var toast = new ToastNotification(toastContent.GetXml());
            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }

        private void Trim(int option)
        {
            string textStr;
            currentEditBox.Document.GetText(TextGetOptions.None, out textStr);

            var myRichEditLength = textStr.Length;
            currentEditBox.Document.Selection.SetRange(0, myRichEditLength);
            if(option == 0)
            {
                currentEditBox.Document.Selection.Text = currentEditBox.Document.Selection.Text.Trim();
            }
            else if(option == 1)
            {
                currentEditBox.Document.Selection.Text = currentEditBox.Document.Selection.Text.TrimStart();
            }
            else if (option == 2)
            {
                currentEditBox.Document.Selection.Text = currentEditBox.Document.Selection.Text.TrimEnd();
            }
        }

        private void Vibrate(double timeMilliseconds)
        {
            string VibrateBool = localSettings.Values["vibrate"].ToString();
            if (VibrateBool == "1" &&
                ApiInformation.IsTypePresent("Windows.Phone.Devices.Notification.VibrationDevice"))
            {
                var device = VibrationDevice.GetDefault();
                device.Vibrate(TimeSpan.FromMilliseconds(timeMilliseconds));
            }
        }

        private async void ShowMessageBox(string Title, string Content,
            UICommandInvokedHandler CommandYes, UICommandInvokedHandler CommandNo)
        {
            var messageDialog = new MessageDialog(Content, Title);

            var yesCommand = new UICommand("Yes", CommandYes);
            var noCommand = new UICommand("No", CommandNo);
            var cancelCommand = new UICommand("Cancel");

            messageDialog.Commands.Add(yesCommand);

            messageDialog.DefaultCommandIndex = 0;
            messageDialog.CancelCommandIndex = 1;

            if (noCommand != null)
            {
                messageDialog.Commands.Add(noCommand);
                messageDialog.CancelCommandIndex = (uint)messageDialog.Commands.Count - 1;
            }

            if (cancelCommand != null)
            {
                // Devices with a hardware back button
                // use the hardware button for Cancel.
                // for other devices, show a third option

                var t_hardwareBackButton = "Windows.Phone.UI.Input.HardwareButtons";

                if (ApiInformation.IsTypePresent(t_hardwareBackButton))
                {
                    // disable the default Cancel command index
                    // so that dialog.ShowAsync() returns null
                    // in that case

                    messageDialog.CancelCommandIndex = UInt32.MaxValue;
                }
                else
                {
                    messageDialog.Commands.Add(cancelCommand);
                    messageDialog.CancelCommandIndex = (uint)messageDialog.Commands.Count - 1;
                }
            }

            await messageDialog.ShowAsync();
        }

        private async void ShowMessageBoxTwoButton(string Title, string Content,
            UICommandInvokedHandler CommandYes)
        {
            var messageDialog = new MessageDialog(Content, Title);

            var yesCommand = new UICommand("Yes", CommandYes);
            var noCommand = new UICommand("No");

            messageDialog.Commands.Add(yesCommand);
            messageDialog.Commands.Add(noCommand);

            messageDialog.DefaultCommandIndex = 0;
            messageDialog.CancelCommandIndex = 1;

            // Devices with a hardware back button
            // use the hardware button for Cancel.
            // for other devices, show a third option

            await messageDialog.ShowAsync();
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
                        Content = "\nSorry, printing can' t proceed at this time.",
                        PrimaryButtonText = "OK"
                    };

                    await noPrintingDialog.ShowAsync();
                }
            }
            else
            {
                // Printing is not supported on this device
                ContentDialog noPrintingDialog = new ContentDialog()
                {
                    Title = "Printing not supported",
                    Content = "\nSorry, printing is not supported on this device.",
                    PrimaryButtonText = "OK"
                };
                    await noPrintingDialog.ShowAsync();
            }
        }

        private void ShareText()
        {
            DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += DataTransferManager_DataRequested;
            DataTransferManager.ShowShareUI();
        }

        private async void Launch(StorageFile htmlFile)
        {
            // Path to the file in the app package to launch

            if (htmlFile != null)
            {
                // Launch the retrieved file
                var success = await Launcher.LaunchFileAsync(htmlFile);

                if (success)
                {
                    // File launched
                }
                else
                {
                    var messageDialog = new MessageDialog(htmlFile.Path, "Site");
                    await messageDialog.ShowAsync();
                }
            }
            else
            {
                // Could not find file
            }
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
            var applicationView = ApplicationView.GetForCurrentView();

            applicationView.TryEnterFullScreenMode();

            SecondaryCommands.Hide();
        }

        private async void Navigate(string Url)
        {
            var uri = new Uri(Url);

            var success = await Launcher.LaunchUriAsync(uri);
            if (success)
            {
                // URI launched
            }
            else
            {
                // URI launch failed
            }
        }
        #endregion

        #region "Key Accelerators"
        private void MakeKeyAccelerators()
        {
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
            }
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
                //AppTitleBar.Background = BasicBackBrush;
            }
            else
            {
                BasicAccentBrush();
            }

            //ContentGrid.Background = BasicBackBrush;

            if (theme == "WD")
            {
                if (ApiInformation.IsTypePresent("Windows.UI.Xaml.Media.RevealBrush"))
                {
                    var AppBarButtonReveal =
                        Resources["AppBarButtonRevealStyle"] as Style;
                    var AppBarButtonOverflowReveal =
                        Resources["RightAlignRevealAppBarButton"] as Style;
                    var AppBarToggleOverflowReveal =
                        Resources["RightAlignRevealAppBarToggle"] as Style;

                    var ButtonReveal = Resources["ButtonRevealStyle"] as Style;
                    UndoTitleBarButton.Style = ButtonReveal;
                    RedoTitleBarButton.Style = ButtonReveal;

                    MainCommandBar.Style =
                        Resources["CommandBarRevealStyle"] as Style;

                    AllTabsButton.Style = AppBarButtonReveal;

                    NewWindowButton.Style = AppBarButtonReveal;
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
                    LaunchButton.Style = AppBarButtonOverflowReveal;
                    ZoomOutButton.Style = AppBarButtonOverflowReveal;
                    ZoomInButton.Style = AppBarButtonOverflowReveal;
                    FullScreenButton.Style = AppBarButtonOverflowReveal;
                    SettingsButton.Style = AppBarButtonOverflowReveal;
                }

                if (ApiInformation.IsTypePresent("Windows.UI.Xaml.Media.AcrylicBrush"))
                {
                    var AcrylicSystemBrush =
                        Resources["SystemControlChromeMediumAcrylicWindowMediumBrush"] as AcrylicBrush;

                    var AcrylicElementBrush =
                        Resources["SystemControlChromeMediumAcrylicElementMediumBrush"] as AcrylicBrush;

                    titleBar.BackgroundColor = Colors.Transparent;
                    titleBar.ButtonBackgroundColor = Colors.Transparent;
                    if (titleBarColor == "0")
                    {
                        AppTitleBar.Background = AcrylicSystemBrush;
                    }
                    else
                    {
                        titleBar.ButtonForegroundColor = Colors.White;
                        titleBar.BackgroundColor = Resources["SystemAccentColor"] as Color?;
                        AppTitleBar.RequestedTheme = ElementTheme.Dark;
                        AppTitleBar.Background =
                        Resources["SystemControlAccentAcrylicWindowAccentMediumHighBrush"] as AcrylicBrush;
                    }

                    string TransparencyBool = localSettings.Values["transparency"].ToString();
                    if (TransparencyBool == "1")
                    {
                        ContentGrid.Background = AcrylicSystemBrush;

                        MainCommandBar.Background = AcrylicElementBrush;

                        Style AcrylicFlyoutStyle = new Style { TargetType = typeof(FlyoutPresenter) };

                        AcrylicFlyoutStyle.Setters.Add(new Setter(BackgroundProperty,
                            AcrylicElementBrush));
                        AcrylicFlyoutStyle.Setters.Add(new Setter(RequestedThemeProperty,
                            MainGrid.RequestedTheme));

                        AllTabsFlyout.FlyoutPresenterStyle = AcrylicFlyoutStyle;
                        SecondaryCommands.FlyoutPresenterStyle = AcrylicFlyoutStyle;
                        FindFlyout.FlyoutPresenterStyle = AcrylicFlyoutStyle;
                        ReplaceFlyout.FlyoutPresenterStyle = AcrylicFlyoutStyle;

                        Style AcrylicMenuFlyoutStyle = new Style { TargetType = typeof(MenuFlyoutPresenter) };

                        AcrylicMenuFlyoutStyle.Setters.Add(new Setter(BackgroundProperty,
                            AcrylicElementBrush));
                        AcrylicMenuFlyoutStyle.Setters.Add(new Setter(RequestedThemeProperty,
                            MainGrid.RequestedTheme));

                        InsertSelection.MenuFlyoutPresenterStyle = AcrylicMenuFlyoutStyle;
                        FormatSelection.MenuFlyoutPresenterStyle = AcrylicMenuFlyoutStyle;
                    }
                }
            }
        }

        private void BasicAccentBrush()
        {
            var titleBar = ApplicationView.GetForCurrentView().TitleBar;

            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.BackgroundColor = Resources["SystemAccentColor"] as Color?;
            AppTitleBar.RequestedTheme = ElementTheme.Dark;
            AppTitleBar.Background = Resources["SystemControlBackgroundAccentBrush"] as Brush;
        }
        #endregion

        private async void PinFileStartMenu_Click(object sender, RoutedEventArgs e)
        {
            if(currentEditBox.Tag != null)
            {
                string tileId;

                string path = ((StorageFile)currentEditBox.Tag).Path;
                tileId = "file" + PivotMain.Items.Count;
                var mycoll = colllaunch.coll;
                mycoll.Add(tileId, path);
                // Use a display name you like
                string displayName;
                if (PivotMain.SelectedRichEditBoxItem.HeaderTextBlock.Text.Length > 10
                    && currentEditBox.Tag != null)
                {
                    displayName = ((StorageFile)currentEditBox.Tag).Name;
                }
                else
                {
                    displayName = PivotMain.SelectedRichEditBoxItem.HeaderTextBlock.Text;
                }

                // Provide all the required info in arguments so that when user
                // clicks your tile, you can navigate them to the correct content
                string arguments = tileId;

                var imageUri = new Uri("ms-appx:///Assets/Square150x150Logo.scale-100.png");

                // During creation of secondary tile, an application may set additional arguments on the tile that will be passed in during activation.

                // Create a Secondary tile with all the required arguments.
                var secondaryTile = new SecondaryTile(tileId,
                    displayName,
                    arguments,
                    imageUri, TileSize.Default);
                secondaryTile.VisualElements.ShowNameOnSquare150x150Logo = true;

                await secondaryTile.RequestCreateAsync();
            }
        }
    }
}