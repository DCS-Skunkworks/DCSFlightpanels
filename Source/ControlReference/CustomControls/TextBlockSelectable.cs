using System;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using ClassLibraryCommon;

namespace ControlReference.CustomControls
{
    internal class TextBlockSelectable : TextBlock
    {
        private TextPointer _startSelectPosition;
        private TextPointer _endSelectPosition;
        public string SelectedText = "";

        public delegate void TextSelectedHandler(string selectedText);
        public event TextSelectedHandler TextSelected;
        private TextRange _selectedTextRange;

        private static Color SelectedBackgroundColor { get; set; } = Color.FromRgb(187, 191, 189);
        private static Color TextBackgroundColor { get; set; } = Colors.WhiteSmoke;
        

        public TextBlockSelectable(string text)
        {
            Text = text;
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
                SelectedText = _selectedTextRange.Text;
                TextSelected?.Invoke(SelectedText);
            }
            catch (Exception exception)
            {
                Common.ShowMessageBox(exception.Message + Environment.NewLine + exception.StackTrace);
            }
        }

        public void SelectAll()
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

        public void DeSelectAll()
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

    }
}
