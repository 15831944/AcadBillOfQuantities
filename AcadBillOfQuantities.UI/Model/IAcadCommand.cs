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

    public interface IAcadCommand<TResult, TInput>
    {
        TResult Execute(TInput arg);
    }

    public interface IGetTotalLengthCommand : IAcadCommand
    {
    }

    public interface ICreateCategoryPolyline : IAcadCommand<string>
    {
    }

    public interface IGetPlineCollectionsCommand : IAcadCommand<Dictionary<string, (int count, double length, double area)>, IEnumerable<string>>
    {
    }
}
