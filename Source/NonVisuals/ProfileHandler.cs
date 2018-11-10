using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using ClassLibraryCommon;
using DCS_BIOS;
using HidLibrary;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace NonVisuals
{

    public class ProfileHandler : IProfileHandlerListener
    {
        public delegate void ProfileReadFromFileEventHandler(object sender, SettingsReadFromFileEventArgs e);
        public event ProfileReadFromFileEventHandler OnSettingsReadFromFile;

        public delegate void SavePanelSettingsEventHandler(object sender, ProfileHandlerEventArgs e);
        public event SavePanelSettingsEventHandler OnSavePanelSettings;

        public delegate void AirframeSelectedEventHandler(object sender, AirframeEventArgs e);
        public event AirframeSelectedEventHandler OnAirframeSelected;

        public delegate void ClearPanelSettingsEventHandler(object sender);
        public event ClearPanelSettingsEventHandler OnClearPanelSettings;

        public delegate void UserMessageEventHandler(object sender, UserMessageEventArgs e);
        public event UserMessageEventHandler OnUserMessageEventHandler;

        public void Attach(SaitekPanel saitekPanel)
        {
            OnSettingsReadFromFile += saitekPanel.PanelSettingsReadFromFile;
            OnSavePanelSettings += saitekPanel.SavePanelSettings;
            OnClearPanelSettings += saitekPanel.ClearPanelSettings;
            OnAirframeSelected += saitekPanel.SelectedAirframe;
        }

        public void Detach(SaitekPanel saitekPanel)
        {
            OnSettingsReadFromFile -= saitekPanel.PanelSettingsReadFromFile;
            OnSavePanelSettings -= saitekPanel.SavePanelSettings;
            OnClearPanelSettings -= saitekPanel.ClearPanelSettings;
            OnAirframeSelected -= saitekPanel.SelectedAirframe;
        }

        public void Attach(IProfileHandlerListener saitekPanelSettingsListener)
        {
            OnSettingsReadFromFile += saitekPanelSettingsListener.PanelSettingsReadFromFile;
            OnAirframeSelected += saitekPanelSettingsListener.SelectedAirframe;
        }

        public void Detach(IProfileHandlerListener saitekPanelSettingsListener)
        {
            OnSettingsReadFromFile -= saitekPanelSettingsListener.PanelSettingsReadFromFile;
            OnAirframeSelected -= saitekPanelSettingsListener.SelectedAirframe;
        }

        public void AttachUserMessageHandler(IUserMessageHandler userMessageHandler)
        {
            OnUserMessageEventHandler += userMessageHandler.UserMessage;
        }

        public void DetachUserMessageHandler(IUserMessageHandler userMessageHandler)
        {
            OnUserMessageEventHandler -= userMessageHandler.UserMessage;
        }

        //Both directory and filename
        private string _filename = Path.GetFullPath((Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments))) + "\\" + "Saitek_DCS_Profile.bindings";
        private string _lastProfileUsed = "";
        private string _jsonDirectory = "";
        private bool _isDirty;
        private bool _isNewProfile;
        private readonly List<string> _listPanelSettingsData = new List<string>();
        private readonly object _lockObject = new object();
        private const string OpenFileDialogFileName = "*.bindings";
        private const string OpenFileDialogDefaultExt = ".bindings";
        private const string OpenFileDialogFilter = "DCSFlightpanels (.bindings)|*.bindings";
        private DCSAirframe _airframe = DCSAirframe.NOFRAMELOADEDYET;

        private readonly List<KeyValuePair<string, SaitekPanelsEnum>> _profileFileInstanceIDs = new List<KeyValuePair<string, SaitekPanelsEnum>>();
        private bool _profileLoaded;

        public ProfileHandler(string jsonDirectory)
        {
            _jsonDirectory = jsonDirectory;
            DCSBIOSControlLocator.JSONDirectory = jsonDirectory;
        }

        public ProfileHandler(string jsonDirectory, string lastProfileUsed)
        {
            _jsonDirectory = jsonDirectory;
            DCSBIOSControlLocator.JSONDirectory = jsonDirectory;
            _lastProfileUsed = lastProfileUsed;
        }

        public void OpenProfile()
        {
            if (IsDirty && MessageBox.Show("Discard unsaved changes to current profile?", "Discard changes?", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
            {
                return;
            }
            var tempDirectory = string.IsNullOrEmpty(Filename) ? MyDocumentsPath() : Path.GetFullPath(Filename);
            ClearAll();
            var openFileDialog = new OpenFileDialog();
            openFileDialog.RestoreDirectory = true;
            openFileDialog.InitialDirectory = tempDirectory;
            openFileDialog.FileName = OpenFileDialogFileName;
            openFileDialog.DefaultExt = OpenFileDialogDefaultExt;
            openFileDialog.Filter = OpenFileDialogFilter;
            if (openFileDialog.ShowDialog() == true)
            {
                LoadProfile(openFileDialog.FileName);
            }
        }

        public void PanelSettingsReadFromFile(object sender, SettingsReadFromFileEventArgs e)
        {
            try
            {
                //todo do nada
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(1050, ex);
            }
        }

        public void PanelSettingsChanged(object sender, PanelEventArgs e)
        {
            try
            {
                Common.DebugP("Settings changed for " + e.SaitekPanelEnum + "   " + e.UniqueId);
                IsDirty = true;
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(1051, ex);
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
            //This sends info to all to clear their settings
            OnClearPanelSettings?.Invoke(this);
        }

        public void ClearAll()
        {
            _listPanelSettingsData.Clear();
            _profileFileInstanceIDs.Clear();
        }

        public string DefaultFile()
        {
            return Path.GetFullPath((Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments))) + "\\" + "Saitek_DCS_Profile.bindings";
        }

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
            {
                Common.DebugP("LoadProfile filename : " + filename);
                if (!string.IsNullOrEmpty(filename))
                {
                    _filename = filename;
                    _lastProfileUsed = filename;
                }
                /*
                 * 0 Open specified filename (parameter) if not null
                 * 1 If exists open last profile used (settings)
                 * 2 Try and open default profile located in My Documents
                 * 3 If none found create default file
                 */
                _isNewProfile = false;
                ClearAll();
                if (string.IsNullOrEmpty(_filename))
                {
                    if (!string.IsNullOrEmpty(_lastProfileUsed) && File.Exists(_lastProfileUsed))
                    {
                        _filename = _lastProfileUsed;
                    }
                    else if (File.Exists(DefaultFile()))
                    {
                        _filename = DefaultFile();
                        _lastProfileUsed = filename;
                    }
                }

                Common.DebugP("LoadProfile _lastProfileUsed : " + _lastProfileUsed);
                Common.DebugP("LoadProfile _filename : " + _filename);

                if (string.IsNullOrEmpty(_filename) || !File.Exists(_filename))
                {
                    //Main window will handle this
                    Common.DebugP("LoadProfile returns false");
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
                var fileLines = File.ReadAllLines(_filename);
                SaitekPanelsEnum currentPanelType = SaitekPanelsEnum.Unknown;
                string currentPanelInstanceID = null;
                string currentPanelSettingsVersion = null;
                var insidePanel = false;
                const string sepString = "\\o/";

                foreach (var fileLine in fileLines)
                {
                    if (fileLine.StartsWith("Airframe="))
                    {
                        if (fileLine.StartsWith("Airframe=NONE"))
                        {
                            //Backward compat
                            _airframe = DCSAirframe.KEYEMULATOR;
                        }
                        else
                        {
                            _airframe = (DCSAirframe)Enum.Parse(typeof(DCSAirframe), fileLine.Replace("Airframe=", "").Trim());
                        }
                        DCSBIOSControlLocator.Airframe = _airframe;
                        DCSBIOSControlLocator.JSONDirectory = _jsonDirectory;
                    }else if (fileLine.StartsWith("OperationLevelFlag="))
                    {
                        Common.SetOperationModeFlag(int.Parse(fileLine.Replace("OperationLevelFlag=", "").Trim()));
                    }
                    else if (!fileLine.StartsWith("#") && fileLine.Length > 2)
                    {
                        //Process all these lines.
                        if (fileLine.StartsWith("PanelType="))
                        {
                            currentPanelType = (SaitekPanelsEnum)Enum.Parse(typeof(SaitekPanelsEnum), fileLine.Replace("PanelType=", "").Trim());
                        }
                        else if (fileLine.StartsWith("PanelInstanceID="))
                        {
                            currentPanelInstanceID = fileLine.Replace("PanelInstanceID=", "").Trim();
                            _profileFileInstanceIDs.Add(new KeyValuePair<string, SaitekPanelsEnum>(currentPanelInstanceID, currentPanelType));
                        }
                        else if (fileLine.StartsWith("PanelSettingsVersion="))
                        {
                            currentPanelSettingsVersion = fileLine.Trim();
                        }
                        else if (fileLine.StartsWith("BeginPanel"))
                        {
                            insidePanel = true;
                        }
                        else if (fileLine.StartsWith("EndPanel"))
                        {
                            insidePanel = false;
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
                                if (currentPanelSettingsVersion != null)
                                {
                                    //0X marks that setting version isn't used (yet). Any number above 0 indicated the panel are using new versions of the settings
                                    //and that old settings won't be loaded.
                                    if (currentPanelSettingsVersion.EndsWith("0X"))
                                    {
                                        _listPanelSettingsData.Add(line + sepString + currentPanelInstanceID);
                                    }
                                    else
                                    {
                                        _listPanelSettingsData.Add(line + sepString + currentPanelInstanceID + sepString + currentPanelSettingsVersion);
                                    }
                                }
                                else
                                {
                                    _listPanelSettingsData.Add(line + sepString + currentPanelInstanceID);
                                }
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
                Common.ShowErrorMessageBox(1061, ex);
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
            foreach (var saitekPanelSkeleton in Common.SaitekPanelSkeletons)
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
                            Common.ShowErrorMessageBox(7931061, ex);
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
                        OnUserMessageEventHandler(this, new UserMessageEventArgs(){UserMessage = "The " + profileFileInstanceID.Value + " panel with USB Instance ID :" + Environment.NewLine + profileFileInstanceID.Key + Environment.NewLine + "cannot be found. Have you rearranged your panels (USB ports) or have you copied someone elses profile?" + Environment.NewLine + "Use the ID button to copy current Instance ID and replace the faulty one in the profile file."});
                    }
                }
            }
        }

        public void SendSettingsReadEvent()
        {
            try
            {
                Common.DebugP("ProfileHandler Sends OnAirframeSelected & OnSettingsReadFromFile event");
                if (OnSettingsReadFromFile == null)
                {
                    Common.DebugP("ProfileHandler : no one is listening to OnSettingsReadFromFile?");
                }
                if (OnAirframeSelected == null)
                {
                    Common.DebugP("ProfileHandler : no one is listening to OnAirframeSelected?");
                }
                if (OnSettingsReadFromFile != null)
                {
                    //TODO DENNA ORSAKAR HÄNGANDE!!
                    OnAirframeSelected?.Invoke(this, new AirframeEventArgs(){Airframe =  _airframe});
                    //TODO DENNA ORSAKAR HÄNGANDE!!
                    OnSettingsReadFromFile(this, new SettingsReadFromFileEventArgs(){Settings = _listPanelSettingsData});
                }
            }
            catch (Exception e)
            {
                Common.ShowErrorMessageBox(12061, e);
            }
        }

        public bool SaveAsNewProfile()
        {
            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.RestoreDirectory = true;
            saveFileDialog.InitialDirectory = MyDocumentsPath();
            saveFileDialog.FileName = "Saitek_DCS_Profile.bindings";
            saveFileDialog.DefaultExt = OpenFileDialogDefaultExt;
            saveFileDialog.Filter = OpenFileDialogFilter;
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
            OnSavePanelSettings?.Invoke(this, new ProfileHandlerEventArgs(){ProfileHandlerEA =  this});
        }

        public bool IsNewProfile => _isNewProfile;

        public string Filename
        {
            get => _filename;
            set => _filename = value;
        }

        public void RegisterProfileData(SaitekPanel saitekPanel, List<string> strings)
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
                    _listPanelSettingsData.Add("PanelType=" + saitekPanel.TypeOfSaitekPanel);
                    _listPanelSettingsData.Add("PanelInstanceID=" + saitekPanel.InstanceId);
                    _listPanelSettingsData.Add("PanelSettingsVersion=" + saitekPanel.SettingsVersion());
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
                Common.ShowErrorMessageBox(1062, ex);
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
                Common.ShowErrorMessageBox(1063, ex);
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

        public DCSAirframe Airframe
        {
            get => _airframe;
            set
            {
                if (value != _airframe)
                {
                    _isDirty = true;
                }
                _airframe = value;

                SetOperationLevelFlag();
                OnAirframeSelected?.Invoke(this, new AirframeEventArgs() {Airframe = _airframe});
            }
        }

        public string LastProfileUsed
        {
            get => _lastProfileUsed;
            set => _lastProfileUsed = value;
        }

        public void SelectedAirframe(object sender, AirframeEventArgs e)
        {
            try
            {
                //nada
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(471473, ex);
            }
        }
        public string JSONDirectory
        {
            get => _jsonDirectory;
            set => _jsonDirectory = value;
        }

        public bool ProfileLoaded => _profileLoaded || _isNewProfile;


        public bool UseNS430
        {
            get => Common.IsOperationModeFlagSet(OperationFlag.NS430Enabled);
            set {
                if (value)
                {
                    Common.SetOperationModeFlag(OperationFlag.NS430Enabled);
                }
                else
                {
                    Common.ClearOperationModeFlag(OperationFlag.NS430Enabled);
                }
                _isDirty = true;
            }
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
