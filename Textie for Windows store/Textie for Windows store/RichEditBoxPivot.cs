using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.IO;
using Windows.Storage;
using Windows.Storage.Streams;
namespace Textie_for_Windows_store
{
    public class RichEditBoxPivot : Pivot
    {
        public RichEditBoxPivotItem SelectedRichEditBoxItem
        {
            get
            {
                return (RichEditBoxPivotItem)SelectedItem;
            }
        }

        public RichEditBoxCore SelectedRichEditBox
        {
            get
            {
                return SelectedRichEditBoxItem.EditBox;
            }
        }

        public RichEditBoxPivotItem AddTab()
        {
            RichEditBoxPivotItem item = new RichEditBoxPivotItem();
            Items.Add(item);
            SelectedItem = item;
            return item;
        }

        public bool CloseTab(RichEditBoxPivotItem tab)
        {
            bool canRemove = Items.Count > 1;

            if (canRemove)
            {
                Items.Remove(tab);
            }

            return canRemove;
        }

        public void CloseCurrentTab()
        {
            RichEditBoxPivotItem item = SelectedRichEditBoxItem;
            CloseTab(item);
        }
    }
}
