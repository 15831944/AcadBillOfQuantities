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
using AcadBillOfQuantities.UI;
using AcadBillOfQuantities.UI.Model;
using AcadBillOfQuantities.UI.ViewModel;
using CommonServiceLocator;
using GalaSoft.MvvmLight.Ioc;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;


[assembly: CommandClass(typeof(AcadBillOfQuantities.Core.AcadCommands))]

namespace AcadBillOfQuantities.Core
{
    public class AcadCommands
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
        public static void GetTotalLength()
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

                    System.Windows.Clipboard.SetText(totalLength.ToString(CultureInfo.InvariantCulture));
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

                    System.Windows.Clipboard.SetText(totalArea.ToString(CultureInfo.InvariantCulture));
                }
            }
            else
            {
                Application.ShowAlertDialog("Select polylines first");
            }
        }

        [CommandMethod("AddMyLayer")]
        public static void AddMyLayer(string layerName = null)
        {
            layerName = layerName ?? "_BOQ_Main";
            // Get the current document and database, and start a transaction
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;

            // Lock the new document
            using (DocumentLock acLckDoc = acDoc.LockDocument())
            {
                using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
                {
                    // Returns the layer table for the current database
                    LayerTable acLyrTbl;
                    acLyrTbl = acTrans.GetObject(acCurDb.LayerTableId,
                        OpenMode.ForRead) as LayerTable;

                    // Check to see if MyLayer exists in the Layer table
                    if (acLyrTbl.Has(layerName) != true)
                    {

                        // Open the Layer Table for write
                        acLyrTbl.UpgradeOpen();
                        // Create a new layer table record and name the layer "MyLayer"
                        LayerTableRecord acLyrTblRec = new LayerTableRecord();
                        acLyrTblRec.Name = layerName;

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

        [CommandMethod("SetLayerCurrent")]
        public static void SetLayerCurrent(string layerName = null)
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


        [CommandMethod("SendACommandToAutoCAD")]
        public static void SendACommandToAutoCAD(string acadCommandString)
        {
            Document acDoc = Application.DocumentManager.MdiActiveDocument;

            // Draws a circle and zooms to the extents or
            // limits of the drawing
            acDoc.SendStringToExecute(acadCommandString, true, false, false);
        }

        private static object syncRoot = new object();
        private static UI.App _boqApp;

        public static UI.App BoqApp
        {
            get
            {
                lock (syncRoot)
                {
                    if (_boqApp == null)
                        _boqApp = new UI.App();
                    _boqApp.InitializeComponent();
                }

                return _boqApp;
            }
        }

        private static ViewModelLocator _viewModelLocator;
        public static ViewModelLocator ViewModelLocatorInstance
        {
            get
            {
                lock (syncRoot)
                {
                    if (_viewModelLocator == null)
                    {
                        //ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);
                        SimpleIoc.Default.Reset();
                        SimpleIoc.Default.Register<IGetTotalLengthCommand, GetTotalLengthCommand>();
                        SimpleIoc.Default.Register<ICreateCategoryPolyline, CreateCategoryPolyline>();
                        _viewModelLocator = new ViewModelLocator();
                    }
                }
                return _viewModelLocator;
            }
        }

        private static MainWindow _mainWindow;
        public static MainWindow MainWindow
        {
            get
            {
                lock (syncRoot)
                {
                    if (_mainWindow == null)
                    {
                        _mainWindow = new MainWindow()
                        {
                            Topmost = true
                        };
                        _mainWindow.DataContext = AcadCommands.ViewModelLocatorInstance.Main;
                    }
                }
                return _mainWindow;
            }
        }


        private static UI.App _uiApp;
        public static UI.App UiApp
        {
            get
            {
                lock (syncRoot)
                {
                    if (_uiApp == null)
                    {
                        _uiApp = new UI.App();
                        _uiApp.Resources.Add("Locator", ViewModelLocatorInstance);
                    }
                }
                return _uiApp;
            }
        }


        [CommandMethod("StartAcadBillOfQuantitiesApp")]
        public static void StartAcadBillOfQuantitiesApp()
        {
            //UiApp.MainWindow.Show();
            MainWindow.Show();
        }

        public class GetTotalLengthCommand : IGetTotalLengthCommand
        {
            public void Execute()
            {
                AcadCommands.GetTotalLength();
            }
        }

        public class CreateCategoryPolyline : ICreateCategoryPolyline
        {
            public void Execute(string layerName)
            {
                AcadCommands.AddMyLayer(layerName);
                AcadCommands.SetLayerCurrent(layerName);
                AcadCommands.SendACommandToAutoCAD("PLINE ");
            }
        }
    }
}
