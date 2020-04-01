using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadBillOfQuantities.Infrastructure.Interfaces;

namespace AcadBillOfQuantities.Core.Commands.Composite
{
    public class GetPlineCollectionsCommand : IGetPlineCollectionsCommand
    {
        private CountPolylines CountPolylinesCommand { get; }

        public GetPlineCollectionsCommand(CountPolylines countPolylines)
        {
            this.CountPolylinesCommand = countPolylines;
        }

        public Dictionary<string, (int count, double length, double area)> Execute(IEnumerable<string> arg)
        {
            this.CountPolylinesCommand.Execute(arg);
            return CountPolylinesCommand.Results;
        }
    }
}
