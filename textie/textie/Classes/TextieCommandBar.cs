using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Textie
{
    public class TextieCommandBar : CommandBar
    {
        public TextieCommandBar()
        {
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var layoutRoot = GetTemplateChild("LayoutRoot") as Grid;
            if (layoutRoot != null)
            {
                VisualStateManager.SetCustomVisualStateManager(layoutRoot, new TextieCommandBarVisualStateManager());
            }
        }
    }

    public class TextieCommandBarVisualStateManager : VisualStateManager
    {
        protected override bool GoToStateCore(Control control, FrameworkElement templateRoot, string stateName, VisualStateGroup group, VisualState state, bool useTransitions)
        {
            //replace OpenUp state change with OpenDown one and continue as normal
            if (!string.IsNullOrWhiteSpace(stateName) && stateName.EndsWith("OpenUp"))
            {
                stateName = stateName.Substring(0, stateName.Length - 6) + "OpenDown";
            }

            return base.GoToStateCore(control, templateRoot, stateName, group, state, useTransitions);
         }   
    }
}
