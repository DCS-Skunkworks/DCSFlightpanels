using ControlReference.Events;

namespace ControlReference.Interfaces
{
    internal interface ICategoryChange
    {
        void ChangeCategory(object sender, CategoryEventArgs args);
    }
}
