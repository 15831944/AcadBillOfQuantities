using System.Collections.Generic;
using System.Collections.ObjectModel;
using GalaSoft.MvvmLight;
using System.IO;
using System.Windows.Input;
using System.Xml.Serialization;
using AcadBillOfQuantities.UI.Model;
using System.Xml;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Ioc;

namespace AcadBillOfQuantities.UI.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        public ObservableCollection<Category> Categories { get; set; }
        public ICommand ExecuteAcadCommand { get; private set; }

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            if (IsInDesignMode)
            {
                // Code runs in Blend --> create design time data.
            }
            else
            {
                ExecuteAcadCommand = new RelayCommand(SimpleIoc.Default.GetInstance<IGetTotalLengthCommand>().Execute);
                string thisDir = Directory.GetCurrentDirectory();
                string configFilePath = Path.Combine(thisDir, "AppSettings.xml");

                var serializer = new XmlSerializer(type: typeof(Settings));
                Settings settings;
                using (var stream = XmlReader.Create(configFilePath))
                {
                    settings = (Settings)serializer.Deserialize(stream);
                }

                this.Categories = GenerateObservableCollection(settings.Categories);
            }
        }

        private ObservableCollection<Category> GenerateObservableCollection(IEnumerable<XmlCategory> xmlCategories)
        {
            var result = new ObservableCollection<Category>();
            foreach (var cat in xmlCategories)
            {
                var category = new Category();
                category.Name = cat.Name;
                if (cat.HasSubCategories)
                    category.SubCategories = GenerateObservableCollection(cat.SubCategories);
                else category.SubCategories = new ObservableCollection<Category>();
                result.Add(category);
            }

            return result;
        }
    }
}