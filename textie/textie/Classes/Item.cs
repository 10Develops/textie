using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.Storage;
using System.ComponentModel;

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
        public string CopiedFileName { get; set; }
        public bool IsCachedFile { get; set; }

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
            ContentStackPanel.Children.Add(new TextBlock() { Text = FilePath, FontSize = 12 });
            ContentStackPanel.Children.Add(new TextBlock() { Text = "File type: " + FileType });
            ContentStackPanel.Children.Add(new TextBlock() { Text = "File size: " + basicProperties.Size + " bytes" });
            ContentStackPanel.Children.Add(new TextBlock() { Text = "Date created: " + File.DateCreated.DateTime });
            ContentStackPanel.Children.Add(new TextBlock() { Text = "Date modified: " + basicProperties.DateModified.DateTime });

            FileInfoDialog.Content = ContentStackPanel;

            await FileInfoDialog.ShowAsync();
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
