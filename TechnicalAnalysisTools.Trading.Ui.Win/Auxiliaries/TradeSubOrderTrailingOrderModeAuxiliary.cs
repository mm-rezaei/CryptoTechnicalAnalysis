using TechnicalAnalysisTools.Trading.Ui.Win.Enumerations;

namespace TechnicalAnalysisTools.Trading.Ui.Win.Auxiliaries
{
    internal class TradeSubOrderTrailingOrderModeAuxiliary : TradeSubOrderModeAuxiliaryBase
    {
        public TradeSubOrderTrailingOrderModeAuxiliary(decimal primaryPrice, TrailingDirectionTypes trailingDirection, decimal percent) : base(primaryPrice)
        {
            TrailingDirection = trailingDirection;

            Percent = percent;

            LastSeenPrice = primaryPrice;
        }

        private TrailingDirectionTypes TrailingDirection { get; set; }

        private decimal Percent { get; set; }

        private decimal LastSeenPrice { get; set; }

        public override bool CheckPrice(decimal price)
        {
            var result = false;

            switch (TrailingDirection)
            {
                case TrailingDirectionTypes.Up:
                    {
                        if (price >= LastSeenPrice)
                        {
                            LastSeenPrice = price;
                        }
                        else
                        {
                            var expectedValue = LastSeenPrice * (1m - (Percent / 100m));

                            if (price <= expectedValue)
                            {
                                result = true;
                            }
                        }
                    }
                    break;
                case TrailingDirectionTypes.Down:
                    {
                        if (price <= LastSeenPrice)
                        {
                            LastSeenPrice = price;
                        }
                        else
                        {
                            var expectedValue = LastSeenPrice * (1m + (Percent / 100m));

                            if (expectedValue <= price)
                            {
                                result = true;
                            }
                        }
                    }
                    break;
            }

            return result;
        }
    }
}
