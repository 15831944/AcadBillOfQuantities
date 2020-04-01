using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcadBillOfQuantities.Core.Models
{
    class PlineCollection
    {
        public int Count { get; set; }
        public double Length { get; set; }
        public double Area { get; set; }

        public PlineCollection()
        {
            Count = default;
            Length = default;
            Area = default;
        }
    }
}
