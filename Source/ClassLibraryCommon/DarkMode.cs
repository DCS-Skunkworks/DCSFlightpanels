using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using Theraot.Collections;

namespace ClassLibraryCommon
{
    public static class DarkMode
    {
        private static bool _darkModeEnabled;

        [DllImport("dwmapi.dll", PreserveSig = true)]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref bool attrValue, int attrSize);

        public static bool DarkModeEnabled { 
            get => _darkModeEnabled;
            set {
                _darkModeEnabled = value;
                if (_darkModeEnabled)
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
                    DwmSetWindowAttribute(new System.Windows.Interop.WindowInteropHelper((Window)dependencyObject).Handle, 20, ref value, Marshal.SizeOf(value));

                    var bc = new BrushConverter();
                    ((Window)dependencyObject).Background = (Brush)bc.ConvertFrom("#FF242424");
                }
            }
        }

        public static void SetFrameworkElementDarkMode(FrameworkElement frameworkElement)
        {
            if (DarkModeEnabled)
            {
                SetDependencyObjectDarkMode(frameworkElement);
            }
        }

        public static void SetDarkMode()
        {
            Application.Current.Resources.MergedDictionaries.Clear();
            List<ResourceDictionary> darkModeResources = new()
            {
                new() { Source = new Uri("pack://application:,,,/ClassLibraryCommon;component/DarkMode/ComboBox.xaml") },
                new() { Source = new Uri("pack://application:,,,/ClassLibraryCommon;component/DarkMode/ComboBoxItem.xaml") },
                new() { Source = new Uri("pack://application:,,,/ClassLibraryCommon;component/DarkMode/CheckBox.xaml") },
                new() { Source = new Uri("pack://application:,,,/ClassLibraryCommon;component/DarkMode/Window.xaml") },
                new() { Source = new Uri("pack://application:,,,/ClassLibraryCommon;component/DarkMode/Label.xaml") },
                new() { Source = new Uri("pack://application:,,,/ClassLibraryCommon;component/DarkMode/Button.xaml") },
                new() { Source = new Uri("pack://application:,,,/ClassLibraryCommon;component/DarkMode/TabControl.xaml") },
                new() { Source = new Uri("pack://application:,,,/ClassLibraryCommon;component/DarkMode/TabControlItem.xaml") },
                new() { Source = new Uri("pack://application:,,,/ClassLibraryCommon;component/DarkMode/TextBox.xaml") },
                new() { Source = new Uri("pack://application:,,,/ClassLibraryCommon;component/DarkMode/GroupBox.xaml") },
                new() { Source = new Uri("pack://application:,,,/ClassLibraryCommon;component/DarkMode/ToolBar.xaml") },
                new() { Source = new Uri("pack://application:,,,/ClassLibraryCommon;component/DarkMode/StatusBar.xaml") },
                new() { Source = new Uri("pack://application:,,,/ClassLibraryCommon;component/DarkMode/RadioButton.xaml") },
                new() { Source = new Uri("pack://application:,,,/ClassLibraryCommon;component/DarkMode/DataGrid.xaml") },
                new() { Source = new Uri("pack://application:,,,/ClassLibraryCommon;component/DarkMode/DataGridColumnHeader.xaml") },
                new() { Source = new Uri("pack://application:,,,/ClassLibraryCommon;component/DarkMode/DataGridCell.xaml") },
                new() { Source = new Uri("pack://application:,,,/ClassLibraryCommon;component/DarkMode/ContextMenu.xaml") },
                new() { Source = new Uri("pack://application:,,,/ClassLibraryCommon;component/DarkMode/Menu.xaml") },
                new() { Source = new Uri("pack://application:,,,/ClassLibraryCommon;component/DarkMode/MenuItem.xaml") },
                new() { Source = new Uri("pack://application:,,,/ClassLibraryCommon;component/DarkMode/ScrollBar.xaml") },
            };
            Application.Current.Resources.MergedDictionaries.AddRange(darkModeResources);
        }

        public static void ClearDarkMode()
        {
            Application.Current.Resources.MergedDictionaries.Clear();
        }

        public static SolidColorBrush TextBoxSelectedBackgroundColor
        {
            get { return _darkModeEnabled ? Brushes.IndianRed : Brushes.Yellow; }
        }
        public static SolidColorBrush TextBoxUnselectedBackgroundColor
        {
            get { return _darkModeEnabled ? new SolidColorBrush(  Color.FromArgb(255, 56, 56, 56)) : Brushes.White; } //"#FF383838"
        }
    }
}
