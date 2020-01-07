using System.ComponentModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Textie
{
    public class RichEditBoxPivotItem : PivotItem
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private RichEditBoxCore _editBox;
        private Item _listViewItem;

        public RichEditBoxPivotItem() : this(new Item(), new RichEditBoxCore())
        {

        }

        public RichEditBoxPivotItem(Item listViewItem, RichEditBoxCore editBox)
        {
            listViewItem.PivotItem = this;
            _listViewItem = listViewItem;

            Margin = new Thickness(0, 0, 0, 0);
            Content = editBox;
            _editBox = editBox;

            Padding = new Thickness(0, 0, 0, 0);
        }

        public Item ListViewItem
        {
            get
            {
                return _listViewItem;
            }
        }

        public RichEditBoxCore EditBox
        {
            get
            {
                return _editBox;
            }
        }

        public new object Header
        {
            get
            {
                return base.Header;
            }
            set
            {
                base.Header = value;
                OnPropertyChanged("Header");
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
