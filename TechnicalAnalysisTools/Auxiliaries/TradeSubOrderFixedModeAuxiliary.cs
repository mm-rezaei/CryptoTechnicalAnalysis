using System;
using TechnicalAnalysisTools.Enumerations;
using TechnicalAnalysisTools.Shared.Enumerations;

namespace TechnicalAnalysisTools.Auxiliaries
{
    public class TradeSubOrderFixedModeAuxiliary : TradeSubOrderModeAuxiliaryBase
    {
        public TradeSubOrderFixedModeAuxiliary(TradeSubOrderModeTriggerdTypes tradeSubOrderExitType, float primaryPrice, FixedModeActiveRangeTypes fixedModeActiveRange) : base(primaryPrice)
        {
            TradeSubOrderExitType = tradeSubOrderExitType;

            FixedModeActiveRange = fixedModeActiveRange;
        }

        private TradeSubOrderModeTriggerdTypes TradeSubOrderExitType { get; }

        private FixedModeActiveRangeTypes FixedModeActiveRange { get; }

        public override float TargetPrice => PrimaryPrice;

        public override TradeSubOrderModes TradeSubOrderMode => TradeSubOrderModes.None;

        public override bool TraversePrice(float fromPrice, float toPrice, out float matchedPrice)
        {
            var result = false;

            matchedPrice = 0;

            if (fromPrice <= toPrice)
            {
                if (fromPrice <= PrimaryPrice && PrimaryPrice <= toPrice)
                {
                    result = true;

                    matchedPrice = PrimaryPrice;
                }
            }
            else
            {
                if (toPrice <= PrimaryPrice && PrimaryPrice <= fromPrice)
                {
                    result = true;

                    matchedPrice = PrimaryPrice;
                }
            }

            if (!result)
            {
                switch (FixedModeActiveRange)
                {
                    case FixedModeActiveRangeTypes.Up:
                        {
                            if (Math.Max(fromPrice, toPrice) <= PrimaryPrice)
                            {
                                result = true;

                                if (TradeSubOrderExitType == TradeSubOrderModeTriggerdTypes.Liquid)
                                {
                                    matchedPrice = PrimaryPrice;
                                }
                                else
                                {
                                    matchedPrice = fromPrice;
                                }
                            }
                        }
                        break;
                    case FixedModeActiveRangeTypes.Down:
                        {
                            if (Math.Min(fromPrice, toPrice) >= PrimaryPrice)
                            {
                                result = true;

                                if (TradeSubOrderExitType == TradeSubOrderModeTriggerdTypes.Liquid)
                                {
                                    matchedPrice = PrimaryPrice;
                                }
                                else
                                {
                                    matchedPrice = fromPrice;
                                }
                            }
                        }
                        break;
                }
            }

            return result;
        }
    }
}
