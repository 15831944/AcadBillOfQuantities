using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Eventing.Reader;
using GalaSoft.MvvmLight;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
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
        private HashAlgorithm _hashAlgorithm;
        public ObservableCollection<Category> Categories { get; set; }
        public ICommand ExpandAllCommand { get; private set; }
        public ICommand CollapseAllCommand { get; private set; }
        public ICommand AddCategoryPolylineCommand { get; private set; }
        public ICommand GetPolylinesDataCommand { get; private set; }


        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            _hashAlgorithm = new SHA1Managed();
            if (IsInDesignMode)
            {
                // Code runs in Blend --> create design time data.
            }
            else
            {
                AddCategoryPolylineCommand = new RelayCommand<string>(
                    layerName => CreateCategoryPolyline(layerName));
                GetPolylinesDataCommand = new RelayCommand<IEnumerable<string>>(
                    layerNames => GetPolylinesDataFromAcad(layerNames));
                ExpandAllCommand = new RelayCommand(ExpandAll);
                CollapseAllCommand = new RelayCommand(CollapseAll);

                string thisDir = Directory.GetCurrentDirectory();
                string configFilePath = Path.Combine(thisDir, "AppSettings.xml");

                var serializer = new XmlSerializer(type: typeof(Settings));
                Settings settings;
                using (var stream = XmlReader.Create(configFilePath))
                {
                    settings = (Settings)serializer.Deserialize(stream);
                }

                this.Categories = GenerateObservableCollection(settings.Categories, "_");
                this._hashAlgorithm.Dispose();
            }
        }

        private void CreateCategoryPolyline(string layerName)
        {
            try
            {
                SimpleIoc.Default.GetInstance<ICreateCategoryPolyline>().Execute(layerName);
            } catch { }
        }

        private void GetPolylinesDataFromAcad(IEnumerable<string> layersToGet = null)
        {
            var input = layersToGet ?? this.Categories.SelectMany(c => c.GetLeafs()).Select(c => c.LayerName);
            try
            {
                var receivedData = SimpleIoc.Default.GetInstance<IGetPlineCollectionsCommand>()
                    .Execute(input);
            } catch { }
        }

        private ObservableCollection<Category> GenerateObservableCollection(
            IEnumerable<XmlCategory> xmlCategories,
            string parentLayerName = "")
        {
            var result = new ObservableCollection<Category>();
            foreach (var cat in xmlCategories)
            {
                var category = new Category();
                category.Name = cat.Name;
                string delimeter = parentLayerName == "_ " ? string.Empty : " - ";
                string prefix = string.IsNullOrEmpty(parentLayerName) ? string.Empty : parentLayerName + delimeter;
                category.LayerName = prefix + cat.Name;
                if (cat.HasSubCategories)
                    category.Categories = GenerateObservableCollection(cat.SubCategories, category.LayerName);
                byte[] rgb = _hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(category.LayerName)).Take(3).ToArray();
                category.Color = new SolidColorBrush(Color.FromRgb(rgb[0],rgb[1],rgb[2]));
                result.Add(category);
                // TODO: load clors from autocad!!
            }

            return result;
        }

        private void ExpandAll() => SwitchDeep(true, this.Categories);
        private void CollapseAll() => SwitchDeep(false, this.Categories);
        private void SwitchDeep(bool isExpanded, IEnumerable<Category> input = null)
        {
            foreach (var i in input)
            {
                i.IsExpanded = isExpanded;
                SwitchDeep(isExpanded, i.Categories);
            }
        }
    }
}