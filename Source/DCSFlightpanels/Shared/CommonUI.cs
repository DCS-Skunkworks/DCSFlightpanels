using System.Windows;

namespace DCSFlightpanels.Shared
{
    public static class CommonUI
    {
        public static bool DoDiscardAfterMessage(bool isDirty)
        {
            return DoDiscardAfterMessage(isDirty, "Discard changes?");
        }

        public static bool DoDiscardAfterMessage(bool isDirty, string question, string caption = "Confirm")
        {
            /*
             * Defaults to true so only if "is dirty" and "do not discard" will result change
             */
            var result = true;

            if (isDirty)
            {
                result = MessageBox.Show(question, caption, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
            }

            return result;
        }

    }
}
