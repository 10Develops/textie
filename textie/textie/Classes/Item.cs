using System;
using Windows.UI.Xaml.Controls;
using Windows.Storage;
using System.ComponentModel;
using Windows.UI.Xaml;
using Windows.System;
using System.IO;

namespace Textie
{
    public class Item : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _title;
        public string Title
        {
            get
            {
                if(_title.Length >= 10)
                {
                    return string.Format("{0}...", _title.Substring(0, 10));
                }
                else
                {
                    return _title;
                }
            }
            set
            {
                _title = value;
                OnPropertyChanged("Title");
            }
        }

        public RichEditBoxPivotItem PivotItem { get; set; }
        public StorageFile File { get; set; }
        public StorageFile CopiedFile { get; set; }

        bool _isCachedFile = false;
        public bool IsCachedFile
        {
            get
            {
                if(File == null && CopiedFile == null)
                {
                    _isCachedFile = false;
                }
                else if(CopiedFile != null)
                {
                    _isCachedFile = true;
                }

                return _isCachedFile;
            }
        }

        public string CopiedFileName
        {
            get
            {
                if(File == null)
                {
                    if(IsCachedFile == true)
                    {
                        return CopiedFile.Name;
                    }
                    else
                    {
                        return PivotItem.Header.ToString();
                    }
                }
                else
                {
                    return null;
                }
            }
        }

        public string FileName
        {
            get
            {
                return File.Name;
            }
        }

        public string FileType
        {
            get
            {
                return File.FileType;
            }
        }

        public string FilePath
        {
            get
            {
                return File.Path;
            }
        }

        public async void ShowFileInfoDialog()
        {
            Windows.Storage.FileProperties.BasicProperties basicProperties =
                await File.GetBasicPropertiesAsync();

            ContentDialog FileInfoDialog = new ContentDialog()
            {
                PrimaryButtonText = "OK"
            };

            FileInfoDialog.Title = "File Information";

            StackPanel ContentStackPanel = new StackPanel();
            ContentStackPanel.Children.Add(new TextBlock() { Text = FileName, FontSize = 17 });

            RelativePanel FileStackPanel = new RelativePanel();

            TextBlock filePathTextBlock = new TextBlock() { Text = FilePath, FontSize = 12, IsTextSelectionEnabled = true, TextWrapping = TextWrapping.NoWrap, Padding = new Thickness(4) };
            ScrollViewer filePathScrollViewer = new ScrollViewer() { Content = filePathTextBlock, HorizontalScrollBarVisibility = ScrollBarVisibility.Auto, MaxWidth = 320, Margin = new Thickness(0, 0, 90, 0) };
            Button openFolderButton = new Button() { Content = "Open folder", FontSize = 12 };
            FileStackPanel.Children.Add(filePathScrollViewer);
            FileStackPanel.Children.Add(openFolderButton);

            ContentStackPanel.Children.Add(FileStackPanel);

            openFolderButton.Click += OpenFolderButton_Click;

            ContentStackPanel.Children.Add(new TextBlock() { Text = "File type: " + FileType });
            ContentStackPanel.Children.Add(new TextBlock() { Text = "File size: " + basicProperties.Size + " bytes" });
            ContentStackPanel.Children.Add(new TextBlock() { Text = "Date created: " + File.DateCreated.DateTime });
            ContentStackPanel.Children.Add(new TextBlock() { Text = "Date modified: " + basicProperties.DateModified.DateTime });

            FileInfoDialog.Content = ContentStackPanel;

            RelativePanel.SetAlignLeftWithPanel(filePathScrollViewer, true);
            RelativePanel.SetAlignRightWithPanel(openFolderButton, true);

            await FileInfoDialog.ShowAsync();
        }

        private async void OpenFolderButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(Path.GetDirectoryName(FilePath));
                await Launcher.LaunchFolderAsync(folder);
            }
            catch (UnauthorizedAccessException)
            {

            }
        }

        // Create the OnPropertyChanged method to raise the event
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
