namespace DCS_BIOS
{
    using System;

    [Serializable]
    public static class DCSBIOSInputHandler
    {
        public static DCSBIOSInput GetDCSBIOSInput(string controlId)
        {
            var result = new DCSBIOSInput();
            var control = DCSBIOSControlLocator.GetControl(controlId);
            result.Consume(control);
            return result;
        }
    }
}
