using System.Windows;
using System.Windows.Documents;

namespace ProUsbPanels
{
    /// <summary>
    /// Interaction logic for TextBlockDialogWindow.xaml
    /// </summary>
    public partial class TextBlockDialogWindow : Window
    {

        public TextBlockDialogWindow(string title)
        {
            InitializeComponent();
            Title = title;
        }

        public TextBlockDialogWindow(string title, string message)
        {
            InitializeComponent();
            Title = title;
            TextBlockInformation.Text = message;
        }

        private void ButtonOk_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public void Add(string text)
        {
            TextBlockInformation.Inlines.Add(new Run(text));
        }

        public void AddBold(string text)
        {
            TextBlockInformation.Inlines.Add(new Run(text) { FontWeight = FontWeights.Bold });
        }

        public void AddItalic(string text, bool bold = false)
        {
            if (bold)
            {
                TextBlockInformation.Inlines.Add(new Italic(new Run(text) { FontWeight = FontWeights.Bold }));
            }
            else
            {
                TextBlockInformation.Inlines.Add(new Italic(new Run(text)));
            }
        }

        public void AddLineBreak()
        {
            TextBlockInformation.Inlines.Add(new LineBreak());
        }

        public void AddInline(Inline inline)
        {
            TextBlockInformation.Inlines.Add(inline);
        }
    }
}
