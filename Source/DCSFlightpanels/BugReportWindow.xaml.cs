using System;
using System.ComponentModel;
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
    /// Interaction logic for BugReportWindow.xaml
    /// </summary>
    public partial class BugReportWindow : Window
    {
        private string _isThisDCSBIOSRelated = "";
        private string _relatedCockpitControl = "";

        public BugReportWindow()
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

        private string GatherDataForForum()
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
                result.AppendLine(TextBoxProblemDCSBIOS.Text);
                result.AppendLine("Related cockpit control/device/gauge? : " + _relatedCockpitControl);
                result.AppendLine(TextBoxProblemControl.Text);
                result.AppendLine("[/CODE]");
                result.AppendLine("\n");
                result.AppendLine("[CODE]");
                result.AppendLine(TextBlockErrorLog.Text);
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


        private string GatherDataForDiscord()
        {
            try
            {
                var result = new StringBuilder();
                result.AppendLine("**Bug Report from DCSFP**");
                result.AppendLine("```**DCS Installation : **" + (string)ComboBoxDCSInstallation.SelectedValue);
                result.AppendLine("**DCS Module : **" + (string)ComboBoxDCSModule.SelectedValue);
                result.AppendLine("**Saitek Panel : **" + (string)ComboBoxSaitekPanel.SelectedValue);
                result.AppendLine("**Has it worked before? : **" + (string)ComboBoxOldProblem.SelectedValue);
                result.AppendLine("**DCS-BIOS Status? : **" + (string)ComboBoxDCSBIOSStatus.SelectedValue);
                result.AppendLine("**Is this DCS-BIOS related? : **" + _isThisDCSBIOSRelated);
                if (!string.IsNullOrEmpty(TextBoxProblemDCSBIOS.Text))
                {
                    result.AppendLine(TextBoxProblemDCSBIOS.Text);
                }
                result.AppendLine("**Related cockpit control/device/gauge? : **" + _relatedCockpitControl);
                if (!string.IsNullOrEmpty(TextBoxProblemControl.Text))
                {
                    result.AppendLine(TextBoxProblemControl.Text);
                }
                result.AppendLine("```\n");
                result.AppendLine("**Error log**");
                result.AppendLine("```cs" + TextBlockErrorLog.Text + "```");
                result.AppendLine("\n");
                result.AppendLine("**Description**");
                result.AppendLine("```cs" + TextBlockExplanation.Text + "```");
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
            ComboBoxDCSModule.Items.Clear();
            foreach (DCSAirframe airframe in Enum.GetValues(typeof(DCSAirframe)))
            {
                if (airframe != DCSAirframe.NOFRAMELOADEDYET && airframe != DCSAirframe.NS430)
                {
                    ComboBoxDCSModule.Items.Add(EnumEx.GetDescription(airframe));
                }
            }
            ComboBoxDCSModule.Items.Insert(0,"Not applicable");
        }

        private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
        
        private void ButtonForumFormatCopy_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CheckDataIsOk())
                {
                    var bugReport = GatherDataForForum();
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

        private void ButtonDiscordFormatCopy_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CheckDataIsOk())
                {
                    var bugReport = GatherDataForDiscord();
                    if (!string.IsNullOrEmpty(bugReport) && bugReport.Length >= 2000)
                    {
                        MessageBox.Show("Report is too long. Discord has a size limit of 2000 characters.\nPlease modify your text.\nCurrent length is " + bugReport.Length + " characters.", "Report too large", MessageBoxButton.OK,
                            MessageBoxImage.Information);
                        return;
                    }
                    if (!string.IsNullOrEmpty(bugReport))
                    {
                        Clipboard.SetText(bugReport);
                        MessageBox.Show("Bug report copied to Clipboard.\n\nHead on over to the Discord server paste the contents in the bug channel.", "Bug Report OK", MessageBoxButton.OK,
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

        private void BugReportWindow_OnClosing(object sender, CancelEventArgs e)
        {
            try
            {
                if (MessageBox.Show("Close Bug Report?", "Close", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.No)
                {
                    e.Cancel = true;
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(20297, ex);
            }
        }
    }
}
