using System;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using ClassLibraryCommon;

namespace ControlReference.CustomControls
{
    internal class TextBlockSelectable : TextBlock
    {
        TextPointer _startSelectPosition;
        TextPointer _endSelectPosition;
        private string _selectedText = "";

        public delegate void TextSelectedHandler(string selectedText);
        public event TextSelectedHandler TextSelected;
        private TextRange _selectedTextRange;

        private readonly ContextMenu _contextMenu = new();

        public static Color SelectedBackgroundColor { get; set; } = Color.FromRgb(187, 191, 189);
        public static Color TextBackgroundColor { get; set; } = Colors.WhiteSmoke;

        public TextBlockSelectable()
        {
            SetContextMenu();
        }

        public TextBlockSelectable(string text)
        {
            Text = text;
            SetContextMenu();
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            try
            {
                base.OnMouseDown(e);

                if (e.RightButton == MouseButtonState.Pressed) return;

                if (e.ClickCount == 2)
                {
                    SelectAll();
                }
                else
                {
                    DeSelectAll();
                }

                var mouseDownPoint = e.GetPosition(this);

                _startSelectPosition = GetPositionFromPoint(mouseDownPoint, true);
            }
            catch (Exception exception)
            {
                Common.ShowMessageBox(exception.Message + Environment.NewLine + exception.StackTrace);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            try
            {
                base.OnMouseMove(e);
                if (e.LeftButton != MouseButtonState.Pressed) return;

                var mouseUpPoint = e.GetPosition(this);
                _endSelectPosition = GetPositionFromPoint(mouseUpPoint, true);

                if (_startSelectPosition == null || _endSelectPosition == null) return;
                _selectedTextRange = new TextRange(_startSelectPosition, _endSelectPosition);
                _selectedTextRange.ApplyPropertyValue(TextElement.BackgroundProperty, new SolidColorBrush(SelectedBackgroundColor));
            }
            catch (Exception exception)
            {
                Common.ShowMessageBox(exception.Message + Environment.NewLine + exception.StackTrace);
            }
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            try
            {
                if (_selectedTextRange == null) return;
                _selectedText = _selectedTextRange.Text;
                TextSelected?.Invoke(_selectedText);
            }
            catch (Exception exception)
            {
                Common.ShowMessageBox(exception.Message + Environment.NewLine + exception.StackTrace);
            }
        }


        protected override void OnMouseEnter(MouseEventArgs e)
        {
            try
            {
                Mouse.OverrideCursor = Cursors.IBeam;
            }
            catch (Exception exception)
            {
                Common.ShowMessageBox(exception.Message + Environment.NewLine + exception.StackTrace);
            }
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Arrow;
            }
            catch (Exception exception)
            {
                Common.ShowMessageBox(exception.Message + Environment.NewLine + exception.StackTrace);
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            try
            {
                if (e.SystemKey == Key.LeftCtrl && e.Key == Key.C)
                {
                    CopyToClipboard();
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        private void SelectAll()
        {
            try
            {
                //Double click => select all
                var textRange = new TextRange(ContentStart, ContentEnd);
                textRange.ApplyPropertyValue(TextElement.BackgroundProperty, new SolidColorBrush(SelectedBackgroundColor));
                _selectedTextRange = new TextRange(ContentStart, ContentEnd);
            }
            catch (Exception exception)
            {
                Common.ShowMessageBox(exception.Message + Environment.NewLine + exception.StackTrace);
            }
        }

        private void DeSelectAll()
        {
            try
            {
                //De-select any selection
                var textRange = new TextRange(ContentStart, ContentEnd);
                textRange.ApplyPropertyValue(TextElement.BackgroundProperty, new SolidColorBrush(TextBackgroundColor));
                _selectedTextRange = null;
            }
            catch (Exception exception)
            {
                Common.ShowMessageBox(exception.Message + Environment.NewLine + exception.StackTrace);
            }
        }

        private void SetContextMenu()
        {
            try
            {
                //_contextMenu.Opened += TextBlockContextMenuOpened;

                var menuItemCopy = new MenuItem();
                menuItemCopy.Header = "Copy";
                menuItemCopy.Click += MenuItemCopy_OnClick;
                //menuItemCopy.ContextMenuOpening += TextBlockContextMenuOpened;
                _contextMenu.Items.Add(menuItemCopy);
                ContextMenu = _contextMenu;
            }
            catch (Exception exception)
            {
                Common.ShowMessageBox(exception.Message + Environment.NewLine + exception.StackTrace);
            }
        }
        /*
        
        public void TextBlockContextMenuOpened(object sender, RoutedEventArgs e)
        {
            try
            {
                var menuItemCopy = (MenuItem) sender;
                menuItemCopy.IsEnabled = !string.IsNullOrEmpty(_selectedText);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }
        */

        private void MenuItemCopy_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                CopyToClipboard();
            }
            catch (Exception exception)
            {
                Common.ShowMessageBox(exception.Message + Environment.NewLine + exception.StackTrace);
            }
        }

        private void CopyToClipboard()
        {
            if (string.IsNullOrEmpty(_selectedText))
            {
                SelectAll();
            }
            Clipboard.SetText(_selectedText ?? "");
            SystemSounds.Asterisk.Play();
        }
    }
}
