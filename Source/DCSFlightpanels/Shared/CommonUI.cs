using System.Windows;

namespace DCSFlightpanels.Shared
{
    public static class CommonUI
    {
        public static bool DoDiscardAfterMessage(bool isDirty)
        {
            return DoDiscardAfterMessage(isDirty, "Discard changes?");
        }

        public static bool DoDiscardAfterMessage(bool isDirty, string question)
        {
            // Defaults to true so only if "is dirty" and "do not discard" will result change
            if (isDirty)
            {
                return MessageBox.Show(question, "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
            }
            return true;
        }

    }
}
