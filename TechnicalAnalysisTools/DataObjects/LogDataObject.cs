using System;

namespace TechnicalAnalysisTools.DataObjects
{
    public class LogDataObject
    {
        public DateTime LogTime { get; set; }

        public string Action { get; set; }

        public string Message { get; set; }
    }
}
