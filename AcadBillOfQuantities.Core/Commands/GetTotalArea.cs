using System.Globalization;
using AcadBillOfQuantities.Infrastructure.Interfaces;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;
using Polyline = Autodesk.AutoCAD.DatabaseServices.Polyline;

[assembly: CommandClass(typeof(AcadBillOfQuantities.Core.Commands.GetTotalArea))]

namespace AcadBillOfQuantities.Core.Commands
{
    class GetTotalArea : IAcadCommand
    {
        [CommandMethod("GetTotalArea", CommandFlags.UsePickSet)]
        public void Execute()
        {
            // Get the current document
            var acDocument = Application.DocumentManager.MdiActiveDocument;

            // Get the PickFirst selection set
            PromptSelectionResult acSSPrompt;
            acSSPrompt = acDocument.Editor.SelectImplied();

            SelectionSet acSSet;

            // If the prompt status is OK, objects were selected before
            // the command was started
            if (acSSPrompt.Status == PromptStatus.OK)
            {
                acSSet = acSSPrompt.Value;


                // Get the current document and database, and start a transaction
                Database acCurDb = acDocument.Database;

                double totalArea = default;
                using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
                {
                    int nCnt = 0;

                    // Step through each object in Model space and
                    // display the type of object found
                    foreach (var acObjId in acSSet.GetObjectIds())
                    {
                        var pline = acTrans.GetObject(acObjId, OpenMode.ForRead) as Polyline;
                        if (!(pline is null))
                        {
                            totalArea += pline.Area;
                            nCnt++;
                        }
                    }

                    string userInfoContetnt = $"\nSummed polylines: {nCnt}" +
                                              $"\nTotal Area: {totalArea}" +
                                              $"\nTotal area is copied to clipboard." +
                                              $"\nUse Paste command (Ctrl+V) to place vale.";


                    Application.ShowAlertDialog(userInfoContetnt);
                    acDocument.Editor.WriteMessage(userInfoContetnt);

                    System.Windows.Clipboard.SetText(totalArea.ToString(CultureInfo.InvariantCulture));
                }
            }
            else
            {
                Application.ShowAlertDialog("Select polylines first");
            }
        }
    }
}
