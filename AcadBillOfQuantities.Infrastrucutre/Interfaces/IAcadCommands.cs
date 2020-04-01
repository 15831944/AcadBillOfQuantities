using System;
using System.Collections.Generic;
using System.Text;

namespace AcadBillOfQuantities.Infrastructure.Interfaces
{
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
