using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using System.Windows;
using System.Windows.Forms;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;


[assembly: CommandClass(typeof(AcadBillOfQuantities.Core.Class1))]

namespace AcadBillOfQuantities.Core
{
    public class Class1
    {
        //[CommandMethod("GetPointsFromUser")]
        public static void GetPointsFromUser()
        {
            // Get the current database and start the Transaction Manager
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;

            PromptPointResult pPtRes;
            PromptPointOptions pPtOpts = new PromptPointOptions("");

            // Prompt for the start point
            pPtOpts.Message = "\nEnter the start point of the line: ";
            pPtRes = acDoc.Editor.GetPoint(pPtOpts);
            Point3d ptStart = pPtRes.Value;

            // Exit if the user presses ESC or cancels the command
            if (pPtRes.Status == PromptStatus.Cancel) return;

            // Prompt for the end point
            pPtOpts.Message = "\nEnter the end point of the line: ";
            pPtOpts.UseBasePoint = true;
            pPtOpts.BasePoint = ptStart;
            pPtRes = acDoc.Editor.GetPoint(pPtOpts);
            Point3d ptEnd = pPtRes.Value;

            if (pPtRes.Status == PromptStatus.Cancel) return;

            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                BlockTable acBlkTbl;
                BlockTableRecord acBlkTblRec;

                // Open Model space for write
                acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId,
                    OpenMode.ForRead) as BlockTable;

                acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
                    OpenMode.ForWrite) as BlockTableRecord;

                // Define the new line
                Line acLine = new Line(ptStart, ptEnd);
                acLine.SetDatabaseDefaults();

                // Add the line to the drawing
                acBlkTblRec.AppendEntity(acLine);
                acTrans.AddNewlyCreatedDBObject(acLine, true);

                // Zoom to the extents or limits of the drawing
                acDoc.SendStringToExecute("._zoom _all ", true, false, false);

                // Commit the changes and dispose of the transaction
                acTrans.Commit();
            }
        }

        //[CommandMethod("CheckForPickfirstSelection", CommandFlags.UsePickSet)]
        public static void CheckForPickfirstSelection()
        {
            // Get the current document
            Editor acDocEd = Application.DocumentManager.MdiActiveDocument.Editor;

            // Get the PickFirst selection set
            PromptSelectionResult acSSPrompt;
            acSSPrompt = acDocEd.SelectImplied();

            SelectionSet acSSet;

            // If the prompt status is OK, objects were selected before
            // the command was started
            if (acSSPrompt.Status == PromptStatus.OK)
            {
                acSSet = acSSPrompt.Value;

                Application.ShowAlertDialog("Number of objects in Pickfirst selection: " +
                                            acSSet.Count.ToString());
            }
            else
            {
                Application.ShowAlertDialog("Number of objects in Pickfirst selection: 0");
            }

            // Clear the PickFirst selection set
            ObjectId[] idarrayEmpty = new ObjectId[0];
            acDocEd.SetImpliedSelection(idarrayEmpty);

            // Request for objects to be selected in the drawing area
            acSSPrompt = acDocEd.GetSelection();

            // If the prompt status is OK, objects were selected
            if (acSSPrompt.Status == PromptStatus.OK)
            {
                acSSet = acSSPrompt.Value;

                Application.ShowAlertDialog("Number of objects selected: " +
                                            acSSet.Count.ToString());
            }
            else
            {
                Application.ShowAlertDialog("Number of objects selected: 0");
            }
        }

        //[CommandMethod("ListEntities")]
        public static void ListEntities()
        {
            // Get the current document and database, and start a transaction
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;

            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Block table record for read
                BlockTable acBlkTbl;
                acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId,
                    OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for read
                BlockTableRecord acBlkTblRec;
                acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
                    OpenMode.ForRead) as BlockTableRecord;

                int nCnt = 0;
                acDoc.Editor.WriteMessage("\nModel space objects: ");

                // Step through each object in Model space and
                // display the type of object found
                foreach (ObjectId acObjId in acBlkTblRec)
                {
                    acDoc.Editor.WriteMessage("\n" + acObjId.ObjectClass.DxfName);

                    nCnt = nCnt + 1;
                }

                // If no objects are found then display a message
                if (nCnt == 0)
                {
                    acDoc.Editor.WriteMessage("\n No objects found");
                }

                // Dispose of the transaction
            }
        }

        [CommandMethod("GetTotalLength", CommandFlags.UsePickSet)]
        public void GetTotalLength()
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

                double totalLength = default;
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
                            totalLength += pline.Length;
                            nCnt++;
                        }
                    }

                    string userInfoContetnt = $"\nSummed polylines: {nCnt}" +
                                              $"\nTotal Length: {totalLength}" +
                                              $"\nTotal length is copied to clipboard." +
                                              $"\nUse Paste command (Ctrl+V) to place vale.";

                    Application.ShowAlertDialog(userInfoContetnt);
                    acDocument.Editor.WriteMessage(userInfoContetnt);

                    Clipboard.SetText(totalLength.ToString(CultureInfo.InvariantCulture));
                }
            }
            else
            {
                Application.ShowAlertDialog("Select polylines first");
            }
        }

        [CommandMethod("GetTotalArea", CommandFlags.UsePickSet)]
        public void GetTotalArea()
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

                    Clipboard.SetText(totalArea.ToString(CultureInfo.InvariantCulture));
                }
            }
            else
            {
                Application.ShowAlertDialog("Select polylines first");
            }
        }

        [CommandMethod("AddMyLayer")]
        public static void AddMyLayer()
        {
            // Get the current document and database, and start a transaction
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;

            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Returns the layer table for the current database
                LayerTable acLyrTbl;
                acLyrTbl = acTrans.GetObject(acCurDb.LayerTableId,
                                             OpenMode.ForRead) as LayerTable;

                // Check to see if MyLayer exists in the Layer table
                if (acLyrTbl.Has("MyLayer") != true)
                {
                    // Open the Layer Table for write
                    acLyrTbl.UpgradeOpen();

                    // Create a new layer table record and name the layer "MyLayer"
                    LayerTableRecord acLyrTblRec = new LayerTableRecord();
                    acLyrTblRec.Name = "MyLayer";

                    // Add the new layer table record to the layer table and the transaction
                    acLyrTbl.Add(acLyrTblRec);
                    acTrans.AddNewlyCreatedDBObject(acLyrTblRec, true);

                    // Commit the changes
                    acTrans.Commit();
                }

                // Dispose of the transaction
            }
        }
    }
}
