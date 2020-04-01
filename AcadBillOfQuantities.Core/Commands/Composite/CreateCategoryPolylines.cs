using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadBillOfQuantities.Infrastructure.Interfaces;

namespace AcadBillOfQuantities.Core.Commands.Composite
{
    public class CreateCategoryPolyline : ICreateCategoryPolyline
    {
        private PrepareLayer PrepareLayerCommand { get; }
        private SetLayerCurrent SetLayerCurrentCommand { get; }
        private ExecuteAcadCommandLine ExecuteAcadCommandLineCommand { get; }
        public CreateCategoryPolyline(
            PrepareLayer prepareLayerCommand,
            SetLayerCurrent setLayerCurrent,
            ExecuteAcadCommandLine executeAcadCommandLine)
        {
            this.PrepareLayerCommand = prepareLayerCommand;
            this.SetLayerCurrentCommand = setLayerCurrent;
            this.ExecuteAcadCommandLineCommand = executeAcadCommandLine;

        }

        public void Execute(string layerName)
        {
            this.PrepareLayerCommand.Execute(layerName);
            this.SetLayerCurrentCommand.Execute(layerName);
            this.ExecuteAcadCommandLineCommand.Execute("PLINE ");
        }
    }
}
