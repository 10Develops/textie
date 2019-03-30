using System;
using Windows.Foundation;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.System;
using Windows.Storage;
using System.Text;
using Windows.Foundation.Metadata;
using Windows.UI.Text;
using Windows.UI;
using System.Diagnostics;
using Windows.UI.Xaml.Controls.Primitives;

namespace Textie
{
    public class RichEditBoxCore : RichEditBox
    {
        public ITextSelection TextSelection;

        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        RichEditBoxCoreText coreText;

        private string CRLF = "\r\n";
        private string LF = "\n";
        private string CR = "\r";

        public RichEditBoxCore()
        {
            coreText = new RichEditBoxCoreText(this);

            string TextBoxTheme = localSettings.Values["TextBoxTheme"].ToString();
            string highContrast = localSettings.Values["highContrast"].ToString();

            if (highContrast == "1")
            {
                RequestedTheme = ElementTheme.Dark;
            }
            else
            {
                if (TextBoxTheme == "Light")
                {
                    RequestedTheme = ElementTheme.Light;
                }
                else if (TextBoxTheme == "Dark")
                {
                    RequestedTheme = ElementTheme.Dark;
                }
            }

            string FontType = localSettings.Values["FontFamily"].ToString();
            FontFamily = new FontFamily(FontType);

            if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 3))
            {
                ContextFlyout = null;
            }

            if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 7))
            {
                SelectionFlyout = null;
            }

            ReadyToSave = false;

            FontSize = 15;
            TextWrapping = TextWrapping.Wrap;

            TextChanging += OnTextChanging;
            TextChanged += OnTextChanged;
            SelectionChanged += OnSelectionChanged;

            if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 5))
            {
                PreviewKeyDown += OnPreviewKeyDown;
            }

            ToolTipService.SetToolTip(this, string.Format("Length: {0}", CoreText.Text.Length));
        }

        private void OnTextChanging(RichEditBox sender, RichEditBoxTextChangingEventArgs args)
        {
        }

        private void OnTextChanged(object sender, RoutedEventArgs e)
        {
            ToolTipService.SetToolTip(this, string.Format("Length: {0}", CoreText.Text.Length));
        }

        private void OnSelectionChanged(object sender, RoutedEventArgs e)
        {
            string SelectionText = Document.Selection.Text;
            if (SelectionText != string.Empty)
            {
                ToolTipService.SetToolTip(this, string.Format("Length: {0} Selection length: {1}", CoreText.Text.Length, SelectionText.Length));
            }
            else
            {
                ToolTipService.SetToolTip(this, string.Format("Length: {0}", CoreText.Text.Length));
            }
        }

        private void OnPreviewKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Tab)
            {
                if (this != null)
                {
                    Document.Selection.TypeText("\t");
                    e.Handled = true;
                }

            }
        }

        private bool _readyToSave = false;
        public bool ReadyToSave
        {
            get
            {
                return _readyToSave;
            }
            set
            {
                _readyToSave = value;
            }
        }

        public RichEditBoxCoreText CoreText
        {
            get
            {
                return coreText;
            }
        }

        NewLine _EOL;
        public NewLine EOL
        {
            get
            {
                if (CoreText.Text.Contains(CRLF))
                {
                    _EOL = NewLine.CRLF;
                }
                else if (CoreText.Text.Contains(LF))
                {
                    _EOL = NewLine.LF;
                }
                else if (CoreText.Text.Contains(CR))
                {
                    _EOL = NewLine.CR;
                }

                return _EOL;
            }
        }

        public void ConvertNewLine(NewLine lineOption)
        {
            switch (lineOption)
            {
                case (NewLine.LF):
                    CoreText.Text = CoreText.Text.Replace(CRLF, @LF);
                    CoreText.Text = CoreText.Text.Replace(CR, @LF);
                    break;
                case (NewLine.CRLF):
                    CoreText.Text = CoreText.Text.Replace(LF, @CRLF);
                    CoreText.Text = CoreText.Text.Replace(CR, @CRLF);
                    break;
                case (NewLine.CR):
                    CoreText.Text = CoreText.Text.Replace(CRLF, @CR);
                    CoreText.Text = CoreText.Text.Replace(LF, @CR);
                    break;
            }
        }

        public int Find(string FindingText, bool MatchCase)
        {
            var textLength = CoreText.Text.Length;
            Document.Selection.SetRange(0, textLength);
            Document.Selection.CharacterFormat.BackgroundColor = Colors.White;

            int i = 1;
            while (i > 0)
            {
                Focus(FocusState.Programmatic);

                if (MatchCase == true)
                {
                    i = Document.Selection.FindText(FindingText,
                        textLength, FindOptions.Case);
                }
                else
                {
                    i = Document.Selection.FindText(FindingText,
                        textLength, FindOptions.None);
                }

                TextSelection = Document.Selection;
                if (TextSelection != null && i != 0)
                {
                    TextSelection.CharacterFormat.BackgroundColor = Colors.Yellow;
                }
                else
                {
                    Document.Selection.SetRange(0, 0);
                }
            }

            return i;
        }

        public int Replace(string WhatText, string WithText, bool MatchCase)
        {
            var textLength = CoreText.Text.Length;
            Document.Selection.SetRange(0, textLength);
            Document.Selection.CharacterFormat.BackgroundColor = Colors.White;
            int i = 1;
            while (i > 0)
            {
                Focus(FocusState.Programmatic);

                if (MatchCase == true)
                {
                    i = Document.Selection.FindText(WhatText,
                        textLength, FindOptions.Case);
                }
                else
                {
                    i = Document.Selection.FindText(WhatText,
                        textLength, FindOptions.None);
                }

                TextSelection = Document.Selection;
                if (TextSelection != null && i != 0)
                {
                    TextSelection.SetText(TextSetOptions.None, WithText);
                    TextSelection.CharacterFormat.BackgroundColor = Colors.Yellow;
                }
                else
                {
                    Document.Selection.SetRange(0, 0);
                }
            }

            return i;
        }

        public void Trim(TrimingOptions option)
        {
            Document.Selection.SetRange(0, CoreText.Text.Length);
            if (option == TrimingOptions.Trim)
            {
                Document.Selection.Text = Document.Selection.Text.Trim();
            }
            else if (option == TrimingOptions.TrimStart)
            {
                Document.Selection.Text = Document.Selection.Text.TrimStart();
            }
            else if (option == TrimingOptions.TrimEnd)
            {
                Document.Selection.Text = Document.Selection.Text.TrimEnd();
            }
        }
    }

    public enum TrimingOptions
    {
        Trim = 0,
        TrimStart = 1,
        TrimEnd = 2
    }

    public enum NewLine
    {
        CRLF = 0,
        LF = 1,
        CR = 2
    }
}