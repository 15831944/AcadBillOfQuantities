using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AcadBillOfQuantities.Infrastructure.Interfaces;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;
using Polyline = Autodesk.AutoCAD.DatabaseServices.Polyline;


namespace AcadBillOfQuantities.Core.Commands
{
    public class CountPolylines : IAcadCommand<IEnumerable<string>>
    {
        public Dictionary<string, (int count, double length, double area)> Results { get; private set; }

        public void Execute(IEnumerable<string> layerNames)
        {
            // Get the current document
            var acDocument = Application.DocumentManager.MdiActiveDocument;

            // Get the PickFirst selection set
            PromptSelectionResult acSSPrompt;
            acSSPrompt = acDocument.Editor.SelectImplied();

            SelectionSet acSSet = null;

            // If the prompt status is OK, objects were selected before
            // the command was started
            if (acSSPrompt.Status == PromptStatus.OK)
            {
                acSSet = acSSPrompt.Value;
            }

            this.Results = new Dictionary<string, (int count, double length, double area)>();
            // Get the current document

            // Get the current document and database, and start a transaction
            Database acCurDb = acDocument.Database;

            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                int nCnt = 0;

                // Open the Block table record for read
                BlockTable acBlkTbl;
                acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId,
                                             OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for read
                BlockTableRecord acBlkTblRec;
                acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
                                                OpenMode.ForRead) as BlockTableRecord;

                acDocument.Editor.WriteMessage("\nModel space objects: ");

                // Step through each object in Model space and
                // display the type of object found
                IEnumerable<ObjectId> iterator = acSSet?.GetObjectIds();
                if (iterator is null)
                {
                    var objectIds = new List<ObjectId>();
                    foreach (ObjectId acObjId in acBlkTblRec) objectIds.Add(acObjId);
                    iterator = objectIds;
                }
                foreach (ObjectId acObjId in iterator)
                {
                    //acDocument.Editor.WriteMessage("\n" + acObjId.ObjectClass.DxfName);
                    var pline = acTrans.GetObject(acObjId, OpenMode.ForRead) as Polyline;
                    if (!(pline is null) && layerNames.Any(layer => layer == pline.Layer))
                    {
                        if (!this.Results.ContainsKey(pline.Layer))
                            this.Results.Add(pline.Layer, (default, default, default));
                        var current = this.Results[pline.Layer];
                        var updated =
                            (current.count += 1, current.length += pline.Length, current.area += pline.Area);
                        this.Results[pline.Layer] = updated;
                    }
                }

                string userInfo = acSSet is null ? string.Empty : "[Selection only]\r\n\r\n";
                userInfo += "Count of polylines per layer:\r\n";
                userInfo += "Category\tCount\tLength\tArea\r\n";
                foreach (var entry in this.Results)
                {
                    userInfo += $"{entry.Key.Replace("_", " ").Trim()}\t{entry.Value.count}\t" +
                                $"{GetFormated(entry.Value.length)}\t{GetFormated(entry.Value.area)}\r\n";
                }

                Application.ShowAlertDialog(userInfo);
                acDocument.Editor.WriteMessage(userInfo);

                System.Windows.Clipboard.SetText(userInfo);
            }
        }

        private static string GetFormated(double input)
        {
            return string.Format(CultureInfo.CurrentCulture, "{0:0.00}", input).Replace(".",",");
        }
    }
}
