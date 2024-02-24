using System;
using System.Collections.Generic;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ClassLibraryCommon;
using ControlReference.CustomControls;
using DCS_BIOS.ControlLocator;
using DCS_BIOS.Json;

namespace ControlReference.Windows
{
    /// <summary>
    /// Interaction logic for GlobalSearchWindow.xaml
    /// </summary>
    public partial class GlobalSearchWindow
    {
        private bool _isLoaded;

        public GlobalSearchWindow()
        {
            InitializeComponent();
        }

        private void SetFormState()
        {
        }

        private void GlobalSearchWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_isLoaded)
                {
                    return;
                }
                
                SetFormState();
                TextBoxSearchControl.Focus();
                _isLoaded = true;
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message + Environment.NewLine + exception.StackTrace);
            }
        }

        private void ShowControls(List<Tuple<string, DCSBIOSControl>> controls)
        {
            StackPanelControls.Children.Clear();
            var lastControl = "";
            TextBlockSelectable textBlock = null;
            foreach (var tuple in controls)
            {
                if (tuple.Item1 != lastControl)
                {
                    lastControl = tuple.Item1;
                    textBlock = new TextBlockSelectable(tuple.Item1 + Environment.NewLine);
                    textBlock.MouseEnter += Common.UIElement_OnMouseEnterHandIcon;
                    textBlock.MouseLeave += Common.UIElement_OnMouseLeaveNormalIcon;
                    SetContextMenu(textBlock);

                    textBlock.FontFamily = new System.Windows.Media.FontFamily("Consolas");
                    textBlock.Width = double.NaN;

                    var border = new Border
                    {
                        Child = textBlock
                    };
                    StackPanelControls.Children.Add(border);
                    StackPanelControls.Children.Add(new Line());

                    StackPanelControls.UpdateLayout();
                }

                if (textBlock != null)
                {
                    textBlock.Text += tuple.Item2.Identifier + "\t" + tuple.Item2.Description + Environment.NewLine;
                }
            }
        }

        private void GlobalSearchWindow_OnKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.F && (Keyboard.Modifiers & (ModifierKeys.Control | ModifierKeys.Shift)) == (ModifierKeys.Control | ModifierKeys.Shift))
                {
                    TextBoxSearchControl.Focus();
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }
        
        private void SetContextMenu(TextBlockSelectable textBlock)
        {
            try
            {
                //_contextMenu.Opened += TextBlockContextMenuOpened;
                ContextMenu contextMenu = new();
                contextMenu.Opened += TextBlock_ContextMenuOpened;
                contextMenu.Tag = textBlock;
                var menuItemCopy = new MenuItem
                {
                    Tag = textBlock,
                    Header = "Copy"
                };
                menuItemCopy.Click += MenuItemCopy_OnClick;
                contextMenu.Items.Add(menuItemCopy);
                textBlock.ContextMenu = contextMenu;
            }
            catch (Exception exception)
            {
                Common.ShowMessageBox(exception.Message + Environment.NewLine + exception.StackTrace);
            }
        }

        private static void TextBlock_ContextMenuOpened(object sender, RoutedEventArgs e)
        {
            try
            {
                var contextMenu = (ContextMenu)sender;
                var textBlock = (TextBlockSelectable)contextMenu.Tag;
                
                ((MenuItem)contextMenu.Items[0])!.IsEnabled = !string.IsNullOrEmpty(textBlock.SelectedText);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void MenuItemCopy_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var textBlock = ((MenuItem)sender).Tag;
                CopyToClipboard((TextBlockSelectable)textBlock);
            }
            catch (Exception exception)
            {
                Common.ShowMessageBox(exception.Message + Environment.NewLine + exception.StackTrace);
            }
        }

        private static void CopyToClipboard(TextBlockSelectable textBlock)
        {
            if (string.IsNullOrEmpty(textBlock.SelectedText)) textBlock.SelectAll();

            Clipboard.SetText(textBlock.SelectedText ?? "");
            SystemSounds.Asterisk.Play();
        }

        private void GlobalSearchWindow_OnKeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key != Key.Escape) return;

                Close();
            }
            catch (Exception exception)
            {
                Common.ShowMessageBox(exception.Message + Environment.NewLine + exception.StackTrace);
            }
        }
        
        private void TextBoxSearchControl_OnKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(TextBoxSearchControl.Text)) return;

                if (e.Key != Key.Enter) return;

                var controls = DCSBIOSControlLocator.GlobalControlSearch(TextBoxSearchControl.Text);
                ShowControls(controls);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void ButtonSearchControls_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(TextBoxSearchControl.Text)) return;

                var controls = DCSBIOSControlLocator.GlobalControlSearch(TextBoxSearchControl.Text);
                ShowControls(controls);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }
    }
}
