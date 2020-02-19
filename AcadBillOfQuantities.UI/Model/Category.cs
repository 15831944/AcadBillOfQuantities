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
        //public char ShortcutKey { get; set; }
        public ObservableCollection<Category> SubCategories { get; set; }

        public bool HasSubCategories => this.SubCategories.Any();
    }
}
