using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Navigation;
using ClassLibraryCommon;
using HidLibrary;

namespace NonVisuals
{
    public static class BindingMappingManager
    {
        private static List<GenericPanelBinding> _genericBindings = new List<GenericPanelBinding>();

        public static void AddBinding(GenericPanelBinding genericBinding)
        {
            if (genericBinding != null)
            {
                _genericBindings.Add(genericBinding);
            }
        }

        public static void VerifyBindings()
        {
            foreach (var genericBinding in _genericBindings)
            {
                var found = false;

                foreach (var hidSkeleton in HIDHandler.GetInstance().HIDSkeletons)
                {
                    if (genericBinding.HIDInstance == hidSkeleton.InstanceId)
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    genericBinding.HardwareWasFound = false;
                }
            }
        }

        public static bool FindSolution(GenericPanelBinding genericBinding)
        {
            /*
             * 1) Check, are there multiple such panels where hardware does not match? If so user must map them
             *    If only 1, then we can map it without asking questions.
             * 2) Hardware missing, then it will be left as is and saved next time user saves bindings.
             */
            var count = _genericBindings.FindAll(o => (o.HardwareWasFound == false) && (o.PanelType == genericBinding.PanelType)).Count;
            if (count == 1)
            {
                //This we can map ourselves!
                var hidSkeleton = HIDHandler.GetInstance().HIDSkeletons.Find(o => o.PanelInfo.GamingPanelType == genericBinding.PanelType);
                genericBinding.HIDInstance = hidSkeleton.InstanceId;
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
                .Select(group => new {
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
            /*if (_profileFileInstanceIDs.Count > 0)
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
