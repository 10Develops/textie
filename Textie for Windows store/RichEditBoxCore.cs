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
namespace Textie_for_Windows_store
{
    public class RichEditBoxCore : RichEditBox
    {
        public bool ReadyToSave = false;
        public ITextSelection TextSelection;

        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        public RichEditBoxCore()
        {
            string TextBoxTheme = localSettings.Values["TextBoxTheme"].ToString();
            if (TextBoxTheme == "Light")
            {
                RequestedTheme = ElementTheme.Light;
            }
            else if (TextBoxTheme == "Dark")
            {
                RequestedTheme = ElementTheme.Dark;
            }

            IsSpellCheckEnabled = false;
            TextWrapping = TextWrapping.Wrap;

            TextChanging += OnTextChanging;
            TextChanged += OnTextChanged;

            if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 5))
            {
                PreviewKeyDown += OnPreviewKeyDown;
            }
        }

        private void OnTextChanging(RichEditBox sender, RichEditBoxTextChangingEventArgs args)
        {

        }

        private void OnTextChanged(object sender, RoutedEventArgs e)
        {
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

        string _text;
        public string Text
        {
            get
            {
                Document.GetText(TextGetOptions.None, out _text);
                return _text;
            }

            set
            {
                Document.SetText(TextSetOptions.None, value);
                _text = value;
            }
            
        }

        string _textRtf;
        public string TextRtf
        {
            get
            {
                Document.GetText(TextGetOptions.FormatRtf, out _textRtf);
                return _text;
            }

            set
            {
                Document.SetText(TextSetOptions.FormatRtf, value);
                _text = value;
            }

        }

        public int Find(string FindingText, bool MatchCase)
        {
            var textLength = Text.Length;
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
            var textLength = Text.Length;
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
    }
}