using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using AcadBillOfQuantities.UI;
using AcadBillOfQuantities.UI.Model;
using AcadBillOfQuantities.UI.ViewModel;
using Autodesk.AutoCAD.GraphicsInterface;
using CommonServiceLocator;
using GalaSoft.MvvmLight.Ioc;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;
using Color = Autodesk.AutoCAD.Colors.Color;
using Polyline = Autodesk.AutoCAD.DatabaseServices.Polyline;


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

        [CommandMethod("CountPolylinesInLayer")]
        public static void CountPolylinesIn(string layerName = null)
        {
            // Get the current document
            var acDocument = Application.DocumentManager.MdiActiveDocument;

            if (string.IsNullOrEmpty(layerName))
            {
                PromptStringOptions pStrOpts = new PromptStringOptions("\nEnter your name: ");
                pStrOpts.AllowSpaces = true;
                PromptResult pStrRes = acDocument.Editor.GetString(pStrOpts);

                layerName = pStrRes.StringResult;
            }

            // Get the current document and database, and start a transaction
            Database acCurDb = acDocument.Database;

            double totalLength = default;
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
                    if (!(pline is null) && pline.Layer == layerName)
                    {
                        //totalLength += pline.Length;
                        nCnt++;
                    }
                }

                string userInfoContetnt = $"\nSummed polylines: {nCnt}" +
                                          $"\nTotal Length: {totalLength}" +
                                          $"\nTotal length is copied to clipboard." +
                                          $"\nUse Paste command (Ctrl+V) to place vale.";

                Application.ShowAlertDialog(nCnt.ToString());
                acDocument.Editor.WriteMessage(nCnt.ToString());

                System.Windows.Clipboard.SetText(totalLength.ToString(CultureInfo.InvariantCulture));
            }
        }

        public class PlineCollection
        {
            public int Count { get; set; }
            public double Length { get; set; }
            public double Area { get; set; }

            public PlineCollection()
            {
                Count = default;
                Length = default;
                Area = default;
            }
        }

        [CommandMethod("CountPolylines")]
        public static void CountPolylines(IEnumerable<string> layerNames)
        {
            AcadCommands.ResultDictionary = new Dictionary<string, (int count, double length, double area)>();
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
                        if (!AcadCommands.ResultDictionary.ContainsKey(pline.Layer))
                            AcadCommands.ResultDictionary.Add(pline.Layer, (default, default, default));
                        var current = AcadCommands.ResultDictionary[pline.Layer];
                        var updated =
                            (current.count += 1, current.length += pline.Length, current.area += pline.Area);
                        AcadCommands.ResultDictionary[pline.Layer] = updated;
                    }
                }

                string userInfo = "Count of polylines per layer:\r\n";
                userInfo += "Category\tCount\tLength\tArea\r\n";
                foreach (var entry in AcadCommands.ResultDictionary)
                {
                    userInfo += $"{entry.Key}\t{entry.Value.count}\t" +
                                $"{entry.Value.length}\t{entry.Value.area}\r\n";
                    /*
                    userInfo += $"In layer \"{entry.Key}\" count: {entry.Value.count}, " +
                                $"sum length: {entry.Value.length}, sum area: {entry.Value.area}\r\n";
                                */
                }

                Application.ShowAlertDialog(userInfo);
                acDocument.Editor.WriteMessage(userInfo);

                System.Windows.Clipboard.SetText(userInfo);
            }
        }

        public static Dictionary<string, (int count, double length, double area)> ResultDictionary { get; set; }

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

            acDoc.SendStringToExecute(acadCommandString, true, false, false);
        }


        [CommandMethod("PrepareLayer")]
        public static void PrepareLayer(string layerName)
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
                    }

                    // Save the changes and dispose of the transaction
                    acTrans.Commit();
                }
            }
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
                        SimpleIoc.Default.Register<IGetPlineCollectionsCommand, GetPlineCollectionsCommand>();
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
                AcadCommands.PrepareLayer(layerName);
                AcadCommands.SetLayerCurrent(layerName);
                AcadCommands.SendACommandToAutoCAD("PLINE ");
            }
        }

        public class GetPlineCollectionsCommand : IGetPlineCollectionsCommand
        {
            public Dictionary<string, (int count, double length, double area)> Execute(IEnumerable<string> arg)
            {
                AcadCommands.CountPolylines(arg);
                return AcadCommands.ResultDictionary;
            }
        }
    }
}
