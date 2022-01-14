using System;
using TechnicalAnalysisTools.Enumerations;

namespace TechnicalAnalysisTools.Auxiliaries
{
    public class TradeSubOrderAuxiliary
    {
        public int Id { get; set; }

        public int OrderId { get; set; }

        public TradeSubStatusTypes TradeSubStatusType { get; set; }

        public DateTime OpenTime { get; set; }

        public DateTime CloseTime { get; set; }

        public bool IsOpened
        {
            get { return OpenTime != DateTime.MinValue; }
        }

        public bool IsClosed
        {
            get { return CloseTime != DateTime.MinValue; }
        }

        public float OpenPrice { get; set; }

        public float ClosePrice { get; set; }

        public TradeAllocatedBalanceAuxiliary TradeAllocatedBalance { get; set; }

        public TradeSubOrderModeAuxiliaryBase EnterTradeSubOrderMode { get; set; }

        public TradeSubOrderModeAuxiliaryBase TakeProfitTradeSubOrderMode { get; set; }

        public TradeSubOrderModeAuxiliaryBase StopLossTradeSubOrderMode { get; set; }

        public TradeSubOrderModeAuxiliaryBase LiquidTradeSubOrderMode { get; set; }

        public void SetTradeSubOrderModeObjectsToNull()
        {
            EnterTradeSubOrderMode = null;
            TakeProfitTradeSubOrderMode = null;
            StopLossTradeSubOrderMode = null;
            LiquidTradeSubOrderMode = null;
        }
    }
}
