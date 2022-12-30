using System.Windows.Controls;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

using System;
using System.Runtime.InteropServices;

namespace ClassLibraryCommon
{
    public static class DarkMode
    {
        [DllImport("dwmapi.dll", PreserveSig = true)]
        public static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref bool attrValue, int attrSize);

        public static bool DarkModeEnabled { get; set; }
        private static Style GetStyle(FrameworkElement frameworkElement)
        {
            return frameworkElement switch
            {
                Button => (Style)frameworkElement.FindResource("ButtonDarkMode"),
                TextBlock => (Style)frameworkElement.FindResource("TextBlockDarkMode"),
                Label => (Style)frameworkElement.FindResource("LabelDarkMode"),
                CheckBox => (Style)frameworkElement.FindResource("CheckBoxDarkMode"),
                ComboBox => (Style)frameworkElement.FindResource("ComboBoxDarkMode"),
                ComboBoxItem => (Style)frameworkElement.FindResource("ComboBoxItemDarkMode"),
                TabControl => (Style)frameworkElement.FindResource("TabControlDarkMode"),
                TabItem => (Style)frameworkElement.FindResource("TabControlItemDarkMode"),
                TextBox => (Style)frameworkElement.FindResource("TextBoxDarkMode"),
                GroupBox => (Style)frameworkElement.FindResource("GroupBoxDarkMode"),
                ToolBar => (Style)frameworkElement.FindResource("ToolBarDarkMode"),
                StatusBar => (Style)frameworkElement.FindResource("StatusBarDarkMode"),
                Menu => (Style)frameworkElement.FindResource("MenuDarkMode"),
                MenuItem => (Style)frameworkElement.FindResource("MenuItemDarkMode"),
                RadioButton => (Style)frameworkElement.FindResource("RadioButtonDarkMode"),
                DataGrid => (Style)frameworkElement.FindResource("DataGridDarkMode"),
                DataGridColumnHeader => (Style)frameworkElement.FindResource("DataGridColumnHeaderDarkMode"),
                _ => null,
            };
        }

        public static void SetDependencyObjectDarkMode(DependencyObject dependencyObject)
        {
            if (dependencyObject is FrameworkElement)
            {
                if (dependencyObject.GetType().BaseType == typeof(Window))
                {
                    var value = true;
                    DwmSetWindowAttribute(new System.Windows.Interop.WindowInteropHelper((Window)dependencyObject).Handle, 20, ref value, System.Runtime.InteropServices.Marshal.SizeOf(value));

                    var bc = new BrushConverter();
                    ((Window)dependencyObject).Background = (System.Windows.Media.Brush)bc.ConvertFrom("#FF242424");
                }
                else
                {
                    Style style = GetStyle((FrameworkElement)dependencyObject);
                    if (style != null)
                    {
                        ((FrameworkElement)dependencyObject).Style = style;
                    }
                    
                    //Special cases for containerStyle:
                    if (dependencyObject.GetType() == typeof(ComboBox))
                    {
                        ((ComboBox)dependencyObject).ItemContainerStyle = (Style)((FrameworkElement)dependencyObject).FindResource("ComboBoxItemDarkMode");
                    }
                    if (dependencyObject.GetType() == typeof(TabControl))
                    {
                        ((TabControl)dependencyObject).ItemContainerStyle = (Style)((FrameworkElement)dependencyObject).FindResource("TabControlItemDarkMode");
                    }
                }
                 ((FrameworkElement)dependencyObject).UpdateLayout();
            }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(dependencyObject); i++)
            {
                var child = VisualTreeHelper.GetChild(dependencyObject, i);
                SetDependencyObjectDarkMode(child);
            }
        }

        public static void SetFrameworkElemenDarkMode(FrameworkElement frameworkElement)
        {
            if (DarkModeEnabled)
            {
                SetDependencyObjectDarkMode(frameworkElement);
            }
        }
    }
}
