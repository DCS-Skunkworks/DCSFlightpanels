using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using ClassLibraryCommon;
using DCS_BIOS;
using HidLibrary;
using NonVisuals.Interfaces;
using NonVisuals.Saitek;
using Theraot.Core;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace NonVisuals
{

    public class ProfileHandler : IProfileHandlerListener, IIsDirty
    {
        //Both directory and filename
        private string _filename = Path.GetFullPath((Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments))) + "\\" + "dcsfp_profile.bindings";
        private string _lastProfileUsed = "";
        private bool _isDirty;
        private bool _isNewProfile;
        private readonly List<string> _listPanelSettingsData = new List<string>();
        private readonly object _lockObject = new object();
        private const string OPEN_FILE_DIALOG_FILE_NAME = "*.bindings";
        private const string OPEN_FILE_DIALOG_DEFAULT_EXT = ".bindings";
        private const string OPEN_FILE_DIALOG_FILTER = "DCSFlightpanels (.bindings)|*.bindings";
        private DCSAirframe _airframe = DCSAirframe.NOFRAMELOADEDYET;

        private readonly List<KeyValuePair<string, GamingPanelEnum>> _profileFileInstanceIDs = new List<KeyValuePair<string, GamingPanelEnum>>();
        private bool _profileLoaded;










        public ProfileHandler(string dcsbiosJSONDirectory)
        {
            DCSBIOSControlLocator.JSONDirectory = dcsbiosJSONDirectory;
        }

        public ProfileHandler(string dcsbiosJSONDirectory, string lastProfileUsed)
        {
            DCSBIOSControlLocator.JSONDirectory = dcsbiosJSONDirectory;
            _lastProfileUsed = lastProfileUsed;
        }

        public string OpenProfile()
        {
            if (IsDirty && MessageBox.Show("Discard unsaved changes to current profile?", "Discard changes?", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
            {
                return null;
            }
            var tempDirectory = string.IsNullOrEmpty(Filename) ? MyDocumentsPath() : Path.GetDirectoryName(Filename);
            ClearAll();
            var openFileDialog = new OpenFileDialog();
            openFileDialog.RestoreDirectory = true;
            openFileDialog.InitialDirectory = tempDirectory;
            openFileDialog.FileName = OPEN_FILE_DIALOG_FILE_NAME;
            openFileDialog.DefaultExt = OPEN_FILE_DIALOG_DEFAULT_EXT;
            openFileDialog.Filter = OPEN_FILE_DIALOG_FILTER;
            if (openFileDialog.ShowDialog() == true)
            {
                return openFileDialog.FileName;
            }

            return null;
        }

        public void PanelSettingsReadFromFile(object sender, SettingsReadFromFileEventArgs e) { }

        public void PanelSettingsChanged(object sender, PanelEventArgs e)
        {
            try
            {
                IsDirty = true;
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }

        public void NewProfile()
        {
            if (IsDirty && MessageBox.Show("Discard unsaved changes to current profile?", "Discard changes?", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
            {
                return;
            }
            _isNewProfile = true;
            ClearAll();
            Airframe = DCSAirframe.NOFRAMELOADEDYET;//Just a default that doesn't remove non emulation panels from the GUI
            Common.UseGenericRadio = false;
            //This sends info to all to clear their settings
            OnClearPanelSettings?.Invoke(this);
        }

        public void ClearAll()
        {
            _listPanelSettingsData.Clear();
            _profileFileInstanceIDs.Clear();
        }

        /*public string DefaultFile()
        {
            return Path.GetFullPath((Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments))) + "\\" + "dcsfp_profile.bindings";
        }*/

        public string MyDocumentsPath()
        {
            return Path.GetFullPath((Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)));
        }

        public bool ReloadProfile()
        {
            return LoadProfile(null);
        }

        public bool LoadProfile(string filename)
        {
            try
            {/*
                 * 0 Open specified filename (parameter) if not null
                 * 1 If exists open last profile used (settings)
                 * 2 Try and open default profile located in My Documents
                 * 3 If none found create default file
                 */
                _isNewProfile = false;
                ClearAll();

                if (!string.IsNullOrEmpty(filename))
                {
                    _filename = filename;
                    _lastProfileUsed = filename;
                }
                else
                {
                    if (!string.IsNullOrEmpty(_lastProfileUsed) && File.Exists(_lastProfileUsed))
                    {
                        _filename = _lastProfileUsed;
                    }
                    else
                    {
                        return false;
                    }
                }
                
                if (string.IsNullOrEmpty(_filename) || !File.Exists(_filename))
                {
                    //Main window will handle this
                    return false;
                }
                /*
                 * Read all information and add InstanceID to all lines using BeginPanel and EndPanel
                 *             
                 * PanelType=PZ55SwitchPanel
                 * PanelInstanceID=\\?\hid#vid_06a3&pid_0d06#8&3f11a32&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}
                 * BeginPanel
                 *      SwitchPanelKey{1KNOB_ENGINE_RIGHT}\o/OSKeyPress{FiftyMilliSec,LSHIFT + VK_Q}
                 *      SwitchPanelKey{1KNOB_ENGINE_LEFT}\o/OSKeyPress{FiftyMilliSec,LCONTROL + VK_Q}
                 *      SwitchPanelKey{1KNOB_ENGINE_BOTH}\o/OSKeyPress{FiftyMilliSec,LSHIFT + VK_C}
                 * EndPanel
                 * 
                 */
                _profileLoaded = true;
                Debug.WriteLine("ProfileHandler reading file ".Append(_filename));
                var fileLines = File.ReadAllLines(_filename);
                var currentPanelType = GamingPanelEnum.Unknown;
                string currentPanelInstanceID = null;
                string currentBindingHash = null;
                var insidePanel = false;
                var insideJSONPanel = false;

                foreach (var fileLine in fileLines)
                {
                    if (fileLine.StartsWith("Airframe="))
                    {
                        if (fileLine.StartsWith("Airframe=NONE"))
                        {
                            //Backward compability
                            _airframe = DCSAirframe.KEYEMULATOR;
                        }
                        else
                        {
                            //Backward compability
                            var airframeAsString = fileLine.Replace("Airframe=", "").Trim();
                            if (airframeAsString.StartsWith("SA342"))
                            {
                                _airframe = DCSAirframe.SA342M;
                            }
                            else if (airframeAsString.StartsWith("P51D") || airframeAsString.StartsWith("TF51D"))
                            {
                                _airframe = DCSAirframe.P51D;
                            }
                            else if (airframeAsString.StartsWith("L39"))
                            {
                                _airframe = DCSAirframe.L39ZA;
                            }
                            else
                            {
                                _airframe = (DCSAirframe)Enum.Parse(typeof(DCSAirframe), airframeAsString);
                            }
                        }
                        DCSBIOSControlLocator.Airframe = _airframe;
                    }
                    else if (fileLine.StartsWith("OperationLevelFlag="))
                    {
                        Common.SetOperationModeFlag(int.Parse(fileLine.Replace("OperationLevelFlag=", "").Trim()));
                    }
                    else if (fileLine.StartsWith("UseGenericRadio="))
                    {
                        Common.UseGenericRadio = (bool.Parse(fileLine.Replace("UseGenericRadio=", "").Trim()));
                    }
                    else if (!fileLine.StartsWith("#") && fileLine.Length > 0)
                    {
                        //Process all these lines.
                        if (fileLine.StartsWith("PanelType="))
                        {
                            currentPanelType = (GamingPanelEnum)Enum.Parse(typeof(GamingPanelEnum), fileLine.Replace("PanelType=", "").Trim());
                        }
                        else if (fileLine.StartsWith("PanelInstanceID="))
                        {
                            currentPanelInstanceID = fileLine.Replace("PanelInstanceID=", "").Trim();
                            _profileFileInstanceIDs.Add(new KeyValuePair<string, GamingPanelEnum>(currentPanelInstanceID, currentPanelType));
                        }
                        else if (fileLine.StartsWith("BindingHash="))
                        {
                            currentBindingHash = fileLine.Replace("BindingHash=", "").Trim();
                        }
                        else if (fileLine.StartsWith("PanelSettingsVersion="))
                        {
                            //do nothing, this will be phased out
                        }
                        else if (fileLine.Equals("BeginPanel"))
                        {
                            insidePanel = true;
                        }
                        else if (fileLine.Equals("EndPanel"))
                        {
                            insidePanel = false;
                        }
                        else if (fileLine.Equals("BeginPanelJSON"))
                        {
                            insideJSONPanel = true;
                        }
                        else if (fileLine.Equals("EndPanelJSON"))
                        {
                            insideJSONPanel = false;
                        }
                        else
                        {
                            if (insidePanel)
                            {
                                var line = fileLine;
                                if (line.StartsWith("\t"))
                                {
                                    line = line.Replace("\t", "");
                                }
                                
                                _listPanelSettingsData.Add(line + SaitekConstants.SEPARATOR_SYMBOL + currentPanelInstanceID + SaitekConstants.PANEL_HASH_SEPARATOR_SYMBOL + currentBindingHash);
                            }
                            
                            if (insideJSONPanel)
                            {
                                _listPanelSettingsData.Add(fileLine + SaitekConstants.SEPARATOR_SYMBOL + currentPanelInstanceID + SaitekConstants.PANEL_HASH_SEPARATOR_SYMBOL + currentBindingHash);
                            }
                        }
                    }
                }
                //For backwards compability 10.11.2018
                if (Common.GetOperationModeFlag() == 0)
                {
                    SetOperationLevelFlag();
                }

                SendSettingsReadEvent();
                CheckAllProfileInstanceIDsAgainstAttachedHardware();
                return true;
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
                return false;
            }
        }
        
        private void SetOperationLevelFlag()
        {
            if (_airframe == DCSAirframe.KEYEMULATOR)
            {
                Common.SetOperationModeFlag(OperationFlag.KeyboardEmulationOnly);
            }
            else if (_airframe == DCSAirframe.KEYEMULATOR_SRS)
            {
                Common.SetOperationModeFlag(OperationFlag.KeyboardEmulationOnly);
                Common.SetOperationModeFlag(OperationFlag.SRSEnabled);
            }
            else if (_airframe == DCSAirframe.FC3_CD_SRS)
            {
                Common.SetOperationModeFlag(OperationFlag.SRSEnabled);
                Common.SetOperationModeFlag(OperationFlag.DCSBIOSOutputEnabled);
            }
            else
            {
                Common.SetOperationModeFlag(OperationFlag.DCSBIOSOutputEnabled | OperationFlag.DCSBIOSInputEnabled);
            }
        }

        private void CheckAllProfileInstanceIDsAgainstAttachedHardware()
        {
            foreach (var saitekPanelSkeleton in Common.GamingPanelSkeletons)
            {
                foreach (var hidDevice in HidDevices.Enumerate(saitekPanelSkeleton.VendorId, saitekPanelSkeleton.ProductId))
                {
                    if (hidDevice != null)
                    {
                        try
                        {
                            _profileFileInstanceIDs.RemoveAll(item => item.Key.Equals(hidDevice.DevicePath));
                        }
                        catch (Exception ex)
                        {
                            Common.ShowErrorMessageBox( ex);
                        }
                    }
                }
            }
            if (_profileFileInstanceIDs.Count > 0)
            {
                if (OnUserMessageEventHandler != null)
                {
                    foreach (var profileFileInstanceID in _profileFileInstanceIDs)
                    {
                        if (profileFileInstanceID.Key != HIDSkeletonIgnore.HidSkeletonIgnore)
                        {
                            OnUserMessageEventHandler(this,
                                new UserMessageEventArgs()
                                {
                                    UserMessage = "The " + profileFileInstanceID.Value + " panel with USB Instance ID :" + Environment.NewLine + profileFileInstanceID.Key + Environment.NewLine +
                                                  "cannot be found. Have you rearranged your panels (USB ports) or have you copied someone else's profile?" + Environment.NewLine +
                                                  "Use the ID button to copy current Instance ID and replace the faulty one in the profile file."
                                });
                        }
                    }
                }
            }
        }

        public void SendSettingsReadEvent()
        {
            try
            {
                if (OnSettingsReadFromFile != null)
                {
                    //TODO DENNA ORSAKAR HÄNGANDE!!
                    OnAirframeSelected?.Invoke(this, new AirframeEventArgs() { Airframe = _airframe });
                    //TODO DENNA ORSAKAR HÄNGANDE!!
                    OnSettingsReadFromFile(this, new SettingsReadFromFileEventArgs() { Settings = _listPanelSettingsData });
                }
            }
            catch (Exception e)
            {
                Common.ShowErrorMessageBox( e);
            }
        }

        public bool SaveAsNewProfile()
        {
            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.RestoreDirectory = true;
            saveFileDialog.InitialDirectory = MyDocumentsPath();
            saveFileDialog.FileName = "dcsfp_profile.bindings";
            saveFileDialog.DefaultExt = OPEN_FILE_DIALOG_DEFAULT_EXT;
            saveFileDialog.Filter = OPEN_FILE_DIALOG_FILTER;
            saveFileDialog.OverwritePrompt = true;
            if (saveFileDialog.ShowDialog() == true)
            {
                _isNewProfile = false;
                Filename = saveFileDialog.FileName;
                _lastProfileUsed = Filename;
                SaveProfile();
                return true;
            }
            return false;
        }

        public void SendEventRegardingSavingPanelConfigurations()
        {
            OnSavePanelSettings?.Invoke(this, new ProfileHandlerEventArgs() { ProfileHandlerEA = this });
            OnSavePanelSettingsJSON?.Invoke(this, new ProfileHandlerEventArgs() { ProfileHandlerEA = this });
        }

        public bool IsNewProfile => _isNewProfile;

        public string Filename
        {
            get => _filename;
            set => _filename = value;
        }

        public void RegisterProfileData(GamingPanel gamingPanel, List<string> strings)
        {
            try
            {
                lock (_lockObject)
                {
                    if (strings == null || strings.Count == 0)
                    {
                        return;
                    }

                    /*
                     * Example:
                     *         
                     * PanelType=PZ55SwitchPanel
                     * PanelInstanceID=\\?\hid#vid_06a3&pid_0d06#8&3f11a32&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}
                     * PanelSettingsVersion=2X
                     * BeginPanel
                     *      SwitchPanelKey{1KNOB_ENGINE_RIGHT}\o/OSKeyPress{FiftyMilliSec,LSHIFT + VK_Q}
                     *      SwitchPanelKey{1KNOB_ENGINE_LEFT}\o/OSKeyPress{FiftyMilliSec,LCONTROL + VK_Q}
                     *      SwitchPanelKey{1KNOB_ENGINE_BOTH}\o/OSKeyPress{FiftyMilliSec,LSHIFT + VK_C}
                     * EndPanel
                     * 
                     */
                    _listPanelSettingsData.Add(Environment.NewLine);
                    _listPanelSettingsData.Add("PanelType=" + gamingPanel.TypeOfPanel);
                    _listPanelSettingsData.Add("PanelInstanceID=" + gamingPanel.InstanceId);
                    _listPanelSettingsData.Add("BindingHash=" + gamingPanel.BindingHash);
                    _listPanelSettingsData.Add("BeginPanel");

                    foreach (var s in strings)
                    {
                        if (s != null)
                        {
                            _listPanelSettingsData.Add("\t" + s);
                        }
                    }

                    _listPanelSettingsData.Add("EndPanel");
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }

        public void RegisterJSONProfileData(GamingPanel gamingPanel, string jsonData)
        {
            try
            {
                lock (_lockObject)
                {
                    if (string.IsNullOrEmpty(jsonData))
                    {
                        return;
                    }

                    _listPanelSettingsData.Add(Environment.NewLine);
                    _listPanelSettingsData.Add("PanelType=" + gamingPanel.TypeOfPanel);
                    _listPanelSettingsData.Add("PanelInstanceID=" + gamingPanel.InstanceId);
                    _listPanelSettingsData.Add("BindingHash=" + gamingPanel.BindingHash);
                    _listPanelSettingsData.Add("BeginPanelJSON");
                    _listPanelSettingsData.Add(jsonData);
                    _listPanelSettingsData.Add("EndPanelJSON");
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }

        public void SaveProfile()
        {
            try
            {
                //Clear all current settings entries, requesting new ones from the panels
                ClearAll();
                SendEventRegardingSavingPanelConfigurations();
                if (IsNewProfile)
                {
                    SaveAsNewProfile();
                    return;
                }
                _lastProfileUsed = Filename;

                var headerStringBuilder = new StringBuilder();
                headerStringBuilder.Append("#This file can be manually edited using any ASCII editor.\n#File created on " + DateTime.Today + " " + DateTime.Now);
                headerStringBuilder.AppendLine("#");
                headerStringBuilder.AppendLine("#");
                headerStringBuilder.AppendLine("#IMPORTANT INFO REGARDING the keyboard key AltGr (RAlt as named in DCS) or RMENU as named DCSFP");
                headerStringBuilder.AppendLine("#When you press AltGr DCSFP will register RMENU + LCONTROL. This is a bug which \"just is\". You need to modify that in the profile");
                headerStringBuilder.AppendLine("#by deleting the + LCONTROL part.");
                headerStringBuilder.AppendLine("#So for example AltGr + HOME pressed on the keyboard becomes RMENU + LCONTROL + HOME");
                headerStringBuilder.AppendLine("#Open text editor and delete the LCONTROL ==> RMENU + HOME");
                var stringBuilder = new StringBuilder();
                stringBuilder.AppendLine(headerStringBuilder.ToString());
                stringBuilder.AppendLine("#  ***Do not change the location nor content of the line below***");
                stringBuilder.AppendLine("Airframe=" + _airframe);
                stringBuilder.AppendLine("OperationLevelFlag=" + Common.GetOperationModeFlag());
                stringBuilder.AppendLine("UseGenericRadio=" + Common.UseGenericRadio);

                foreach (var s in _listPanelSettingsData)
                {
                    stringBuilder.AppendLine(s);
                }
                //if (!Common.Debug)
                //{
                stringBuilder.AppendLine(GetFooter());
                //}
                File.WriteAllText(_filename, stringBuilder.ToString(), Encoding.ASCII);
                _isDirty = false;
                _isNewProfile = false;
                LoadProfile(_filename);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox( ex);
            }
        }

        private string GetFooter()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine + "#--------------------------------------------------------------------");
            stringBuilder.AppendLine("#Below are all the Virtual Keycodes in use listed. You can manually edit this file using these codes.");
            var enums = Enum.GetNames(typeof(VirtualKeyCode));
            foreach (var @enum in enums)
            {
                stringBuilder.AppendLine("#\t" + @enum);
            }
            return stringBuilder.ToString();
        }

        public bool IsDirty
        {
            get => _isDirty;
            set => _isDirty = value;
        }

        public void SetIsDirty()
        {
            _isDirty = true;
        }

        public DCSAirframe Airframe
        {
            get => _airframe;
            set
            {
                //Called only when user creates a new profile
                if (value != _airframe)
                {
                    SetIsDirty();
                }
                _airframe = value;
                Common.ResetOperationModeFlag();
                SetOperationLevelFlag();
                OnAirframeSelected?.Invoke(this, new AirframeEventArgs() { Airframe = _airframe });
            }
        }

        public string LastProfileUsed
        {
            get => _lastProfileUsed;
            set => _lastProfileUsed = value;
        }

        public string DCSBIOSJSONDirectory
        {
            get => DCSBIOSControlLocator.JSONDirectory;
            set => DCSBIOSControlLocator.JSONDirectory = value;
        }

        public void SelectedAirframe(object sender, AirframeEventArgs e) { }

        public bool ProfileLoaded => _profileLoaded || _isNewProfile;

        public bool UseNS430
        {
            get => Common.IsOperationModeFlagSet(OperationFlag.NS430Enabled);
            set
            {
                if (value)
                {
                    Common.SetOperationModeFlag(OperationFlag.NS430Enabled);
                }
                else
                {
                    Common.ClearOperationModeFlag(OperationFlag.NS430Enabled);
                }
                SetIsDirty();
            }
        }


        public delegate void ProfileReadFromFileEventHandler(object sender, SettingsReadFromFileEventArgs e);
        public event ProfileReadFromFileEventHandler OnSettingsReadFromFile;

        public delegate void SavePanelSettingsEventHandler(object sender, ProfileHandlerEventArgs e);
        public event SavePanelSettingsEventHandler OnSavePanelSettings;

        public delegate void SavePanelSettingsEventHandlerJSON(object sender, ProfileHandlerEventArgs e);
        public event SavePanelSettingsEventHandlerJSON OnSavePanelSettingsJSON;

        public delegate void AirframeSelectedEventHandler(object sender, AirframeEventArgs e);
        public event AirframeSelectedEventHandler OnAirframeSelected;

        public delegate void ClearPanelSettingsEventHandler(object sender);
        public event ClearPanelSettingsEventHandler OnClearPanelSettings;

        public delegate void UserMessageEventHandler(object sender, UserMessageEventArgs e);
        public event UserMessageEventHandler OnUserMessageEventHandler;

        public void Attach(GamingPanel gamingPanel)
        {
            OnSettingsReadFromFile += gamingPanel.PanelSettingsReadFromFile;
            OnSavePanelSettings += gamingPanel.SavePanelSettings;
            OnSavePanelSettingsJSON += gamingPanel.SavePanelSettingsJSON;
            OnClearPanelSettings += gamingPanel.ClearPanelSettings;
            OnAirframeSelected += gamingPanel.SelectedAirframe;
        }

        public void Detach(GamingPanel gamingPanel)
        {
            OnSettingsReadFromFile -= gamingPanel.PanelSettingsReadFromFile;
            OnSavePanelSettings -= gamingPanel.SavePanelSettings;
            OnSavePanelSettingsJSON -= gamingPanel.SavePanelSettingsJSON;
            OnClearPanelSettings -= gamingPanel.ClearPanelSettings;
            OnAirframeSelected -= gamingPanel.SelectedAirframe;
        }

        public void Attach(IProfileHandlerListener gamingPanelSettingsListener)
        {
            OnSettingsReadFromFile += gamingPanelSettingsListener.PanelSettingsReadFromFile;
            OnAirframeSelected += gamingPanelSettingsListener.SelectedAirframe;
        }

        public void Detach(IProfileHandlerListener gamingPanelSettingsListener)
        {
            OnSettingsReadFromFile -= gamingPanelSettingsListener.PanelSettingsReadFromFile;
            OnAirframeSelected -= gamingPanelSettingsListener.SelectedAirframe;
        }

        public void AttachUserMessageHandler(IUserMessageHandler userMessageHandler)
        {
            OnUserMessageEventHandler += userMessageHandler.UserMessage;
        }

        public void DetachUserMessageHandler(IUserMessageHandler userMessageHandler)
        {
            OnUserMessageEventHandler -= userMessageHandler.UserMessage;
        }

        public void StateSaved()
        {

        }
    }
    
    public class AirframeEventArgs : EventArgs
    {
        public DCSAirframe Airframe { get; set; }
    }

    public class ProfileHandlerEventArgs : EventArgs
    {
        public ProfileHandler ProfileHandlerEA { get; set; }
    }

    public class UserMessageEventArgs : EventArgs
    {
        public string UserMessage { get; set; }
    }



}
