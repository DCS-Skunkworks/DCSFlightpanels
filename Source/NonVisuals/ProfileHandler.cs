namespace NonVisuals
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using System.Windows;

    using ClassLibraryCommon;

    using DCS_BIOS;

    using MEF;

    using Microsoft.Win32;

    using NonVisuals.EventArgs;
    using NonVisuals.Interfaces;
    using NonVisuals.Properties;

    public class ProfileHandler : ISettingsModifiedListener, IIsDirty, IDisposable
    {
        private const string OPEN_FILE_DIALOG_FILE_NAME = "*.bindings";
        private const string OPEN_FILE_DIALOG_DEFAULT_EXT = ".bindings";
        private const string OPEN_FILE_DIALOG_FILTER = "DCSFlightpanels (.bindings)|*.bindings";

        private static DCSFPProfile _dcsfpProfile = DCSFPProfile.GetNoFrameLoadedYet();

        private readonly List<KeyValuePair<string, GamingPanelEnum>> _profileFileInstanceIDs = new List<KeyValuePair<string, GamingPanelEnum>>();
        private readonly object _lockObject = new object();

        // Both directory and filename
        private string _filename = Path.GetFullPath(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)) + "\\" + "dcsfp_profile.bindings";
        private string _lastProfileUsed = string.Empty;
        private bool _isDirty;
        private bool _isNewProfile;
        private readonly string _dcsbiosJSONDirectory; // hunting weird bug
        private bool _profileLoaded;

        private IHardwareConflictResolver _hardwareConflictResolver;

        public static DCSFPProfile ActiveDCSFPProfile
        {
            get => _dcsfpProfile;
        }

        public ProfileHandler(string dcsbiosJSONDirectory, IHardwareConflictResolver hardwareConflictResolver)
        {
            _dcsbiosJSONDirectory = dcsbiosJSONDirectory;
            _hardwareConflictResolver = hardwareConflictResolver;
            AppEventHandler.AttachSettingsModified(this);
        }

        public ProfileHandler(string dcsbiosJSONDirectory, string lastProfileUsed, IHardwareConflictResolver hardwareConflictResolver)
        {
            _dcsbiosJSONDirectory = dcsbiosJSONDirectory;
            _hardwareConflictResolver = hardwareConflictResolver;
            _lastProfileUsed = lastProfileUsed;
            AppEventHandler.AttachSettingsModified(this);
        }

        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                AppEventHandler.DetachSettingsModified(this);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Init()
        {
            DCSBIOSControlLocator.JSONDirectory = _dcsbiosJSONDirectory;
        }

        public string OpenProfile()
        {
            if (IsDirty && MessageBox.Show("Discard unsaved changes to current profile?", "Discard changes?", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
            {
                return null;
            }

            var tempDirectory = string.IsNullOrEmpty(Settings.Default.LastImageFileDialogLocation) ? Constants.PathRootDriveC : Settings.Default.LastImageFileDialogLocation;
            ClearAll();
            var openFileDialog = new OpenFileDialog
            {
                RestoreDirectory = true,
                InitialDirectory = tempDirectory,
                FileName = OPEN_FILE_DIALOG_FILE_NAME,
                DefaultExt = OPEN_FILE_DIALOG_DEFAULT_EXT,
                Filter = OPEN_FILE_DIALOG_FILTER
            };
            if (openFileDialog.ShowDialog() == true)
            {
                Settings.Default.LastImageFileDialogLocation = Path.GetDirectoryName(openFileDialog.FileName);
                Settings.Default.Save();
                return openFileDialog.FileName;
            }

            return null;
        }

        public void NewProfile()
        {
            if (IsDirty && MessageBox.Show("Discard unsaved changes to current profile?", "Discard changes?", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
            {
                return;
            }

            _isNewProfile = true;
            ClearAll();
            Profile = DCSFPProfile.GetNoFrameLoadedYet(); // Just a default that doesn't remove non emulation panels from the GUI

            // This sends info to all to clear their settings
            AppEventHandler.ClearPanelSettings(this);
        }

        public void SettingsModified(object sender, PanelInfoArgs e)
        {
            try
            {
                IsDirty = true;
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }
        
        public void ClearAll()
        {
            _profileFileInstanceIDs.Clear();
        }

        /*public string DefaultFile()
        {
            return Path.GetFullPath((Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments))) + "\\" + "dcsfp_profile.bindings";
        }*/
        public bool ReloadProfile()
        {
            return LoadProfile(null);
        }

        public bool LoadProfile(string filename)
        {
            try
            {
                /*
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
                    // Main window will handle this
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
                Debug.WriteLine($"ProfileHandler reading file {_filename}");
                var fileLines = File.ReadAllLines(_filename);
                var currentPanelType = GamingPanelEnum.Unknown;
                string currentPanelInstanceID = null;
                string currentBindingHash = null;
                var insidePanel = false;
                var insideJSONPanel = false;
                DCSFPProfile tmpProfile = null;
                GenericPanelBinding genericPanelBinding = null;

                foreach (var fileLine in fileLines)
                {
                    if (fileLine.StartsWith("Airframe="))
                    {
                        // <== Backward compability
                        if (fileLine.StartsWith("Airframe=NONE"))
                        {
                            // Backward compability
                            tmpProfile = DCSFPProfile.GetKeyEmulator();
                        }
                        else
                        {
                            // Backward compability
                            var airframeAsString = fileLine.Replace("Airframe=", string.Empty).Trim();
                            tmpProfile = DCSFPProfile.GetBackwardCompatible(airframeAsString);
                        }
                    }
                    else if (fileLine.StartsWith("Profile="))
                    {
                        tmpProfile = DCSFPProfile.GetProfile(int.Parse(fileLine.Replace("Profile=", string.Empty)));
                    }
                    else if (fileLine.StartsWith("OperationLevelFlag="))
                    {
                        Common.SetEmulationModesFlag(int.Parse(fileLine.Replace("OperationLevelFlag=", string.Empty).Trim())); // backward compat 13.03.2021
                    }
                    else if (fileLine.StartsWith("EmulationModesFlag="))
                    {
                        Common.SetEmulationModesFlag(int.Parse(fileLine.Replace("EmulationModesFlag=", string.Empty).Trim()));
                    }
                    else if (fileLine.StartsWith("UseGenericRadio="))
                    {
                        tmpProfile.UseGenericRadio = bool.Parse(fileLine.Replace("UseGenericRadio=", string.Empty).Trim());
                    }
                    else if (!fileLine.StartsWith("#") && fileLine.Length > 0)
                    {
                        // Process all these lines.
                        if (fileLine.StartsWith("PanelType="))
                        {
                            currentPanelType = (GamingPanelEnum)Enum.Parse(typeof(GamingPanelEnum), fileLine.Replace("PanelType=", string.Empty).Trim());
                            genericPanelBinding = new GenericPanelBinding
                            {
                                PanelType = currentPanelType
                            };
                        }
                        else if (fileLine.StartsWith("PanelInstanceID="))
                        {
                            currentPanelInstanceID = fileLine.Replace("PanelInstanceID=", string.Empty).Trim();
                            genericPanelBinding.HIDInstance = currentPanelInstanceID;
                            _profileFileInstanceIDs.Add(new KeyValuePair<string, GamingPanelEnum>(currentPanelInstanceID, currentPanelType));
                        }
                        else if (fileLine.StartsWith("BindingHash="))
                        {
                            currentBindingHash = fileLine.Replace("BindingHash=", string.Empty).Trim();
                            genericPanelBinding.BindingHash = currentBindingHash;
                        }
                        else if (fileLine.StartsWith("PanelSettingsVersion="))
                        {
                            // do nothing, this will be phased out
                        }
                        else if (fileLine.Equals("BeginPanel"))
                        {
                            insidePanel = true;
                        }
                        else if (fileLine.Equals("EndPanel"))
                        {
                            if (genericPanelBinding != null)
                            {
                                BindingMappingManager.RegisterBindingFromFile(genericPanelBinding);
                            }

                            insidePanel = false;
                        }
                        else if (fileLine.Equals("BeginPanelJSON"))
                        {
                            insideJSONPanel = true;
                        }
                        else if (fileLine.Equals("EndPanelJSON"))
                        {
                            if (genericPanelBinding != null)
                            {
                                BindingMappingManager.RegisterBindingFromFile(genericPanelBinding);
                            }

                            insideJSONPanel = false;
                        }
                        else
                        {
                            if (insidePanel)
                            {
                                var line = fileLine;
                                if (line.StartsWith("\t"))
                                {
                                    line = line.Replace("\t", string.Empty);
                                }

                                genericPanelBinding.Settings.Add(line);
                            }

                            if (insideJSONPanel)
                            {
                                genericPanelBinding.JSONAddLine(fileLine);
                            }
                        }
                    }
                }

                // For backwards compability 10.11.2018
                if (Common.GetEmulationModesFlag() == 0)
                {
                    SetEmulationModeFlag();
                }

                Profile = tmpProfile;
                CheckHardwareConflicts();

                SendBindingsReadEvent();
                return true;
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
                return false;
            }
        }

        private void CheckHardwareConflicts()
        {
            var settingsWereModified = false;
            if (!BindingMappingManager.VerifyBindings(ref settingsWereModified))
            {
                var modifiedBindings = _hardwareConflictResolver.ResolveConflicts();
                BindingMappingManager.MergeModifiedBindings(modifiedBindings);
                settingsWereModified = modifiedBindings != null && modifiedBindings.Count > 0;
            }

            if (settingsWereModified)
            {
                SetIsDirty();
            }
        }

        private void SetEmulationModeFlag()
        {
            if (DCSFPProfile.IsKeyEmulator(Profile))
            {
                Common.SetEmulationModes(EmulationMode.KeyboardEmulationOnly);
            }
            else if (DCSFPProfile.IsFlamingCliff(Profile))
            {
                Common.SetEmulationModes(EmulationMode.KeyboardEmulationOnly);
            }
            else
            {
                Common.SetEmulationModes(EmulationMode.DCSBIOSOutputEnabled | EmulationMode.DCSBIOSInputEnabled);
            }
        }

        public void SendBindingsReadEvent()
        {
            try
            {
                AppEventHandler.AirframeSelected(this, _dcsfpProfile);

                foreach (var genericPanelBinding in BindingMappingManager.PanelBindings)
                {
                    try
                    {
                        AppEventHandler.ProfileLoaded(this, genericPanelBinding);
                    }
                    catch (Exception ex)
                    {
                        Common.ShowErrorMessageBox(ex, $"Error reading settings. Panel : {genericPanelBinding.PanelType}");
                    }
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        public void SendRadioSettings()
        {
            try
            {
                foreach (var genericPanelBinding in BindingMappingManager.PanelBindings)
                {
                    try
                    {
                        if (genericPanelBinding.PanelType == GamingPanelEnum.PZ69RadioPanel)
                        {
                            AppEventHandler.ProfileLoaded(this, genericPanelBinding);
                        }
                    }
                    catch (Exception ex)
                    {
                        Common.ShowErrorMessageBox(ex, $"Error reading settings. Panel : {genericPanelBinding.PanelType}");
                    }
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        public bool SaveAsNewProfile()
        {
            var saveFileDialog = new SaveFileDialog
            {
                RestoreDirectory = true,
                InitialDirectory = string.IsNullOrEmpty(Settings.Default.LastImageFileDialogLocation) ? Constants.PathRootDriveC : Settings.Default.LastImageFileDialogLocation,
                FileName = "dcsfp_profile.bindings",
                DefaultExt = OPEN_FILE_DIALOG_DEFAULT_EXT,
                Filter = OPEN_FILE_DIALOG_FILTER,
                OverwritePrompt = true
            };
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
            AppEventHandler.SavePanelSettings(this);
            AppEventHandler.SavePanelSettingsJSON(this);
        }

        public bool IsNewProfile => _isNewProfile;

        public string Filename
        {
            get => _filename;
            set => _filename = value;
        }

        public void RegisterPanelBinding(GamingPanel gamingPanel, List<string> strings)
        {
            try
            {
                lock (_lockObject)
                {
                    var genericPanelBinding = BindingMappingManager.GetBinding(gamingPanel);

                    if (genericPanelBinding == null)
                    {
                        genericPanelBinding = new GenericPanelBinding(gamingPanel.HIDInstanceId, gamingPanel.BindingHash, gamingPanel.TypeOfPanel);
                        BindingMappingManager.AddBinding(genericPanelBinding);
                    }

                    genericPanelBinding.ClearSettings();

                    foreach (var str in strings)
                    {
                        genericPanelBinding.Settings.Add(str);
                    }
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        public void RegisterJSONProfileData(GamingPanel gamingPanel, string jsonData)
        {
            try
            {
                lock (_lockObject)
                {
                    var genericPanelBinding = BindingMappingManager.GetBinding(gamingPanel);

                    if (genericPanelBinding == null)
                    {
                        genericPanelBinding = new GenericPanelBinding(gamingPanel.HIDInstanceId, gamingPanel.BindingHash, gamingPanel.TypeOfPanel);
                        BindingMappingManager.AddBinding(genericPanelBinding);
                    }

                    genericPanelBinding.JSONString = jsonData;
                }
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
            }
        }

        public void SaveProfile()
        {
            try
            {
                // Clear all current settings entries, requesting new ones from the panels
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
                headerStringBuilder.AppendLine("#Supported panels :");
                headerStringBuilder.AppendLine("#   PZ55SwitchPanel");
                headerStringBuilder.AppendLine("#   PZ69RadioPanel");
                headerStringBuilder.AppendLine("#   PZ70MultiPanel");
                headerStringBuilder.AppendLine("#   BackLitPanel");
                headerStringBuilder.AppendLine("#   StreamDeckMini");
                headerStringBuilder.AppendLine("#   StreamDeck");
                headerStringBuilder.AppendLine("#   StreamDeckXL");

                var stringBuilder = new StringBuilder();
                stringBuilder.AppendLine(headerStringBuilder.ToString());
                stringBuilder.AppendLine("#  ***Do not change the location nor content of the line below***");
                stringBuilder.AppendLine("Profile=" + Profile.ID);
                stringBuilder.AppendLine("EmulationModesFlag=" + Common.GetEmulationModesFlag());
                stringBuilder.AppendLine("UseGenericRadio=" + Profile.UseGenericRadio + Environment.NewLine);

                foreach (var genericPanelBinding in BindingMappingManager.PanelBindings)
                {
                    if (!genericPanelBinding.HasBeenDeleted)
                    {
                        stringBuilder.AppendLine(genericPanelBinding.ExportBinding());
                    }
                }

                stringBuilder.AppendLine(GetFooter());

                File.WriteAllText(_filename, stringBuilder.ToString(), Encoding.ASCII);
                _isDirty = false;
                _isNewProfile = false;
                LoadProfile(_filename);
            }
            catch (Exception ex)
            {
                Common.ShowErrorMessageBox(ex);
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
            set
            {
                _isDirty = value;
            }
        }

        public void SetIsDirty()
        {
            IsDirty = true;
        }

        public static DCSFPProfile SelectedProfile()
        {
            return _dcsfpProfile;
        }

        public DCSFPProfile Profile
        {
            get => _dcsfpProfile;
            set
            {
                // Called only when user creates a new profile
                if (!DCSFPProfile.IsNoFrameLoadedYet(_dcsfpProfile) && value != _dcsfpProfile)
                {
                    SetIsDirty();
                }

                _dcsfpProfile = value;
                Common.ResetEmulationModesFlag();
                SetEmulationModeFlag();
                DCSBIOSControlLocator.Profile = Profile;
                AppEventHandler.AirframeSelected(this, _dcsfpProfile);
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

        public bool ProfileLoaded => _profileLoaded || _isNewProfile;

        public bool UseNS430
        {
            get => Common.IsEmulationModesFlagSet(EmulationMode.NS430Enabled);
            set
            {
                if (value)
                {
                    Common.SetEmulationModes(EmulationMode.NS430Enabled);
                }
                else
                {
                    Common.ClearEmulationModesFlag(EmulationMode.NS430Enabled);
                }

                SetIsDirty();
            }
        }

        public void StateSaved()
        {
            _isDirty = false;
        }
    }
}
