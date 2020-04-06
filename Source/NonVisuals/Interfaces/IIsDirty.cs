using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NonVisuals.Interfaces
{
    public interface IIsDirty
    {
        void SetIsDirty();
        bool IsDirty { get;}
    }
}
