using Autodesk.AutoCAD.Runtime;


[assembly: CommandClass(typeof(AcadBillOfQuantities.Core.Commands.StartApp))]

namespace AcadBillOfQuantities.Core.Commands
{
    public class StartApp
    {
        [CommandMethod("StartBillOfQuantities")]
        public static void StartAcadBillOfQuantitiesApp()
        {
            App.MainWindow.Show();
        }

    }
}
