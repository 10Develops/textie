using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Foundation.Metadata;
using Windows.Storage;
using Windows.System;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace Textie
{
    public class RichEditBoxMenu
    {
        RichEditBoxContextFlyout _contextFlyout;
        RichEditBoxCore _core;
        object _selectionFlyout;
        public RichEditBoxMenu() : this(new RichEditBoxContextFlyout())
        {
        }

        public RichEditBoxMenu(RichEditBoxContextFlyout contextFlyout)
        {
            _contextFlyout = contextFlyout;
            if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 7))
            {
                _selectionFlyout = new RichEditBoxSelectionFlyout();
            }
            else
            {
                _selectionFlyout = null;
            }
        }

        public RichEditBoxCore Core
        {
            get
            {
                return _core;
            }
            set
            {
                _core = value;
                _contextFlyout.Core = value;
                if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 7))
                {
                    RichEditBoxSelectionFlyout Selection = _selectionFlyout as RichEditBoxSelectionFlyout;
                    Selection.Core = value;
                }
            }
        }

        public object SelectionFlyout
        {
            get
            {
                return _selectionFlyout;
            }
        }

        public RichEditBoxContextFlyout ContextFlyout
        {
            get
            {
                return _contextFlyout;
            }
        }

        public class RichEditBoxContextFlyout : MenuFlyout
        {
            RichEditBoxCore _core;

            MenuFlyoutItem CutButton;
            MenuFlyoutItem CopyButton;
            MenuFlyoutItem PasteButton;
            MenuFlyoutSeparator CutCopyPasteSeparator;
            MenuFlyoutItem SelectAllButton;
            MenuFlyoutSeparator HighlightSeparator;
            MenuFlyoutItem HighlightButton;
            MenuFlyoutItem RemoveHighlightButton;
            MenuFlyoutSeparator CaseSeparator;
            MenuFlyoutItem UppercaseButton;
            MenuFlyoutItem LowercaseButton;
            MenuFlyoutSeparator SearchSeparator;
            MenuFlyoutItem SearchButton;

            public RichEditBoxContextFlyout()
            {
                ResourceLoader loader = ResourceLoader.GetForCurrentView();

                Opened += RichEditBoxContextFlyout_Opened;
                Closed += RichEditBoxContextFlyout_Closed;

                CutButton = new MenuFlyoutItem() { Text = loader.GetString("CutButton/Text") };
                CutButton.Click += CutButton_Click;
                Items.Add(CutButton);

                CopyButton = new MenuFlyoutItem() { Text = loader.GetString("CopyButton/Text") };
                CopyButton.Click += CopyButton_Click;
                Items.Add(CopyButton);

                PasteButton = new MenuFlyoutItem() { Text = loader.GetString("PasteButton/Text") };
                PasteButton.Click += PasteButton_Click;
                Items.Add(PasteButton);

                CutCopyPasteSeparator = new MenuFlyoutSeparator();
                Items.Add(CutCopyPasteSeparator);

                SelectAllButton = new MenuFlyoutItem() { Text = loader.GetString("SelectAllButton/Text") };
                SelectAllButton.Click += SelectAllButton_Click;
                Items.Add(SelectAllButton);

                HighlightSeparator = new MenuFlyoutSeparator();
                Items.Add(HighlightSeparator);

                HighlightButton = new MenuFlyoutItem() { Text = loader.GetString("HighlightButton/Text") };
                HighlightButton.Click += HighlightButton_Click;
                Items.Add(HighlightButton);

                RemoveHighlightButton = new MenuFlyoutItem() { Text = loader.GetString("RemoveHighlightButton/Text") };
                RemoveHighlightButton.Click += RemoveHighlightButton_Click;
                Items.Add(RemoveHighlightButton);

                CaseSeparator = new MenuFlyoutSeparator();
                Items.Add(CaseSeparator);

                UppercaseButton = new MenuFlyoutItem() { Text = loader.GetString("UppercaseButton/Text") };
                UppercaseButton.Click += UppercaseButton_Click;
                Items.Add(UppercaseButton);

                LowercaseButton = new MenuFlyoutItem() { Text = loader.GetString("LowercaseButton/Text") };
                LowercaseButton.Click += LowercaseButton_Click;
                Items.Add(LowercaseButton);

                SearchSeparator = new MenuFlyoutSeparator();
                Items.Add(SearchSeparator);

                SearchButton = new MenuFlyoutItem() { Text = "[search]" };
                SearchButton.Click += SearchButton_Click;
                Items.Add(SearchButton);

                if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 4))
                {
                    CutButton.Icon = new FontIcon { Glyph = "\uE8C6" };
                    CopyButton.Icon = new FontIcon { Glyph = "\uE8C8" };
                    PasteButton.Icon = new FontIcon { Glyph = "\uE77F" };
                }

                if (ApiInformation.IsTypePresent("Windows.UI.Xaml.Input.KeyboardAccelerator"))
                {
                    KeyboardAccelerator CtrlX = new KeyboardAccelerator();
                    CtrlX.Key = VirtualKey.X;
                    CtrlX.Modifiers = VirtualKeyModifiers.Control;
                    CtrlX.IsEnabled = false;
                    CutButton.AccessKey = "X";
                    CutButton.KeyboardAccelerators.Add(CtrlX);

                    KeyboardAccelerator CtrlC = new KeyboardAccelerator();
                    CtrlC.Key = VirtualKey.C;
                    CtrlC.Modifiers = VirtualKeyModifiers.Control;
                    CtrlC.IsEnabled = false;
                    CopyButton.AccessKey = "C";
                    CopyButton.KeyboardAccelerators.Add(CtrlC);

                    KeyboardAccelerator CtrlV = new KeyboardAccelerator();
                    CtrlV.Key = VirtualKey.V;
                    CtrlV.Modifiers = VirtualKeyModifiers.Control;
                    CtrlV.IsEnabled = false;
                    PasteButton.AccessKey = "V";
                    PasteButton.KeyboardAccelerators.Add(CtrlV);

                    KeyboardAccelerator CtrlA = new KeyboardAccelerator();
                    CtrlA.Key = VirtualKey.A;
                    CtrlA.Modifiers = VirtualKeyModifiers.Control;
                    CtrlA.IsEnabled = false;
                    SelectAllButton.AccessKey = "A";
                    SelectAllButton.KeyboardAccelerators.Add(CtrlA);
                }
            }

            internal RichEditBoxCore Core
            {
                get
                {
                    return _core;
                }
                set
                {
                    _core = value;
                }
            }

            private async void RichEditBoxContextFlyout_Opened(object sender, object e)
            {
                string SelectionText = _core.Document.Selection.Text;
                if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 3))
                {
                     if (Target == _core)
                     {
                        ApiResources.Vibrate(60);
                     }
                }

                var CanCopy = _core.Document.CanCopy();
                if (CanCopy && SelectionText != string.Empty)
                {
                    CutButton.Visibility = Visibility.Visible;
                    CopyButton.Visibility = Visibility.Visible;
                }
                else
                {
                    CutButton.Visibility = Visibility.Collapsed;
                    CopyButton.Visibility = Visibility.Collapsed;
                }

                var CanPaste = _core.Document.CanPaste();
                if (CanPaste)
                {
                    PasteButton.Visibility = Visibility.Visible;
                }
                else
                {
                    PasteButton.Visibility = Visibility.Collapsed;
                }

                if (CopyButton.Visibility == Visibility.Visible || PasteButton.Visibility == Visibility.Visible)
                {
                    CutCopyPasteSeparator.Visibility = Visibility.Visible;
                }
                else
                {
                    CutCopyPasteSeparator.Visibility = Visibility.Collapsed;
                }

                ToolTipService.SetToolTip(_core, null);

                if (SelectionText != string.Empty)
                {
                    SearchSeparator.Visibility = Visibility.Visible;
                    SearchButton.Visibility = Visibility.Visible;

                    if (_core.Document.Selection.CharacterFormat.BackgroundColor ==
                        Colors.LimeGreen)
                    {
                        HighlightButton.Visibility = Visibility.Collapsed;
                        RemoveHighlightButton.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        HighlightButton.Visibility = Visibility.Visible;
                        RemoveHighlightButton.Visibility = Visibility.Collapsed;
                    }

                    if (Uri.IsWellFormedUriString(SelectionText, UriKind.Absolute) && SelectionText.Contains("."))
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
                    else if (SelectionText.Length > 20)
                    {
                        SearchButton.Text = string.Format("Search \"{0}...\" in The Web",
                             SelectionText.Substring(0, 20).Replace("\r", " "));
                    }
                    else
                    {
                        SearchButton.Text = string.Format("Search \"{0}\" in The Web",
                        SelectionText.Replace("\r", " "));
                    }

                    UppercaseButton.Visibility = Visibility.Visible;
                    LowercaseButton.Visibility = Visibility.Visible;

                }
                else
                {
                    SearchSeparator.Visibility = Visibility.Collapsed;
                    SearchButton.Visibility = Visibility.Collapsed;

                    UppercaseButton.Visibility = Visibility.Collapsed;
                    LowercaseButton.Visibility = Visibility.Collapsed;

                    HighlightButton.Visibility = Visibility.Collapsed;
                    RemoveHighlightButton.Visibility = Visibility.Collapsed;
                }

                if (HighlightButton.Visibility == Visibility.Visible || RemoveHighlightButton.Visibility == Visibility.Visible)
                {
                    HighlightSeparator.Visibility = Visibility.Visible;
                }
                else
                {
                    HighlightSeparator.Visibility = Visibility.Collapsed;
                }

                if (UppercaseButton.Visibility == Visibility.Visible || LowercaseButton.Visibility == Visibility.Visible)
                {
                    CaseSeparator.Visibility = Visibility.Visible;
                }
                else
                {
                    CaseSeparator.Visibility = Visibility.Collapsed;
                }

                await Task.Delay(100);
                _core.Focus(FocusState.Programmatic);
            }

            private async void RichEditBoxContextFlyout_Closed(object sender, object e)
            {
                await Task.Delay(100);
                _core.Focus(FocusState.Programmatic);
            }

            private void CutButton_Click(object sender, RoutedEventArgs e) { RichEditBoxMenuEvents.Cut(_core); }

            private void CopyButton_Click(object sender, RoutedEventArgs e) { RichEditBoxMenuEvents.Copy(_core); }

            private void PasteButton_Click(object sender, RoutedEventArgs e) { RichEditBoxMenuEvents.Paste(_core); }

            private void SelectAllButton_Click(object sender, RoutedEventArgs e) { RichEditBoxMenuEvents.SelectAll(_core); }

            private void HighlightButton_Click(object sender, RoutedEventArgs e) { RichEditBoxMenuEvents.Highlight(_core); }

            private void RemoveHighlightButton_Click(object sender, RoutedEventArgs e) { RichEditBoxMenuEvents.RemoveHighlight(_core); }

            private void UppercaseButton_Click(object sender, RoutedEventArgs e) { RichEditBoxMenuEvents.Uppercase(_core); }

            private void LowercaseButton_Click(object sender, RoutedEventArgs e) { RichEditBoxMenuEvents.Lowercase(_core); }

            private void SearchButton_Click(object sender, RoutedEventArgs e) { RichEditBoxMenuEvents.Search(_core); }
        }

        public class RichEditBoxSelectionFlyout : CommandBarFlyout
        {
            RichEditBoxCore _core;

            AppBarButton CutButton;
            AppBarButton CopyButton;
            AppBarButton PasteButton;
            AppBarButton SelectAllButton;
            AppBarSeparator HighlightSeparator;
            AppBarButton HighlightButton;
            AppBarButton RemoveHighlightButton;
            AppBarSeparator CaseSeparator;
            AppBarButton UppercaseButton;
            AppBarButton LowercaseButton;
            AppBarSeparator SearchSeparator;
            AppBarButton SearchButton;

            public RichEditBoxSelectionFlyout()
            {
                Opened += RichEditBoxSelectionFlyout_Opened;

                ResourceLoader loader = ResourceLoader.GetForCurrentView();

                CutButton = new AppBarButton() { Label = loader.GetString("CutButton/Text") };
                CutButton.Click += CutButton_Click;
                PrimaryCommands.Add(CutButton);

                CopyButton = new AppBarButton() { Label = loader.GetString("CopyButton/Text") };
                CopyButton.Click += CopyButton_Click;
                PrimaryCommands.Add(CopyButton);

                PasteButton = new AppBarButton() { Label = loader.GetString("PasteButton/Text") };
                PasteButton.Click += PasteButton_Click;
                PrimaryCommands.Add(PasteButton);

                SelectAllButton = new AppBarButton() { Label = loader.GetString("SelectAllButton/Text") };
                SelectAllButton.Click += SelectAllButton_Click;
                SecondaryCommands.Add(SelectAllButton);

                HighlightSeparator = new AppBarSeparator();
                SecondaryCommands.Add(HighlightSeparator);

                HighlightButton = new AppBarButton() { Label = loader.GetString("HighlightButton/Text") };
                HighlightButton.Click += HighlightButton_Click;
                SecondaryCommands.Add(HighlightButton);

                RemoveHighlightButton = new AppBarButton() { Label = loader.GetString("RemoveHighlightButton/Text") };
                RemoveHighlightButton.Click += RemoveHighlightButton_Click;
                SecondaryCommands.Add(RemoveHighlightButton);

                CaseSeparator = new AppBarSeparator();
                SecondaryCommands.Add(CaseSeparator);

                UppercaseButton = new AppBarButton() { Label = loader.GetString("UppercaseButton/Text") };
                UppercaseButton.Click += UppercaseButton_Click;
                SecondaryCommands.Add(UppercaseButton);

                LowercaseButton = new AppBarButton() { Label = loader.GetString("LowercaseButton/Text") };
                LowercaseButton.Click += LowercaseButton_Click;
                SecondaryCommands.Add(LowercaseButton);

                SearchSeparator = new AppBarSeparator();
                SecondaryCommands.Add(SearchSeparator);

                SearchButton = new AppBarButton() { Label = "[search]" };
                SearchButton.Click += SearchButton_Click;
                SecondaryCommands.Add(SearchButton);

                CutButton.Icon = new FontIcon { Glyph = "\uE8C6" };
                CopyButton.Icon = new FontIcon { Glyph = "\uE8C8" };
                PasteButton.Icon = new FontIcon { Glyph = "\uE77F" };

                if (ApiInformation.IsTypePresent("Windows.UI.Xaml.Input.KeyboardAccelerator"))
                {
                    KeyboardAccelerator CtrlX = new KeyboardAccelerator();
                    CtrlX.Key = VirtualKey.X;
                    CtrlX.Modifiers = VirtualKeyModifiers.Control;
                    CtrlX.IsEnabled = false;
                    CutButton.AccessKey = "X";
                    CutButton.KeyboardAccelerators.Add(CtrlX);

                    KeyboardAccelerator CtrlC = new KeyboardAccelerator();
                    CtrlC.Key = VirtualKey.C;
                    CtrlC.Modifiers = VirtualKeyModifiers.Control;
                    CtrlC.IsEnabled = false;
                    CopyButton.AccessKey = "C";
                    CopyButton.KeyboardAccelerators.Add(CtrlC);

                    KeyboardAccelerator CtrlV = new KeyboardAccelerator();
                    CtrlV.Key = VirtualKey.V;
                    CtrlV.Modifiers = VirtualKeyModifiers.Control;
                    CtrlV.IsEnabled = false;
                    PasteButton.AccessKey = "V";
                    PasteButton.KeyboardAccelerators.Add(CtrlV);

                    KeyboardAccelerator CtrlA = new KeyboardAccelerator();
                    CtrlA.Key = VirtualKey.A;
                    CtrlA.Modifiers = VirtualKeyModifiers.Control;
                    CtrlA.IsEnabled = false;
                    SelectAllButton.AccessKey = "A";
                    SelectAllButton.KeyboardAccelerators.Add(CtrlA);
                }
            }

            private void RichEditBoxSelectionFlyout_Opened(object sender, object e)
            {
                string SelectionText = _core.Document.Selection.Text;

                var CanCopy = _core.Document.CanCopy();
                if (CanCopy && SelectionText != string.Empty)
                {
                    CutButton.Visibility = Visibility.Visible;
                    CopyButton.Visibility = Visibility.Visible;
                }
                else
                {
                    CutButton.Visibility = Visibility.Collapsed;
                    CopyButton.Visibility = Visibility.Collapsed;
                }

                var CanPaste = _core.Document.CanPaste();
                if (CanPaste)
                {
                    PasteButton.Visibility = Visibility.Visible;
                }
                else
                {
                    PasteButton.Visibility = Visibility.Collapsed;
                }


                ToolTipService.SetToolTip(_core, null);

                if (SelectionText != string.Empty)
                {
                    SearchSeparator.Visibility = Visibility.Visible;
                    SearchButton.Visibility = Visibility.Visible;

                    if (_core.Document.Selection.CharacterFormat.BackgroundColor ==
                        Colors.LimeGreen)
                    {
                        HighlightButton.Visibility = Visibility.Collapsed;
                        RemoveHighlightButton.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        HighlightButton.Visibility = Visibility.Visible;
                        RemoveHighlightButton.Visibility = Visibility.Collapsed;
                    }

                    if (Uri.IsWellFormedUriString(SelectionText, UriKind.Absolute) && SelectionText.Contains("."))
                    {
                        Uri selectionUri = new Uri(SelectionText);
                        string uriWithoutScheme = selectionUri.Host + selectionUri.PathAndQuery + selectionUri.Fragment;
                        if (uriWithoutScheme.Length > 20)
                        {
                            SearchButton.Label = string.Format("Go to \"{0}...\"",
                                uriWithoutScheme.Substring(0, 20));
                        }
                        else
                        {
                            SearchButton.Label = string.Format("Go to \"{0}\"",
                                uriWithoutScheme.TrimEnd('/'));
                        }
                    }
                    else if (SelectionText.Length > 20)
                    {
                        SearchButton.Label = string.Format("Search \"{0}...\" in The Web",
                             SelectionText.Substring(0, 20).Replace("\r", " "));
                    }
                    else
                    {
                        SearchButton.Label = string.Format("Search \"{0}\" in The Web",
                        SelectionText.Replace("\r", " "));
                    }

                    UppercaseButton.Visibility = Visibility.Visible;
                    LowercaseButton.Visibility = Visibility.Visible;

                }
                else
                {
                    SearchSeparator.Visibility = Visibility.Collapsed;
                    SearchButton.Visibility = Visibility.Collapsed;

                    UppercaseButton.Visibility = Visibility.Collapsed;
                    LowercaseButton.Visibility = Visibility.Collapsed;

                    HighlightButton.Visibility = Visibility.Collapsed;
                    RemoveHighlightButton.Visibility = Visibility.Collapsed;
                }

                if (HighlightButton.Visibility == Visibility.Visible || RemoveHighlightButton.Visibility == Visibility.Visible)
                {
                    HighlightSeparator.Visibility = Visibility.Visible;
                }
                else
                {
                    HighlightSeparator.Visibility = Visibility.Collapsed;
                }

                if (UppercaseButton.Visibility == Visibility.Visible || LowercaseButton.Visibility == Visibility.Visible)
                {
                    CaseSeparator.Visibility = Visibility.Visible;
                }
                else
                {
                    CaseSeparator.Visibility = Visibility.Collapsed;
                }
            }

            internal RichEditBoxCore Core
            {
                get
                {
                    return _core;
                }
                set
                {
                    _core = value;
                }
            }

            private void CutButton_Click(object sender, RoutedEventArgs e) { RichEditBoxMenuEvents.Cut(_core); }

            private void CopyButton_Click(object sender, RoutedEventArgs e) { RichEditBoxMenuEvents.Copy(_core); }

            private void PasteButton_Click(object sender, RoutedEventArgs e) { RichEditBoxMenuEvents.Paste(_core); }

            private void SelectAllButton_Click(object sender, RoutedEventArgs e) { RichEditBoxMenuEvents.SelectAll(_core); }

            private void HighlightButton_Click(object sender, RoutedEventArgs e) { RichEditBoxMenuEvents.Highlight(_core); }

            private void RemoveHighlightButton_Click(object sender, RoutedEventArgs e) { RichEditBoxMenuEvents.RemoveHighlight(_core); }

            private void UppercaseButton_Click(object sender, RoutedEventArgs e) { RichEditBoxMenuEvents.Uppercase(_core); }

            private void LowercaseButton_Click(object sender, RoutedEventArgs e) { RichEditBoxMenuEvents.Lowercase(_core); }

            private void SearchButton_Click(object sender, RoutedEventArgs e) { RichEditBoxMenuEvents.Search(_core); }
        }

        internal class RichEditBoxMenuEvents
        {
            RichEditBoxCore _core;
            internal RichEditBoxMenuEvents(RichEditBoxCore Core)
            {
                _core = Core;
            }

            internal static void Cut(RichEditBoxCore Core)
            {
                Core.Document.Selection.Cut();
            }

            internal static void Copy(RichEditBoxCore Core)
            {
                Core.Document.Selection.Copy();
            }

            internal static void Paste(RichEditBoxCore Core)
            {
                Core.Document.Selection.Paste(1);
            }

            internal static void SelectAll(RichEditBoxCore Core)
            {
                Core.CoreText.SelectAll();
            }

            internal static void Highlight(RichEditBoxCore Core)
            {
                Core.Document.Selection.CharacterFormat.BackgroundColor =
                    Colors.LimeGreen;
            }

            internal static void RemoveHighlight(RichEditBoxCore Core)
            {
                Core.Document.Selection.CharacterFormat.BackgroundColor =
                    Colors.White;
            }

            internal static void Uppercase(RichEditBoxCore Core)
            {
                Core.Document.Selection.ChangeCase(LetterCase.Upper);
            }

            internal static void Lowercase(RichEditBoxCore Core)
            {
                Core.Document.Selection.ChangeCase(LetterCase.Lower);
            }

            internal static void Search(RichEditBoxCore Core)
            {
                ApplicationDataContainer localSettings =
                    ApplicationData.Current.LocalSettings;
                string SelectionText = Core.Document.Selection.Text;
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
        }
    }
}
