using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;

namespace AcadBillOfQuantities.UI.Model
{
    public class Category : ObservableObject
    {
        public string Name { get; set; }
        public string LayerName { get; set; }
        public int Count { get; set; }
        public double SumLength { get; set; }
        public double SumArea { get; set; }
        //public char ShortcutKey { get; set; }
        public ObservableCollection<Category> Categories { get; set; }

        public bool IsLeaf => !this.Categories.Any();
    }
}
