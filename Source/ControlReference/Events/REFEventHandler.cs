using ControlReference.Interfaces;

namespace ControlReference.Events
{
    internal static class REFEventHandler
    {
        public delegate void SetCategoryEventHandler(object sender, CategoryEventArgs e);
        public static event SetCategoryEventHandler OnChangeCategory;
        
        public static void AttachDataListener(ICategoryChange categoryChange)
        {
            OnChangeCategory += categoryChange.ChangeCategory;
        }
        
        public static void DetachDataListener(ICategoryChange categoryChange)
        {
            OnChangeCategory -= categoryChange.ChangeCategory;
        }

        public static void ChangeCategory(object sender, string category)
        {
            OnChangeCategory?.Invoke(sender, new CategoryEventArgs {Sender  = sender, Category = category});
        }
    }
}
