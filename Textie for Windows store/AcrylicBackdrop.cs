using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XamlBrewer.Uwp.Controls;
using Windows.UI.ViewManagement;
using Windows.UI;

namespace Textie_for_Windows_store
{
    public class AcrylicBackdrop : BackDrop
    {
        public AcrylicBackdrop()
        {
            BlurAmount = 35;
            TintAlpha = 200;

            //Changes color to SystemChromeMediumColor for Light and Dark theme
            var DefaultTheme = new UISettings();
            var uiTheme = DefaultTheme.GetColorValue(UIColorType.Background).ToString();
            if (uiTheme == "#FF000000")
            {
                TintColor = Color.FromArgb(255, 31, 31, 31);
            }
            else if (uiTheme == "#FFFFFFFF")
            {
                TintColor = Color.FromArgb(255, 229, 229, 229);
            }
        }
    }
}
