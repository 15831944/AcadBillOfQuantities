using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadBillOfQuantities.Core.Commands;
using AcadBillOfQuantities.Core.Commands.Composite;
using AcadBillOfQuantities.Infrastructure.Interfaces;
using AcadBillOfQuantities.UI;
using AcadBillOfQuantities.UI.ViewModel;
using GalaSoft.MvvmLight.Ioc;

namespace AcadBillOfQuantities.Core
{
    class TypeRegistrator
    {
        public void RegisterAll()
        {
            SimpleIoc.Default.Reset();
            SimpleIoc.Default.Register<PrepareLayer>();
            SimpleIoc.Default.Register<SetLayerCurrent>();
            SimpleIoc.Default.Register<ExecuteAcadCommandLine>();
            SimpleIoc.Default.Register<CountPolylines>();
            SimpleIoc.Default.Register<ICreateCategoryPolyline, CreateCategoryPolyline>();
            SimpleIoc.Default.Register<IGetPlineCollectionsCommand, GetPlineCollectionsCommand>();
            SimpleIoc.Default.Register<ViewModelLocator>();
        }
    }
}
