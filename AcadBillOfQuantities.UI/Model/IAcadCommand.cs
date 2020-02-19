using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcadBillOfQuantities.UI.Model
{
    public interface IAcadCommand
    {
        void Execute();
    }

    public interface IGetTotalLengthCommand : IAcadCommand
    {
    }
}
