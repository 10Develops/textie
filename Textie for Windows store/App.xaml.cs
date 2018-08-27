using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;
using SDKTemplate.Common;
using System.Collections.Generic;

namespace Textie_for_Windows_store
{
    /// <summary>
    /// Обеспечивает зависящее от конкретного приложения поведение, дополняющее класс Application по умолчанию.
    /// </summary>
    public sealed partial class App : Application
    {
        /// <summary>
        /// Инициализирует одноэлементный объект приложения.  Это первая выполняемая строка разрабатываемого
        /// кода; поэтому она является логическим эквивалентом main() или WinMain().
        /// </summary>
        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

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

            SuspensionManager.KnownTypes.AddRange(new[] { typeof(RichEditBoxPivotItem), typeof(RichEditBoxCore) });

            this.Suspending += OnSuspending;
        }

        /// <summary>
        /// Вызывается при обычном запуске приложения пользователем.  Будут использоваться другие точки входа,
        /// например, если приложение запускается для открытия конкретного файла.
        /// </summary>
        /// <param name="e">Сведения о запросе и обработке запуска.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            // Do not repeat app initialization when the Window already has content, 
            // just ensure that the window is active 
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
            rootFrame.Navigate(typeof(MainPage), e.Arguments);
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

        protected override void OnActivated(IActivatedEventArgs args)
        {
            if (args.Kind == ActivationKind.Protocol)
            {
                ProtocolActivatedEventArgs eventArgs = args as ProtocolActivatedEventArgs;
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

            if(args.Kind == ActivationKind.ToastNotification)
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

        /// <summary>
        /// Вызывается в случае сбоя навигации на определенную страницу
        /// </summary>
        /// <param name="sender">Фрейм, для которого произошел сбой навигации</param>
        /// <param name="e">Сведения о сбое навигации</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Вызывается при приостановке выполнения приложения.  Состояние приложения сохраняется
        /// без учета информации о том, будет ли оно завершено или возобновлено с неизменным
        /// содержимым памяти.
        /// </summary>
        /// <param name="sender">Источник запроса приостановки.</param>
        /// <param name="e">Сведения о запросе приостановки.</param>
        private async void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Сохранить состояние приложения и остановить все фоновые операции
            await SuspensionManager.SaveAsync();
            deferral.Complete();
        }
    }
}
