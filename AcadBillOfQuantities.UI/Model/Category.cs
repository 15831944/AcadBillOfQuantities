using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
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

        public Category()
        {
            this.Categories = new ObservableCollection<Category>();
        }

        public bool IsLeaf => !this.Categories.Any();

        private bool _isExpanded;
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set { Set(ref _isExpanded, value); }
        }

        private SolidColorBrush _color;
        public SolidColorBrush Color
        {
            get { return _color; }
            set { Set(ref _color, value); }
        }

        public IEnumerable<Category> GetLeafs()
        {
            var result = this.Categories.Where(c => c.IsLeaf)
                .Concat(this.Categories.Where(c => !c.IsLeaf).SelectMany(c => c.GetLeafs()));
            result = this.IsLeaf ? result.Append(this) : result;

            return result;
        }
    }
}
