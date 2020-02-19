using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using GalaSoft.MvvmLight;

namespace AcadBillOfQuantities.UI.Model
{
    [XmlRoot("Settings")]
    public class Settings
    {
        [XmlElement("Category")]
        public List<XmlCategory> Categories { get; set; }
    }

    public class XmlCategory : ObservableObject
    {
        [XmlAttribute("Name")]
        public string Name { get; set; }
        [XmlAttribute("ShortcutKey")]
        public char ShortcutKey { get; set; }
        [XmlElement("SubCategory")]
        public List<XmlCategory> SubCategories { get; set; }

        public bool HasSubCategories
        {
            get
            {
                if (this.SubCategories is null) return false;
                else return this.SubCategories.Any();
            }
        }
    }
}
