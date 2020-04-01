using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AcadBillOfQuantities.Infrastructure.Interfaces;
using Autodesk.AutoCAD.DatabaseServices;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;
using Polyline = Autodesk.AutoCAD.DatabaseServices.Polyline;


namespace AcadBillOfQuantities.Core.Commands
{
    public class CountPolylines : IAcadCommand<IEnumerable<string>>
    {
        public Dictionary<string, (int count, double length, double area)> Results { get; private set; }

        public void Execute(IEnumerable<string> layerNames)
        {
            this.Results = new Dictionary<string, (int count, double length, double area)>();
            // Get the current document
            var acDocument = Application.DocumentManager.MdiActiveDocument;

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
                foreach (ObjectId acObjId in acBlkTblRec)
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

                string userInfo = "Count of polylines per layer:\r\n";
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
