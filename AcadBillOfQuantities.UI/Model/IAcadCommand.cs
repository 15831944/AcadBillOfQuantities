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

    public interface IAcadCommand<T> where T : class
    {
        void Execute(T obj);
    }

    public interface IGetTotalLengthCommand : IAcadCommand
    {
    }

    public interface ICreateCategoryPolyline : IAcadCommand<string>
    {
    }
}
