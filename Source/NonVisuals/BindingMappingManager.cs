using System.Linq;
using NonVisuals.EventArgs;

namespace NonVisuals
{
    using System.Collections.Generic;
    using System.Windows;

    using ClassLibraryCommon;
    using HID;
    using Panels;

    public static class BindingMappingManager
    {
        private static object _genericBindingsLock = new();
        private static volatile List<GenericPanelBinding> _genericBindings = new();


        public static void ClearBindings()
        {
            lock (_genericBindingsLock)
            {
                _genericBindings.Clear();
            }
        }

        public static bool UnusedBindingsExists()
        {
            lock (_genericBindingsLock)
            {
                return _genericBindings.Any(o => o.InUse == false);
            }
        }

        public static void SetNotInUse(HIDSkeleton hidSkeleton)
        {
            lock (_genericBindingsLock)
            {
                _genericBindings.FindAll(o => o.Match(hidSkeleton)).ToList().ForEach(u => u.InUse = false);
            }
        }

        public static void SendBinding(HIDSkeleton hidSkeleton)
        {
            lock (_genericBindingsLock)
            {
                foreach (var genericPanelBinding in _genericBindings)
                {
                    if (genericPanelBinding.Match(hidSkeleton))
                    {
                        genericPanelBinding.InUse = true;
                        AppEventHandler.ProfileEvent(null, ProfileEventEnum.ProfileSettings, genericPanelBinding,
                            DCSFPProfile.SelectedProfile);
                    }
                }
            }
        }

        public static void SendBinding(string hidInstance)
        {
            lock (_genericBindingsLock)
            {
                var hardwareFound = HIDHandler.GetInstance().HIDSkeletons
                    .Any(o => o.IsAttached && o.HIDInstance.Equals(hidInstance));
                foreach (var genericPanelBinding in _genericBindings)
                {
                    if (genericPanelBinding.HIDInstance.Equals(hidInstance) && genericPanelBinding.InUse == false &&
                        hardwareFound)
                    {
                        genericPanelBinding.InUse = true;
                        AppEventHandler.ProfileEvent(null, ProfileEventEnum.ProfileSettings, genericPanelBinding,
                            DCSFPProfile.SelectedProfile);
                    }
                }
            }
        }

        /*
         * This to be used when loading from file.
         * Checks that bindinghash exists, if not creates it.
         */
        public static void RegisterBindingFromFile(GenericPanelBinding genericBinding)
        {
            if (genericBinding != null)
            {
                if (!Exists(genericBinding))
                {
                    if (string.IsNullOrEmpty(genericBinding.BindingHash))
                    {
                        genericBinding.BindingHash = Common.GetRandomMd5Hash();
                    }

                    lock (_genericBindingsLock)
                    {
                        _genericBindings.Add(genericBinding);
                    }
                }
            }
        }

        public static void AddBinding(GenericPanelBinding genericBinding)
        {
            if (genericBinding != null)
            {
                lock (_genericBindingsLock)
                {
                    if (Exists(genericBinding))
                    {
                        foreach (var binding in _genericBindings)
                        {
                            /*
                             * Tricky considering old profiles that haven't got this property
                             * In the future it should be phased out.
                             */
                            if (string.IsNullOrEmpty(binding.BindingHash))
                            {
                                binding.BindingHash = genericBinding.BindingHash;
                                binding.Settings = genericBinding.Settings;
                            }
                            else if (binding.BindingHash == genericBinding.BindingHash)
                            {
                                binding.Settings = genericBinding.Settings;
                            }
                        }
                    }
                    else
                    {
                        _genericBindings.Add(genericBinding);
                    }
                }
            }
        }

        public static bool Exists(GenericPanelBinding genericPanelBinding)
        {
            lock (_genericBindingsLock)
            {
                foreach (var binding in _genericBindings)
                {
                    if (binding.Match(genericPanelBinding))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool VerifyBindings(ref bool settingsWereModified)
        {
            var problemsPersists = false;
            var settingsWereModifiedLocal = false;


            lock (_genericBindingsLock)
            {
                foreach (var genericBinding in _genericBindings)
                {
                    if (genericBinding.InUse == false)
                    {
                        if (!FindSolution(genericBinding, ref settingsWereModifiedLocal))
                        {
                            problemsPersists = true;
                        }

                        if (settingsWereModifiedLocal)
                        {
                            settingsWereModified = true;
                        }
                    }
                }

                // Here we delete the configurations that may have been marked as deleted by earlier function call FindSolution().
                _genericBindings.RemoveAll(o => o.HasBeenDeleted == true);

            }

            if (problemsPersists)
            {
                return false;
            }

            return true;
        }

        public static void MergeModifiedBindings(List<ModifiedGenericBinding> modifiedGenericBindings)
        {
            bool modificationsMade = false;

            if (modifiedGenericBindings == null || modifiedGenericBindings.Count == 0)
            {
                return;
            }

            foreach (var modifiedGenericBinding in modifiedGenericBindings)
            {
                lock (_genericBindingsLock)
                {
                    for (int i = 0; i < _genericBindings.Count; i++)
                    {
                        var genericBinding = _genericBindings[i];
                        if (modifiedGenericBinding.GenericPanelBinding.BindingHash == genericBinding.BindingHash)
                        {
                            switch (modifiedGenericBinding.State)
                            {
                                case GenericBindingStateEnum.New:
                                {
                                    AddBinding(modifiedGenericBinding.GenericPanelBinding);
                                    modificationsMade = true;
                                    break;
                                }

                                case GenericBindingStateEnum.Modified:
                                {
                                    genericBinding.HIDInstance = modifiedGenericBinding.GenericPanelBinding.HIDInstance;
                                    genericBinding.Settings = modifiedGenericBinding.GenericPanelBinding.Settings;
                                    modificationsMade = true;
                                    break;
                                }

                                case GenericBindingStateEnum.Deleted:
                                {
                                    genericBinding.HasBeenDeleted = true;
                                    modificationsMade = true;
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            if (modificationsMade)
            {
                MessageBox.Show("USB settings has changed in the bindings file. Please save the profile and verify functionality.", "Save & Restart", MessageBoxButton.OK, MessageBoxImage.Information);

            }
        }

        // PROCESSING OF BIPLINKS AND STREAM LINKS STILL MISSING
        public static bool FindSolution(GenericPanelBinding genericBinding, ref bool settingsWereModified)
        {
            settingsWereModified = false;

            /*
             * 1) Check, are there multiple such panels where hardware does not match? If so user must map them
             *    If only 1, then we can map it without asking questions.
             */
            int count;
            lock (_genericBindingsLock)
            {
                count = _genericBindings
                    .FindAll(o => (o.InUse == false) && (o.PanelType == genericBinding.PanelType)).Count;
            }

            if (count == 1)
            {
                var hidSkeleton = HIDHandler.GetInstance().HIDSkeletons.Find(o => o.PanelInfo.GamingPanelType == genericBinding.PanelType && o.IsAttached);
                if (hidSkeleton != null)
                {
                    // This we can map ourselves!
                    genericBinding.HIDInstance = hidSkeleton.HIDInstance;
                    settingsWereModified = true;
                    MessageBox.Show("USB settings has changed. Please save the profile.", "USB changes found", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show($"Reference found in bindings file to a {genericBinding.PanelType.GetEnumDescriptionField()}, no such hardware found.", "Hardware missing", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    if (MessageBox.Show(
                            $"Do you want to remove the configuration(s) for the {genericBinding.PanelType.GetEnumDescriptionField()} ?", 
                            "Delete configuration",
                            MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        settingsWereModified = true;
                        genericBinding.HasBeenDeleted = true;

                        MessageBox.Show(
                            "Profile has been changed, please save the profile.",
                            "Panel Removed", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }

                return true;
            }

            return false;
        }


        public static GenericPanelBinding GetBinding(GamingPanel gamingPanel)
        {
            lock (_genericBindingsLock)
            {
                foreach (var genericPanelBinding in _genericBindings)
                {
                    if (genericPanelBinding.BindingHash == gamingPanel.BindingHash)
                    {
                        return genericPanelBinding;
                    }
                }
            }

            return null;
        }

        public static List<GenericPanelBinding> PanelBindings
        {
            get
            {
                lock (_genericBindingsLock)
                {
                    return _genericBindings;
                }
            }
        }
    }
}
