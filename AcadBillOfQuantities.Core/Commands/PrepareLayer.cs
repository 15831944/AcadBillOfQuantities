using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;
using Color = Autodesk.AutoCAD.Colors.Color;
using AcadBillOfQuantities.Infrastructure.Interfaces;

namespace AcadBillOfQuantities.Core.Commands
{
    public class PrepareLayer : IAcadCommand<string>
    {
        public void Execute(string layerName)
        {
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

                    LayerTableRecord acLyrTblRec;

                    if (!acLyrTbl.Has(layerName))
                    {
                        acLyrTblRec = new LayerTableRecord();

                        // Assign the layer a name
                        acLyrTblRec.Name = layerName;

                        // Upgrade the Layer table for write
                        if (acLyrTbl.IsWriteEnabled == false) acLyrTbl.UpgradeOpen();

                        // Append the new layer to the Layer table and the transaction
                        acLyrTbl.Add(acLyrTblRec);
                        acTrans.AddNewlyCreatedDBObject(acLyrTblRec, true);

                        Color acadColor;
                        // Define an array of colors for the layers
                        using (var hashAlgorithm = new SHA1Managed())
                        {
                            byte[] rgb = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(layerName)).Take(3).ToArray();
                            acadColor = Color.FromRgb(rgb[0], rgb[1], rgb[2]);
                        }

                        // Set the color of the layer
                        acLyrTblRec.Color = acadColor;
                        acLyrTblRec.LineWeight = LineWeight.LineWeight030;
                    }

                    // Save the changes and dispose of the transaction
                    acTrans.Commit();
                }
            }
        }
    }
}
