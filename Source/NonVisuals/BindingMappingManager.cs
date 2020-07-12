using System.Collections.Generic;
using System.Linq;

namespace NonVisuals
{
    public static class BindingMappingManager
    {
        private static List<GenericBinding> _genericBindings = new List<GenericBinding>();

        public static void AddBinding(GenericBinding genericBinding)
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

        public static bool FindSolution(GenericBinding genericBinding)
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

        public static void AskUser()
        {
            /*
             * 1) Check, are there multiple such panels where hardware does not match? If so user must map them
             *    If only 1, then we can map it without asking questions.
             * 2) Hardware missing, then it will be left as is and saved next time user saves bindings.
             */
            var count = _genericBindings.FindAll(o => (o.HardwareWasFound == false) && (o.PanelType == genericBinding.PanelType)).GroupBy(o => o.PanelType);
            if (count == 1)
            {
                //This we can map ourselves!
                var hidSkeleton = HIDHandler.GetInstance().HIDSkeletons.Find(o => o.PanelInfo.GamingPanelType == genericBinding.PanelType);
                genericBinding.HIDInstance = hidSkeleton.InstanceId;
            }
            else
            {
                //Askuser
            }
        }
    }
}
