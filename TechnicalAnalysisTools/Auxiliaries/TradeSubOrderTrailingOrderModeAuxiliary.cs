using System;
using TechnicalAnalysisTools.Enumerations;
using TechnicalAnalysisTools.Shared.Enumerations;

namespace TechnicalAnalysisTools.Auxiliaries
{
    public class TradeSubOrderTrailingOrderModeAuxiliary : TradeSubOrderModeAuxiliaryBase
    {
        public TradeSubOrderTrailingOrderModeAuxiliary(float primaryPrice, TrailingDirectionTypes trailingDirection, float percent) : base(primaryPrice)
        {
            TrailingDirection = trailingDirection;

            Percent = percent;

            LastSeenPrice = primaryPrice;
        }

        private TrailingDirectionTypes TrailingDirection { get; set; }

        private float Percent { get; set; }

        private float LastSeenPrice { get; set; }

        public override float TargetPrice => 0f;

        public override TradeSubOrderModes TradeSubOrderMode => TradeSubOrderModes.TrailingOrder;

        public override bool TraversePrice(float fromPrice, float toPrice, out float matchedPrice)
        {
            var result = false;

            matchedPrice = 0;

            if (fromPrice <= toPrice)
            {
                switch (TrailingDirection)
                {
                    case TrailingDirectionTypes.Up:
                        {
                            if (LastSeenPrice < fromPrice)
                            {
                                LastSeenPrice = toPrice;
                            }
                            else if (fromPrice <= LastSeenPrice && LastSeenPrice <= toPrice)
                            {
                                var expectedValue = LastSeenPrice * (1f - (Percent / 100f));

                                if (expectedValue >= fromPrice)
                                {
                                    result = true;

                                    matchedPrice = fromPrice;
                                }
                                else
                                {
                                    LastSeenPrice = toPrice;
                                }
                            }
                            else if (toPrice < LastSeenPrice)
                            {
                                var expectedValue = LastSeenPrice * (1f - (Percent / 100f));

                                if (expectedValue >= fromPrice)
                                {
                                    result = true;

                                    matchedPrice = fromPrice;
                                }
                            }
                        }
                        break;
                    case TrailingDirectionTypes.Down:
                        {
                            if (LastSeenPrice < fromPrice)
                            {
                                var expectedValue = LastSeenPrice * (1f + (Percent / 100f));

                                if (expectedValue < fromPrice)
                                {
                                    result = true;

                                    matchedPrice = fromPrice;
                                }
                                else if (fromPrice <= expectedValue && expectedValue <= toPrice)
                                {
                                    result = true;

                                    matchedPrice = expectedValue;
                                }
                            }
                            else if (fromPrice <= LastSeenPrice && LastSeenPrice <= toPrice)
                            {
                                LastSeenPrice = fromPrice;

                                var expectedValue = LastSeenPrice * (1f + (Percent / 100f));

                                if (fromPrice <= expectedValue && expectedValue <= toPrice)
                                {
                                    result = true;

                                    matchedPrice = expectedValue;
                                }
                            }
                            else if (toPrice < LastSeenPrice)
                            {
                                LastSeenPrice = fromPrice;
                            }
                        }
                        break;
                    default:
                        throw new Exception("Trailing direction type value is not valid.");
                }
            }
            else
            {
                switch (TrailingDirection)
                {
                    case TrailingDirectionTypes.Up:
                        {
                            if (fromPrice < LastSeenPrice)
                            {
                                var expectedValue = LastSeenPrice * (1f - (Percent / 100f));

                                if (fromPrice < expectedValue)
                                {
                                    result = true;

                                    matchedPrice = fromPrice;
                                }
                                else if (toPrice <= expectedValue && expectedValue <= fromPrice)
                                {
                                    result = true;

                                    matchedPrice = expectedValue;
                                }
                            }
                            else if (toPrice <= LastSeenPrice && LastSeenPrice <= fromPrice)
                            {
                                LastSeenPrice = fromPrice;

                                var expectedValue = LastSeenPrice * (1f - (Percent / 100f));

                                if (toPrice <= expectedValue && expectedValue <= fromPrice)
                                {
                                    result = true;

                                    matchedPrice = expectedValue;
                                }
                            }
                            else if (LastSeenPrice < toPrice)
                            {
                                LastSeenPrice = fromPrice;
                            }
                        }
                        break;
                    case TrailingDirectionTypes.Down:
                        {
                            if (fromPrice < LastSeenPrice)
                            {
                                LastSeenPrice = toPrice;
                            }
                            else if (toPrice <= LastSeenPrice && LastSeenPrice <= fromPrice)
                            {
                                var expectedValue = LastSeenPrice * (1f + (Percent / 100f));

                                if (expectedValue <= fromPrice)
                                {
                                    result = true;

                                    matchedPrice = fromPrice;
                                }
                                else
                                {
                                    LastSeenPrice = toPrice;
                                }
                            }
                            else if (LastSeenPrice < toPrice)
                            {
                                var expectedValue = LastSeenPrice * (1f + (Percent / 100f));

                                if (expectedValue < toPrice)
                                {
                                    result = true;

                                    matchedPrice = fromPrice;
                                }
                                else if (toPrice <= expectedValue && expectedValue <= fromPrice)
                                {
                                    result = true;

                                    matchedPrice = expectedValue;
                                }
                            }
                        }
                        break;
                    default:
                        throw new Exception("Trailing direction type value is not valid.");
                }
            }

            return result;
        }
    }
}
