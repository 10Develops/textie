using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;
using Windows.UI.StartScreen;
using System.Threading.Tasks;
using Windows.Foundation.Metadata;
using Windows.UI.Core;

namespace Textie
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public sealed partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        /// 
        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        /// <summary>

        /// Navigation service, provides a decoupled way to trigger the UI Frame

        /// to transition between views.

        /// </summary>

        public App()
        {
            if (!localSettings.Values.ContainsKey("theme"))
            {
                localSettings.Values.Add("theme", "WD");
            }
            else
            {
                string theme = localSettings.Values["theme"].ToString();
                if (theme != "WD")
                {
                    if (theme == "Dark")
                    {
                        RequestedTheme = ApplicationTheme.Dark;
                    }
                    else if (theme == "Light")
                    {
                        RequestedTheme = ApplicationTheme.Light;
                    }
                }
            }

            if (!localSettings.Values.ContainsKey("transparency"))
            {
                localSettings.Values.Add("transparency", "1");
            }

            if (!localSettings.Values.ContainsKey("TextBoxTheme"))
            {
                localSettings.Values.Add("TextBoxTheme", "Light");
            }

            if (!localSettings.Values.ContainsKey("titleBarColor"))
            {
                localSettings.Values.Add("titleBarColor", "0");
            }

            if (!localSettings.Values.ContainsKey("TabBarPosition"))
            {
                localSettings.Values.Add("TabBarPosition", "0");
            }

            if (!localSettings.Values.ContainsKey("FontFamily"))
            {
                localSettings.Values.Add("FontFamily", "Segoe UI");
            }

            if (!localSettings.Values.ContainsKey("SearchEngine"))
            {
                localSettings.Values.Add("SearchEngine", "Bing");
            }

            if (!localSettings.Values.ContainsKey("vibrate"))
            {
                localSettings.Values.Add("vibrate", "1");
            }

            if (!localSettings.Values.ContainsKey("autoSave"))
            {
                localSettings.Values.Add("autoSave", "0");
            }

            if (!localSettings.Values.ContainsKey("trimNewLines"))
            {
                localSettings.Values.Add("trimNewLines", "0");
            }

            if (!localSettings.Values.ContainsKey("hotExit"))
            {
                localSettings.Values.Add("hotExit", "0");
            }

            if (!localSettings.Values.ContainsKey("flagsEnabled"))
            {
                localSettings.Values.Add("flagsEnabled", "0");
            }

            if (!localSettings.Values.ContainsKey("highContrast"))
            {
                localSettings.Values.Add("highContrast", "0");
            }

            this.Suspending += OnSuspending;
            this.Resuming += OnResuming;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                CoreDispatcherPriority.Normal,
                async () =>
                {
                    Frame rootFrame = Window.Current.Content as Frame;
                    // Do not repeat app initialization when the Window already has content, 
                    // just ensure that the window is active 

                    // CoreApplication.EnablePrelaunch was introduced in Windows 10 version 1607
                    bool canEnablePrelaunch = ApiInformation.IsMethodPresent("Windows.ApplicationModel.Core.CoreApplication", "EnablePrelaunch");

                    if (rootFrame == null)
                    {
                        // Create a Frame to act as the navigation context and navigate to the first page 
                        rootFrame = new Frame();

                        // Associate the frame with a SuspensionManager key 
                        if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                        {
                            // Restore the saved session state only when appropriate 
                        }

                        // Place the frame in the current Window 
                        Window.Current.Content = rootFrame;
                    }

                    if (e.PrelaunchActivated == false)
                    {
                        // On Windows 10 version 1607 or later, this code signals that this app wants to participate in prelaunch
                        if (canEnablePrelaunch)
                        {
                            TryEnablePrelaunch();
                        }

                        // TODO: This is not a prelaunch activation. Perform operations which
                        // assume that the user explicitly launched the app such as updating
                        // the online presence of the user on a social network, updating a
                        // what's new feed, etc.

                        // When the navigation stack isn't restored navigate to the first page,
                        // configuring the new page by passing required information as a navigation
                        // parameter
                        //MainPage is always in rootFrame so we don't have to worry about restoring the navigation state on resume
                        rootFrame.Navigate(typeof(MainPage), e.Arguments);
                    }

                    if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 2))
                    {
                        await ConfigureJumpList();
                    }

                    // Ensure the current window is active
                    Window.Current.Activate();
                });
        }

        protected override async void OnActivated(IActivatedEventArgs args)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                CoreDispatcherPriority.Normal,
                () =>
                {
                    Frame rootFrame = Window.Current.Content as Frame;
                    // Do not repeat app initialization when the Window already has content, 
                    // just ensure that the window is active 
                    if (rootFrame == null)
                    {
                        // Create a Frame to act as the navigation context and navigate to the first page 
                        rootFrame = new Frame();
                        // Associate the frame with a SuspensionManager key 
                        if (args.PreviousExecutionState == ApplicationExecutionState.Terminated)
                        {
                            // Restore the saved session state only when appropriate 
                        }

                        // Place the frame in the current Window 
                        Window.Current.Content = rootFrame;
                    }

                    rootFrame.Navigate(typeof(MainPage), args);

                    // Ensure the current window is active 
                    Window.Current.Activate();
                });
        }

        protected override void OnFileActivated(FileActivatedEventArgs args)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            // Do not repeat app initialization when the Window already has content, 
            // just ensure that the window is active 
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page 
                rootFrame = new Frame();
                // Associate the frame with a SuspensionManager key 
                if (args.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    // Restore the saved session state only when appropriate 
                }

                // Place the frame in the current Window 
                Window.Current.Content = rootFrame;
            }

            rootFrame.Navigate(typeof(MainPage), args);
            if (rootFrame.Content == null)
            {
                if (!rootFrame.Navigate(typeof(MainPage)))
                {
                    throw new Exception("Failed to create initial page");
                }
            }

            // Ensure the current window is active 
            Window.Current.Activate();
        }

        protected override void OnShareTargetActivated(ShareTargetActivatedEventArgs args)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            // Do not repeat app initialization when the Window already has content, 
            // just ensure that the window is active 
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page 
                rootFrame = new Frame();
                // Associate the frame with a SuspensionManager key 
                if (args.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    // Restore the saved session state only when appropriate 
                }

                // Place the frame in the current Window 
                Window.Current.Content = rootFrame;
            }

            rootFrame.Navigate(typeof(MainPage), args);
            if (rootFrame.Content == null)
            {
                if (!rootFrame.Navigate(typeof(MainPage)))
                {
                    throw new Exception("Failed to create initial page");
                }
            }

            // Ensure the current window is active 
            Window.Current.Activate();
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        private void OnResuming(object sender, object e)
        {
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Сохранить состояние приложения и остановить все фоновые операции
            deferral.Complete();
        }

        /// <summary>
        /// Encapsulates the call to CoreApplication.EnablePrelaunch() so that the JIT
        /// won't encounter that call (and prevent the app from running when it doesn't
        /// find it), unless this method gets called. This method should only
        /// be called when the caller determines that we are running on a system that
        /// supports CoreApplication.EnablePrelaunch().
        /// </summary>
        private void TryEnablePrelaunch()
        {
            Windows.ApplicationModel.Core.CoreApplication.EnablePrelaunch(true);
        }

        private async Task ConfigureJumpList()
        {
            if (JumpList.IsSupported() == true)
            {
                JumpList jumpList = await JumpList.LoadCurrentAsync();
                jumpList.SystemGroupKind = JumpListSystemGroupKind.Recent;
                if (jumpList.Items.Count == 0)
                {
                    JumpListItem OpenJumpListItem = JumpListItem.CreateWithArguments("OpenFile", "Open File");
                    OpenJumpListItem.Logo = new Uri("ms-appx:///Assets/JumpList/OpenFile.png");
                    jumpList.Items.Add(OpenJumpListItem);

                    try
                    {
                        await jumpList.SaveAsync();
                    }
                    catch (Exception)
                    {

                    }
                }
            }
        }
    }
}
