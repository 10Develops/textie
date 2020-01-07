using Windows.UI.Text;
using Windows.Storage;
using Windows.ApplicationModel.DataTransfer;
using System;

namespace Textie
{
    public class RichEditBoxCoreText
    {
        RichEditBoxCore _core;

        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        public ICoreTextSelection Selection;

        public RichEditBoxCoreText(RichEditBoxCore core)
        {
            _core = core;
            Selection = new ICoreTextSelection(_core);
        }

        string _text;
        public string Text
        {
            get
            {
                _core.Document.GetText(TextGetOptions.None, out _text);

                string trimNewLines = localSettings.Values["trimNewLines"].ToString();

                if (trimNewLines == "1")
                {
                    _text = _text.TrimStart('\r');
                }
                else if (trimNewLines == "2")
                {
                    _text = _text.TrimEnd('\r');
                }
                else if (trimNewLines == "3")
                {
                    _text = _text.Trim('\r');
                }

                return _text;
            }

            set
            {
                _core.Document.SetText(TextSetOptions.None, value);

                string trimNewLines = localSettings.Values["trimNewLines"].ToString();

                if (trimNewLines == "0")
                {
                    _text = value;
                }
                else if (trimNewLines == "1")
                {
                    _text = value.TrimStart('\r');
                }
                else if (trimNewLines == "2")
                {
                    _text = value.TrimEnd('\r');
                }
                else if (trimNewLines == "3")
                {
                    _text = value.Trim('\r');
                }
            }
        }

        string _textRtf;
        public string TextRtf
        {
            get
            {
                _core.Document.GetText(TextGetOptions.FormatRtf, out _textRtf);

                string trimNewLines = localSettings.Values["trimNewLines"].ToString();

                if (trimNewLines == "1")
                {
                    _textRtf = _textRtf.TrimStart('\r');
                }
                else if (trimNewLines == "2")
                {
                    _textRtf = _textRtf.TrimEnd('\r');
                }
                else if (trimNewLines == "3")
                {
                    _textRtf = _textRtf.Trim('\r');
                }

                return _textRtf;
            }

            set
            {
                _core.Document.SetText(TextSetOptions.FormatRtf, value);

                string trimNewLines = localSettings.Values["trimNewLines"].ToString();

                if (trimNewLines == "0")
                {
                    _textRtf = value;
                }
                else if (trimNewLines == "1")
                {
                    _textRtf = value.TrimStart('\r');
                }
                else if (trimNewLines == "2")
                {
                    _textRtf = value.TrimEnd('\r');
                }
                else if (trimNewLines == "3")
                {
                    _textRtf = value.Trim('\r');
                }
            }
        }

        public void Cut()
        {
            Copy();
            _core.Document.Selection.Text = string.Empty;
        }

        public void Copy()
        {
            var dataPackage = new DataPackage();
            dataPackage.SetText(_core.Document.Selection.Text);
            Clipboard.SetContent(dataPackage);
        }

        public async void Paste()
        {
            DataPackageView dataPackageView = Clipboard.GetContent();
            if (dataPackageView.Contains(StandardDataFormats.Text))
            {
                string text = await dataPackageView.GetTextAsync();
                // To output the text from this example, you need a TextBlock control
                _core.Document.Selection.SetText(TextSetOptions.None, text);
                _core.Document.Selection.SetRange(_core.Document.Selection.EndPosition, _core.Document.Selection.EndPosition);
            }
        }

        public void SelectAll()
        {
            _core.Document.Selection.SetRange(0, Text.Length);
        }
    }

    public class ICoreTextSelection
    {
        RichEditBoxCore _core;

        public ICoreTextSelection(RichEditBoxCore core)
        {
            _core = core;
        }

        string _text;
        public string Text
        {
            get
            {
                _core.Document.Selection.GetText(TextGetOptions.None, out _text);

                return _text;
            }
        }

        public bool IsNumbersOnly()
        {
            foreach (char c in _core.Document.Selection.Text)
            {
                if (!char.IsDigit(c))
                    return false;
            }
            return true;
        }
    }
}
