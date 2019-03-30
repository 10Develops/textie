using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.System;
using Windows.UI.Popups;

namespace Textie
{
    public class WebResources
    {
        public static async void Launch(StorageFile htmlFile)
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

        public static async void Navigate(string Url)
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
    }
}
