using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Windows.UI.Xaml.Controls;
using Windows.UI.Text;
using Windows.Storage;

namespace Textie
{
    public class RichEditBoxCoreText
    {
        RichEditBoxCore _core;

        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
    
        public RichEditBoxCoreText(RichEditBoxCore core)
        {
            _core = core;
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

        public void SelectAll()
        {
            _core.Document.Selection.SetRange(0, Text.Length);
        }
    }
}
