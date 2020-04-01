using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;

namespace AcadBillOfQuantities.Core.Commands
{
    public class ExecuteAcadCommandLine
    {
        public void Execute(string acadCommandString)
        {
            Document acDoc = Application.DocumentManager.MdiActiveDocument;

            acDoc.SendStringToExecute(acadCommandString, true, false, false);
        }
    }
}
