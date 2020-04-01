using AcadBillOfQuantities.Infrastructure.Interfaces;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;

namespace AcadBillOfQuantities.Core.Commands
{
    public class SetLayerCurrent : IAcadCommand<string>
    {
        public void Execute(string layerName = null)
        {
            layerName = layerName ?? "_BOQ_Main";
            // Get the current document and database
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;

            // Lock the new document
            using (DocumentLock acLckDoc = acDoc.LockDocument())
            {
                // Start a transaction
                using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
                {
                    // Open the Layer table for read
                    LayerTable acLyrTbl;
                    acLyrTbl = acTrans.GetObject(acCurDb.LayerTableId,
                        OpenMode.ForRead) as LayerTable;


                    if (acLyrTbl.Has(layerName) == true)
                    {
                        // Set the layer Center current
                        acCurDb.Clayer = acLyrTbl[layerName];

                        // Save the changes
                        acTrans.Commit();
                    }

                    // Dispose of the transaction
                }
            }
        }
    }
}
