using System.Windows.Controls;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using Theraot.Collections;

namespace ClassLibraryCommon
{
    public static class DarkMode
    {
        private static bool darkModeEnabled;

        [DllImport("dwmapi.dll", PreserveSig = true)]
        public static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref bool attrValue, int attrSize);

        public static bool DarkModeEnabled { 
            get => darkModeEnabled;
            set {
                darkModeEnabled = value;
                if (darkModeEnabled)
                {
                    SetDarkMode();
                }
                else
                {
                    ClearDarkMode();
                }
            }
        }


        private static void SetDependencyObjectDarkMode(DependencyObject dependencyObject)
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
            }
        }

        public static void SetFrameworkElemenDarkMode(FrameworkElement frameworkElement)
        {
            if (DarkModeEnabled)
            {
                SetDependencyObjectDarkMode(frameworkElement);
            }
        }

        public static void SetDarkMode()
        {
            System.Windows.Application.Current.Resources.MergedDictionaries.Clear();
            List<ResourceDictionary> darKmodeResources = new()
            {
                new() { Source = new Uri("pack://application:,,,/DarkMode/ComboBox.xaml") },
                new() { Source = new Uri("pack://application:,,,/DarkMode/ComboBoxItem.xaml") },
                new() { Source = new Uri("pack://application:,,,/DarkMode/CheckBox.xaml") },
                new() { Source = new Uri("pack://application:,,,/DarkMode/Window.xaml") },
                new() { Source = new Uri("pack://application:,,,/DarkMode/Label.xaml") },
                new() { Source = new Uri("pack://application:,,,/DarkMode/Button.xaml") },
                new() { Source = new Uri("pack://application:,,,/DarkMode/TabControl.xaml") },
                new() { Source = new Uri("pack://application:,,,/DarkMode/TabControlItem.xaml") },
                new() { Source = new Uri("pack://application:,,,/DarkMode/TextBox.xaml") },
                new() { Source = new Uri("pack://application:,,,/DarkMode/GroupBox.xaml") },
                new() { Source = new Uri("pack://application:,,,/DarkMode/ToolBar.xaml") },
                new() { Source = new Uri("pack://application:,,,/DarkMode/StatusBar.xaml") },
                new() { Source = new Uri("pack://application:,,,/DarkMode/RadioButton.xaml") },
                new() { Source = new Uri("pack://application:,,,/DarkMode/DataGrid.xaml") },
                new() { Source = new Uri("pack://application:,,,/DarkMode/DataGridColumnHeader.xaml") },
                new() { Source = new Uri("pack://application:,,,/DarkMode/DataGridCell.xaml") },
                new() { Source = new Uri("pack://application:,,,/DarkMode/ContextMenu.xaml") },
                new() { Source = new Uri("pack://application:,,,/DarkMode/Menu.xaml") },
                new() { Source = new Uri("pack://application:,,,/DarkMode/MenuItem.xaml") },
            };
            System.Windows.Application.Current.Resources.MergedDictionaries.AddRange(darKmodeResources);
        }

        public static void ClearDarkMode()
        {
            System.Windows.Application.Current.Resources.MergedDictionaries.Clear();
        }

        public static SolidColorBrush TextBoxSelectedBackgroundColor
        {
            get { return darkModeEnabled ? Brushes.IndianRed : Brushes.Yellow; }
        }
        public static SolidColorBrush TextBoxUnselectedBackgroundColor
        {
            get { return darkModeEnabled ? new SolidColorBrush(  Color.FromArgb(255, 56, 56, 56)) : Brushes.White; } //"#FF383838"
        }
    }
}
