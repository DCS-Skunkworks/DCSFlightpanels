using System.Linq;
using NonVisuals.EventArgs;

namespace NonVisuals
{
    using System.Collections.Generic;
    using System.Windows;

    using ClassLibraryCommon;

    public static class BindingMappingManager
    {
        private static volatile List<GenericPanelBinding> _genericBindings = new List<GenericPanelBinding>();


        public static void ClearBindings()
        {
            _genericBindings.Clear();
        }

        public static bool UnusedBindingsExists()
        {
            return _genericBindings.Any(o => o.InUse == false);
        }

        public static void SetNotInUse(HIDSkeleton hidSkeleton)
        {
            _genericBindings.FindAll(o => o.Match(hidSkeleton)).ToList().ForEach(u => u.InUse = false);
        }

        public static void SendBinding(HIDSkeleton hidSkeleton)
        {
            foreach (var genericPanelBinding in _genericBindings)
            {
                if (genericPanelBinding.Match(hidSkeleton))
                {
                    genericPanelBinding.InUse = true;
                    AppEventHandler.ProfileEvent(null, ProfileEventEnum.ProfileSettings, genericPanelBinding, DCSFPProfile.SelectedProfile);
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

                    _genericBindings.Add(genericBinding);
                }
            }
        }

        public static void AddBinding(GenericPanelBinding genericBinding)
        {
            if (genericBinding != null)
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

        public static bool Exists(GenericPanelBinding genericPanelBinding)
        {
            foreach (var binding in _genericBindings)
            {
                if (binding.Match(genericPanelBinding))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool VerifyBindings(ref bool settingsWereModified)
        {
            var problemsPersists = false;
            foreach (var genericBinding in _genericBindings)
            {
                if (genericBinding.InUse == false)
                {
                    if (!FindSolution(genericBinding, ref settingsWereModified))
                    {
                        problemsPersists = true;
                    }
                }
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
            var count = _genericBindings.FindAll(o => (o.InUse == false) && (o.PanelType == genericBinding.PanelType)).Count;
            if (count == 1)
            {
                var hidSkeleton = HIDHandler.GetInstance().HIDSkeletons.Find(o => o.PanelInfo.GamingPanelType == genericBinding.PanelType);
                if (hidSkeleton != null)
                {
                    // This we can map ourselves!
                    genericBinding.HIDInstance = hidSkeleton.InstanceId;
                    settingsWereModified = true;
                    genericBinding.InUse = true;
                    MessageBox.Show("USB settings has changed. Please save the profile.", "USB changes found", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Reference found in bindings file to a " + genericBinding.PanelType.GetEnumDescriptionField() + ", no such hardware found.", "Hardware missing", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }

                return true;
            }

            return false;
        }

        public static GenericPanelBinding GetBinding(GamingPanel gamingPanel)
        {
            foreach (var genericPanelBinding in _genericBindings)
            {
                if (genericPanelBinding.BindingHash == gamingPanel.BindingHash)
                {
                    return genericPanelBinding;
                }
            }

            return null;
        }

        public static List<GenericPanelBinding> PanelBindings => _genericBindings;

    }
}
