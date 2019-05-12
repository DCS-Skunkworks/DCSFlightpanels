using System;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using ClassLibraryCommon;
using CommonClassLibraryJD;

namespace DCSFlightpanels
{
    /// <summary>
    /// Interaction logic for ReportBugWindow.xaml
    /// </summary>
    public partial class ReportBugWindow : Window
    {
        private string _isThisDCSBIOSRelated = "";
        private string _relatedCockpitControl = "";

        public ReportBugWindow()
        {
            InitializeComponent();
        }

        private void ReportBugWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                PopulateAirframeCombobox();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(20297, ex);
            }
        }

        private void SetFormState()
        {

        }

        private bool CheckDataIsOk()
        {
            try
            {
                if (ComboBoxDCSInstallation.SelectedItem == null)
                {
                    MessageBox.Show("DCS Installation Information missing.", "Information Missing", MessageBoxButton.OK, MessageBoxImage.Warning);
                    ComboBoxDCSInstallation.IsDropDownOpen = true;
                    return false;
                }
                if (ComboBoxDCSModule.SelectedItem == null)
                {
                    MessageBox.Show("DCS Module Information missing.", "Information Missing", MessageBoxButton.OK, MessageBoxImage.Warning);
                    ComboBoxDCSModule.IsDropDownOpen = true;
                    return false;
                }
                if (ComboBoxSaitekPanel.SelectedItem == null)
                {
                    MessageBox.Show("Saitek Panel Information missing.", "Information Missing", MessageBoxButton.OK, MessageBoxImage.Warning);
                    ComboBoxSaitekPanel.IsDropDownOpen = true;
                    return false;
                }
                if (ComboBoxOldProblem.SelectedItem == null)
                {
                    MessageBox.Show("Specify whether it has worked before.", "Information Missing", MessageBoxButton.OK, MessageBoxImage.Warning);
                    ComboBoxOldProblem.IsDropDownOpen = true;
                    return false;
                }
                if (ComboBoxDCSBIOSStatus.SelectedItem == null)
                {
                    MessageBox.Show("DCS-BIOS information is missing.", "Information Missing", MessageBoxButton.OK, MessageBoxImage.Warning);
                    ComboBoxDCSBIOSStatus.IsDropDownOpen = true;
                    return false;
                }

                if (RadioButtonDCSBIOSRelatedYes.IsChecked == false && RadioButtonDCSBIOSRelatedNo.IsChecked == false && RadioButtonDCSBIOSRelatedDontKnow.IsChecked == false)
                {
                    MessageBox.Show("Specify whether this is DCS-BIOS related.", "Information Missing", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                if (RadioButtonCockpitControlYes.IsChecked == false && RadioButtonCockpitControlNo.IsChecked == false && RadioButtonCockpitControlDontKnow.IsChecked == false)
                {
                    MessageBox.Show("Specify whether this is related to a specific cockpit control.", "Information Missing", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
                if (string.IsNullOrEmpty(TextBlockExplanation.Text))
                {
                    MessageBox.Show("Missing problem description", "Information Missing", MessageBoxButton.OK, MessageBoxImage.Warning);
                    TextBlockExplanation.Focus();
                    return false;
                }
                
                return true;
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(20297, ex);
                return false;
            }
        }

        private string GatherData()
        {
            try
            {
                var result = new StringBuilder();
                result.AppendLine("Bug Report from DCSFP");
                result.AppendLine("[CODE]");
                result.AppendLine("DCS Installation : " + (string)ComboBoxDCSInstallation.SelectedValue);
                result.AppendLine("DCS Module : " + (string)ComboBoxDCSModule.SelectedValue);
                result.AppendLine("Saitek Panel : " + (string)ComboBoxSaitekPanel.SelectedValue);
                result.AppendLine("Has it worked before? : " + (string)ComboBoxOldProblem.SelectedValue);
                result.AppendLine("DCS-BIOS Status? : " + (string)ComboBoxDCSBIOSStatus.SelectedValue);
                result.AppendLine("Is this DCS-BIOS related? : " + _isThisDCSBIOSRelated);
                result.AppendLine("Related cockpit control/device/gauge? : " + _relatedCockpitControl);
                result.AppendLine("[/CODE]");
                result.AppendLine("\n");
                result.AppendLine("[CODE]");
                result.AppendLine(TextBlockErrorlog.Text);
                result.AppendLine("[/CODE]");
                result.AppendLine("\n");
                result.AppendLine("[CODE]");
                result.AppendLine(TextBlockExplanation.Text);
                result.AppendLine("[/CODE]");
                return result.ToString();
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(20297, ex);
            }
            return null;
        }

        private void PopulateAirframeCombobox()
        {
            if (!IsLoaded)
            {
                return;
            }
            ComboBoxDCSModule.SelectionChanged -= ComboBoxDCSModule_OnSelectionChanged;
            ComboBoxDCSModule.Items.Clear();
            foreach (DCSAirframe airframe in Enum.GetValues(typeof(DCSAirframe)))
            {
                if (airframe != DCSAirframe.NOFRAMELOADEDYET && airframe != DCSAirframe.NS430)
                {
                    ComboBoxDCSModule.Items.Add(EnumEx.GetDescription(airframe));
                }
            }

            ComboBoxDCSModule.Items.Add("Not applicable");
            ComboBoxDCSModule.SelectedIndex = 0;
            ComboBoxDCSModule.SelectionChanged += ComboBoxDCSModule_OnSelectionChanged;
        }

        private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void ComboBoxDCSModule_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void ComboBoxSaitekPanel_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void ComboBoxOldProblem_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void ComboBoxDCSInstallation_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void ButtonCopy_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CheckDataIsOk())
                {
                    var bugReport = GatherData();
                    if (!string.IsNullOrEmpty(bugReport))
                    {
                        Clipboard.SetText(bugReport);
                        MessageBox.Show("Bug report copied to Clipboard.\n\nHead on over to the support thread and paste the contents into a new post.", "Bug Report OK", MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Bug report creation failed", "Bug Report Failed", MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(20297, ex);
            }
        }

        private void ButtonClose_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MessageBox.Show("Close Bug Report?", "Close", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
                {
                    Close();
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(20297, ex);
            }
        }

        private void ComboBoxDCSBIOSStatus_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void RadioButtonDCSBIOSRelatedYes_OnChecked(object sender, RoutedEventArgs e)
        {
            _isThisDCSBIOSRelated = "Yes";
        }

        private void RadioButtonDCSBIOSRelatedNo_OnChecked(object sender, RoutedEventArgs e)
        {
            _isThisDCSBIOSRelated = "No";
        }

        private void RadioButtonDCSBIOSRelatedDontKnow_OnChecked(object sender, RoutedEventArgs e)
        {
            _isThisDCSBIOSRelated = "I don't know";
        }

        private void RadioButtonCockpitControlYes_OnChecked(object sender, RoutedEventArgs e)
        {
            _relatedCockpitControl = "Yes";
        }

        private void RadioButtonCockpitControlNo_OnChecked(object sender, RoutedEventArgs e)
        {
            _relatedCockpitControl = "No";
        }

        private void RadioButtonCockpitControlDontKnow_OnChecked(object sender, RoutedEventArgs e)
        {
            _relatedCockpitControl = "I don't know";
        }
    }
}
