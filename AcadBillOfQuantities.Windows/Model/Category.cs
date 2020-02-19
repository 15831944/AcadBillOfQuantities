using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AcadBillOfQuantities.Windows.Models
{
    [XmlRoot("Settings")]
    public class Settings
    {
        [XmlElement("Category")]
        public List<Category> Categories { get; set; }
    }

    public class Category
    {
        [XmlAttribute("Name")]
        public string Name { get; set; }
        [XmlAttribute("ShortcutKey")]
        public char ShortcutKey { get; set; }
        [XmlElement("SubCategory")]
        public List<Category> SubCategories { get; set; }

        public bool HasSubCategories => this.SubCategories.Any();
    }
}
