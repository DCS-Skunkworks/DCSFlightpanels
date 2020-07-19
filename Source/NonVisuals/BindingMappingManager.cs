using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using ClassLibraryCommon;
using HidLibrary;

namespace NonVisuals
{
    public static class BindingMappingManager
    {
        private static volatile List<GenericPanelBinding> _genericBindings = new List<GenericPanelBinding>();

        public static void AddBinding(GenericPanelBinding genericBinding)
        {
            if (genericBinding != null)
            {
                if (Exists(genericBinding))
                {
                    foreach (var binding in _genericBindings)
                    {
                        if (binding.BindingHash == genericBinding.BindingHash)
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

            Debug.WriteLine("Count is " + _genericBindings.Count);
        }

        public static bool Exists(GenericPanelBinding genericPanelBinding)
        {
            foreach (var binding in _genericBindings)
            {
                if (binding.HIDInstance == genericPanelBinding.HIDInstance)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool VerifyBindings(ref bool settingsWereModified)
        {
            foreach (var genericBinding in _genericBindings)
            {
                var found = false;

                foreach (var hidSkeleton in HIDHandler.GetInstance().HIDSkeletons)
                {
                    if (genericBinding.HIDInstance == hidSkeleton.InstanceId)
                    {
                        genericBinding.HardwareWasFound = true;
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    genericBinding.HardwareWasFound = false;
                }
            }

            var problemsPersists = false;
            foreach (var genericBinding in _genericBindings)
            {
                if (genericBinding.HardwareWasFound == false)
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
                            default:
                                {
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

        public static bool FindSolution(GenericPanelBinding genericBinding, ref bool settingsWereModified)
        {
            settingsWereModified = false;

            /*
             * 1) Check, are there multiple such panels where hardware does not match? If so user must map them
             *    If only 1, then we can map it without asking questions.
             */
            var count = _genericBindings.FindAll(o => (o.HardwareWasFound == false) && (o.PanelType == genericBinding.PanelType)).Count;
            if (count == 1)
            {
                //This we can map ourselves!
                var hidSkeleton = HIDHandler.GetInstance().HIDSkeletons.Find(o => o.PanelInfo.GamingPanelType == genericBinding.PanelType);
                genericBinding.HIDInstance = hidSkeleton.InstanceId;
                settingsWereModified = true;
                MessageBox.Show("USB settings has changed. Please save the profile.", "USB changes found", MessageBoxButton.OK, MessageBoxImage.Information);
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

        public static void AskUser()
        {
            /*
             * 1) Check, are there multiple such panels where hardware does not match? If so user must map them
             *    If only 1, then we can map it without asking questions.
             * 2) Hardware missing, then it will be left as is and saved next time user saves bindings.
             */
            foreach (var bindingsGrouped in _genericBindings.GroupBy(o => o.PanelType)
                .Select(group => new
                {
                    PanelType = group.Key,
                    Count = group.Count()
                })
                .OrderBy(x => x.PanelType))
            {
                //if(bindingsGrouped.Count
            }
            //var count = _genericBindings.FindAll(o => (o.HardwareWasFound == false) && (o.PanelType == genericBinding.PanelType)).GroupBy(o => o.PanelType);
            /*if (count == 1)
            {
                //This we can map ourselves!
                var hidSkeleton = HIDHandler.GetInstance().HIDSkeletons.Find(o => o.PanelInfo.GamingPanelType == genericBinding.PanelType);
                genericBinding.HIDInstance = hidSkeleton.InstanceId;
            }
            else
            {
                //Askuser
            }*/
        }

        private static void CheckAllProfileInstanceIDsAgainstAttachedHardware()
        {
            /*
            foreach (var saitekPanelSkeleton in Common.GamingPanelSkeletons)
            {
                foreach (var hidDevice in HidDevices.Enumerate(saitekPanelSkeleton.VendorId, saitekPanelSkeleton.ProductId))
                {
                    if (hidDevice != null)
                    {
                        try
                        {
                            //_profileFileInstanceIDs.RemoveAll(item => item.Key.Equals(hidDevice.DevicePath));
                        }
                        catch (Exception ex)
                        {
                            Common.ShowErrorMessageBox(ex);
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
            }*/
        }
    }
}
