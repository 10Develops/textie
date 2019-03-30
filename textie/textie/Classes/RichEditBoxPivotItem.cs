using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Textie
{
    public class RichEditBoxPivotItem : PivotItem
    {
        private TextBlock _headerTextBlock;
        private RichEditBoxCore _editBox;
        private Item _listViewItem;

        public RichEditBoxPivotItem() : this(new TextBlock(), new Item(), new RichEditBoxCore())
        {

        }

        public RichEditBoxPivotItem(TextBlock headerTextBlock, Item listViewItem, RichEditBoxCore editBox)
        {
            headerTextBlock.FontSize = 18;
            Header = headerTextBlock;
            _headerTextBlock = headerTextBlock;

            headerTextBlock.CanDrag = true;

            listViewItem.PivotItem = this;
            _listViewItem = listViewItem;

            Margin = new Thickness(0, 0, 0, 0);
            Content = editBox;
            _editBox = editBox;

            Padding = new Thickness(0, 0, 0, 0);
        }

        public TextBlock HeaderTextBlock
        {
            get
            {
                return _headerTextBlock;
            }
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
    }
}
