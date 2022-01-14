using System;
using TechnicalAnalysisTools.Shared.Enumerations;

namespace TechnicalAnalysisTools.Shared.DataObjects
{
    [Serializable]
    public class CommandDataObject
    {
        public CommandDataObject()
        {
            CommandId = Guid.NewGuid();
        }

        public Guid CommandId { get; set; }

        public CommandTypes Command { get; set; }

        public object Parameter { get; set; }
    }
}
