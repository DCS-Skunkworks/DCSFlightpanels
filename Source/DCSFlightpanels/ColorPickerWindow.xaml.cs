using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ClassLibraryCommon;
using Color = System.Windows.Media.Color;

namespace DCSFlightpanels
{
    /// <summary>
    /// Interaction logic for ColorPickerWindow.xaml
    /// </summary>
    public partial class ColorPickerWindow : Window
    {
        private bool _isLoaded = false;
        private Color? _selectedColor;


        public ColorPickerWindow()
        {
            InitializeComponent();
        }

        public ColorPickerWindow(Color color)
        {
            InitializeComponent();
            _selectedColor = color;
            try
            {
                ColorPickerColor.SelectedColor = _selectedColor;
            }
            catch (Exception)
            {
                _selectedColor = Color.FromRgb(255, 255, 255);
                ColorPickerColor.SelectedColor = _selectedColor;
            }
        }

        private void ColorPickerWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _isLoaded = true;
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(20135444, ex);
            }
        }

        private void ColorPickerColor_OnSelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            try
            {
                if (ColorPickerColor.SelectedColor != null)
                {
                    _selectedColor = ColorPickerColor.SelectedColor;
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(20135444, ex);
            }
        }

        private void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void ButtonOk_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        public Color SelectedColor => _selectedColor.GetValueOrDefault();

        public string SelectedColorHtmlString => ColorTranslator.ToHtml(System.Drawing.Color.FromArgb(_selectedColor.GetValueOrDefault().A,
            _selectedColor.GetValueOrDefault().R,
            _selectedColor.GetValueOrDefault().G,
            _selectedColor.GetValueOrDefault().B));

        /*
         * System.Windows.Media.Color color; 
         * System.Drawing.Color newColor = System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
         *
         * System.Drawing.Color color; 
         * System.Windows.Media.Color newColor = System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B);
         */
    }
}
