using System;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using ClassLibraryCommon;
using ControlReference.CustomControls;

namespace ControlReference.Windows
{
    /// <summary>
    /// Interaction logic for LuaWindow.xaml
    /// </summary>
    public partial class LuaWindow
    {
        private bool _isLoaded;
        private readonly string _luaCommand;

        public LuaWindow(string dcsbiosControlId, string luaCommand)
        {
            InitializeComponent();
            _luaCommand = luaCommand;
            LabelControl.Content = dcsbiosControlId.Replace("_", "__");
        }

        private void SetFormState()
        {
        }

        private void LuaWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_isLoaded)
                {
                    return;
                }
                
                var textBlock = new TextBlockSelectable(_luaCommand);
                textBlock.MouseEnter += Common.UIElement_OnMouseEnterHandIcon;
                textBlock.MouseLeave += Common.UIElement_OnMouseLeaveNormalIcon;
                SetContextMenu(textBlock);

                textBlock.FontFamily = new System.Windows.Media.FontFamily("Consolas");
                textBlock.Width = double.NaN;
                
                var border = new Border
                {
                    Child = textBlock
                };
                StackPanelLuaCommand.Children.Add(border);
                StackPanelLuaCommand.Children.Add(new Line());

                StackPanelLuaCommand.UpdateLayout();

                SetFormState();
                _isLoaded = true;
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message + Environment.NewLine + exception.StackTrace);
            }
        }

        private void LuaWindow_OnKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {

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

        private void LuaWindow_OnKeyUp(object sender, KeyEventArgs e)
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
    }
}
