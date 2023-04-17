using ClassLibraryCommon;
using System;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using DCS_BIOS.Json;
using System.Windows.Controls.Primitives;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace DCSFlightpanels.Windows
{
    internal static class TextBoxSearchCommon
    {
        internal static void SetBackgroundSearchBanner(TextBox textBoxSearch)
        {
            try
            {
                if (String.IsNullOrEmpty(textBoxSearch.Text))
                {
                    // Create an ImageBrush.
                    var textImageBrush = new ImageBrush
                    {
                        ImageSource = new BitmapImage(
                            new Uri("pack://application:,,,/dcsfp;component/Images/cue_banner_search_dcsbios.png", UriKind.RelativeOrAbsolute)),
                        AlignmentX = AlignmentX.Left,
                        Stretch = Stretch.Uniform
                    };

                    // Use the brush to paint the button's background.
                    textBoxSearch.Background = textImageBrush;
                }
                else
                {
                    textBoxSearch.Background = null;
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        internal static void AdjustShownPopupData(TextBox textBoxSearch, Popup popupSearch, DataGrid dataGridValues, IEnumerable<DCSBIOSControl> dcsbiosControls)
        {
            try
            {
                popupSearch.PlacementTarget = textBoxSearch;
                popupSearch.Placement = PlacementMode.Bottom;
                dataGridValues.Tag = textBoxSearch;
                if (!popupSearch.IsOpen)
                {
                    popupSearch.IsOpen = true;
                }
                if (dataGridValues != null)
                {
                    if (string.IsNullOrEmpty(textBoxSearch.Text))
                    {
                        dataGridValues.DataContext = dcsbiosControls;
                        dataGridValues.ItemsSource = dcsbiosControls;
                        dataGridValues.Items.Refresh();
                        return;
                    }
                    var subList = dcsbiosControls.Where(controlObject => (!string.IsNullOrWhiteSpace(controlObject.Identifier) && controlObject.Identifier.ToUpper().Contains(textBoxSearch.Text.ToUpper()))
                                                                          || (!string.IsNullOrWhiteSpace(controlObject.Description) && controlObject.Description.ToUpper().Contains(textBoxSearch.Text.ToUpper())));
                    dataGridValues.DataContext = subList;
                    dataGridValues.ItemsSource = subList;
                    dataGridValues.Items.Refresh();
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex, "AdjustShownPopupData()");
            }
        }

        internal static void HandleFirstSpace(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space && ((TextBox)sender).Text == "")
            {
                e.Handled = true;
            }
        }
    }
}
