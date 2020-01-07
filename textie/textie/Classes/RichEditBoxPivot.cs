using Windows.UI.Xaml.Controls;

namespace Textie
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
