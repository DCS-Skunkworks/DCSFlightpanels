using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DCSFlightpanels.Bills;

namespace DCSFlightpanels.CustomControls
{
    public class StreamDeckImage : Image
    {
        private bool _isSelected;

        public BillStreamDeckFace Bill { get; set; }
        public bool IsSelected {
            get { 
                return _isSelected;           
            }
            set {
                _isSelected = value;
                Border borderParent = GTVisualTreeHelper.FindVisualParent<Border>(this);
                borderParent.Style = _isSelected ? (Style)FindResource("BorderSelected") : (Style)FindResource("BorderMouseHoover");
            }
        
        }
    }

    public static class GTVisualTreeHelper
    {
        //Finds the visual parent.
        public static T FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            if (child == null)
            {
                return (null);
            }

            //get parent item
            DependencyObject parentObj = VisualTreeHelper.GetParent(child);

            //we've reached the end of the tree
            if (parentObj == null) return null;

            // check if the parent matches the type we are requested
            T parent = parentObj as T;

            if (parent != null)
            {
                return parent;
            }
            else
            {
                // here, To find the next parent in the tree. we are using recursion until we found the requested type or reached to the end of tree.
                return FindVisualParent<T>(parentObj);
            }
        }
    }
}
