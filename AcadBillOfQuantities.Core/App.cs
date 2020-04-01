using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadBillOfQuantities.UI;
using AcadBillOfQuantities.UI.ViewModel;
using GalaSoft.MvvmLight.Ioc;

namespace AcadBillOfQuantities.Core
{
    internal static class App
    {
        private static object syncRoot = new object();

        private static MainWindow _mainWindow;
        public static MainWindow MainWindow
        {
            get
            {
                lock (syncRoot)
                {
                    if (_mainWindow == null)
                    {
                        new TypeRegistrator().RegisterAll();
                        _mainWindow = new MainWindow()
                        {
                            Topmost = true
                        };
                        _mainWindow.DataContext = SimpleIoc.Default
                            .GetInstance<ViewModelLocator>().Main;
                    }
                }
                return _mainWindow;
            }
        }
    }
}
