using System;
using System.ComponentModel;
using TechnicalAnalysisTools.Shared.Delegates;
using TechnicalAnalysisTools.Shared.Enumerations;

namespace TechnicalAnalysisTools.Shared.DataModels
{
    [Serializable]
    public class StrategyTestDataModel : INotifyPropertyChanged
    {
        private Guid _Id;

        private string _Name;

        private SymbolTypes _Symbol;

        private DateTime _FromDateTime;

        private DateTime _ToDateTime;

        private PositionTypes _Position;

        private StrategyTestPriceMovementFlowModes _StrategyTestPriceMovementFlowMode;

        private float _InitialBaseCoinDeposit;

        private float _MarketFeePercent;

        private byte _Leverage;

        private bool _VisualMode;

        private IndicatorType[] _VisualIndicators;

        private DateTime _VisualSkipToDateTime;

        private TimeFrames _VisualTimeFrame;

        private TimeFrames _VisualTickFrame;

        private int _VisualTickPerSecond;

        private float _SaveProfitPercentOfWinPosition;

        private TradeAmountModes _TradeAmountMode;

        private float _TradeAmountPercent;

        private float _TradeAmountFixedValue;

        // General Info
        public Guid Id
        {
            get { return _Id; }
            set
            {
                if (_Id != value)
                {
                    _Id = value;

                    OnPropertyChanged(nameof(Id));
                }
            }
        }

        public string Name
        {
            get { return _Name; }
            set
            {
                if (_Name != value)
                {
                    _Name = value;

                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        public SymbolTypes Symbol
        {
            get { return _Symbol; }
            set
            {
                if (_Symbol != value)
                {
                    _Symbol = value;

                    OnPropertyChanged(nameof(Symbol));
                }
            }
        }

        public DateTime FromDateTime
        {
            get { return _FromDateTime; }
            set
            {
                if (_FromDateTime != value)
                {
                    _FromDateTime = value;

                    OnPropertyChanged(nameof(FromDateTime));
                }
            }
        }

        public DateTime ToDateTime
        {
            get { return _ToDateTime; }
            set
            {
                if (_ToDateTime != value)
                {
                    _ToDateTime = value;

                    OnPropertyChanged(nameof(ToDateTime));
                }
            }
        }

        public PositionTypes Position
        {
            get { return _Position; }
            set
            {
                if (_Position != value)
                {
                    _Position = value;

                    OnPropertyChanged(nameof(Position));
                }
            }
        }

        public StrategyTestPriceMovementFlowModes StrategyTestPriceMovementFlowMode
        {
            get { return _StrategyTestPriceMovementFlowMode; }
            set
            {
                if (_StrategyTestPriceMovementFlowMode != value)
                {
                    _StrategyTestPriceMovementFlowMode = value;

                    OnPropertyChanged(nameof(StrategyTestPriceMovementFlowMode));
                }
            }
        }

        public float InitialBaseCoinDeposit
        {
            get { return _InitialBaseCoinDeposit; }
            set
            {
                if (_InitialBaseCoinDeposit != value)
                {
                    _InitialBaseCoinDeposit = value;

                    OnPropertyChanged(nameof(InitialBaseCoinDeposit));
                }
            }
        }

        // Market Info
        public float MarketFeePercent
        {
            get { return _MarketFeePercent; }
            set
            {
                if (_MarketFeePercent != value)
                {
                    _MarketFeePercent = value;

                    OnPropertyChanged(nameof(MarketFeePercent));
                }
            }
        }

        public byte Leverage
        {
            get { return _Leverage; }
            set
            {
                if (_Leverage != value)
                {
                    _Leverage = value;

                    OnPropertyChanged(nameof(Leverage));
                }
            }
        }

        // Visual Mode Info
        public bool VisualMode
        {
            get { return _VisualMode; }
            set
            {
                if (_VisualMode != value)
                {
                    _VisualMode = value;

                    OnPropertyChanged(nameof(VisualMode));
                }
            }
        }

        public IndicatorType[] VisualIndicators
        {
            get { return _VisualIndicators; }
            set
            {
                if (_VisualIndicators != value)
                {
                    _VisualIndicators = value;

                    OnPropertyChanged(nameof(VisualIndicators));
                }
            }
        }

        public DateTime VisualSkipToDateTime
        {
            get { return _VisualSkipToDateTime; }
            set
            {
                if (_VisualSkipToDateTime != value)
                {
                    _VisualSkipToDateTime = value;

                    OnPropertyChanged(nameof(VisualSkipToDateTime));
                }
            }
        }

        public TimeFrames VisualTimeFrame
        {
            get { return _VisualTimeFrame; }
            set
            {
                if (_VisualTimeFrame != value)
                {
                    _VisualTimeFrame = value;

                    OnPropertyChanged(nameof(VisualTimeFrame));
                }
            }
        }

        public TimeFrames VisualTickFrame
        {
            get { return _VisualTickFrame; }
            set
            {
                if (_VisualTickFrame != value)
                {
                    _VisualTickFrame = value;

                    OnPropertyChanged(nameof(VisualTickFrame));
                }
            }
        }

        public int VisualTickPerSecond
        {
            get { return _VisualTickPerSecond; }
            set
            {
                if (_VisualTickPerSecond != value)
                {
                    _VisualTickPerSecond = value;

                    OnPropertyChanged(nameof(VisualTickPerSecond));
                }
            }
        }

        // Capital Management
        public float SaveProfitPercentOfWinPosition
        {
            get { return _SaveProfitPercentOfWinPosition; }
            set
            {
                if (_SaveProfitPercentOfWinPosition != value)
                {
                    _SaveProfitPercentOfWinPosition = value;

                    OnPropertyChanged(nameof(SaveProfitPercentOfWinPosition));
                }
            }
        }

        public TradeAmountModes TradeAmountMode
        {
            get { return _TradeAmountMode; }
            set
            {
                if (_TradeAmountMode != value)
                {
                    _TradeAmountMode = value;

                    OnPropertyChanged(nameof(TradeAmountMode));
                }
            }
        }

        public float TradeAmountPercent
        {
            get { return _TradeAmountPercent; }
            set
            {
                if (_TradeAmountPercent != value)
                {
                    _TradeAmountPercent = value;

                    OnPropertyChanged(nameof(TradeAmountPercent));
                }
            }
        }

        public float TradeAmountFixedValue
        {
            get { return _TradeAmountFixedValue; }
            set
            {
                if (_TradeAmountFixedValue != value)
                {
                    _TradeAmountFixedValue = value;

                    OnPropertyChanged(nameof(TradeAmountFixedValue));
                }
            }
        }

        // Trade Positions
        public TradeSubOrderDataModel Enter { get; } = new TradeSubOrderDataModel() { TradeSubOrderType = TradeSubOrderTypes.Enter };

        public TradeSubOrderDataModel ExitTakeProfit { get; } = new TradeSubOrderDataModel() { TradeSubOrderType = TradeSubOrderTypes.ExitTakeProfit };

        public TradeSubOrderDataModel ExitStopLoss { get; } = new TradeSubOrderDataModel() { TradeSubOrderType = TradeSubOrderTypes.ExitStopLoss };

        //
        [field: NonSerialized()]
        public event PropertyChangedEventHandler PropertyChanged;

        [field: NonSerialized()]
        public event StrategyTestStatusUpdatedHandler StrategyTestStatusUpdated;

        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void OnStrategyTestStatusUpdated(StrategyTestStatusDataModel strategyTestStatus)
        {
            StrategyTestStatusUpdated?.Invoke(strategyTestStatus);
        }

        public string Validation()
        {
            var result = "";

            if (string.IsNullOrWhiteSpace(Name))
            {
                result = "The name field should be fill.";
            }
            else if (FromDateTime >= ToDateTime)
            {
                result = "Validation of adjusted dates for test period were failed.";
            }
            else if (InitialBaseCoinDeposit <= 0)
            {
                result = "The initial deposit was not valid.";
            }
            else if (MarketFeePercent < 0)
            {
                result = "The market fee was not valid.";
            }
            else if (Leverage <= 0)
            {
                result = "The leverage field was not valid.";
            }
            else if (SaveProfitPercentOfWinPosition < 0)
            {
                result = "The save profit percent was not valid.";
            }

            if (result == "" && VisualMode)
            {
                if (VisualSkipToDateTime < FromDateTime || VisualSkipToDateTime > ToDateTime)
                {
                    result = "The visual skip date was not valid.";
                }
                else if (VisualTickFrame > VisualTimeFrame)
                {
                    result = "The visual time frame was not valid.";
                }
                else if (VisualTickPerSecond < 0 || VisualTickPerSecond > 100)
                {
                    result = "The visual speed was not valid.";
                }
            }

            if (result == "")
            {
                switch (TradeAmountMode)
                {
                    case TradeAmountModes.Percent:
                        {
                            if (TradeAmountPercent <= 0 || TradeAmountPercent > 100)
                            {
                                result = "The trade amount percent was not valid.";
                            }
                        }
                        break;
                    case TradeAmountModes.PercentWithMinimumFixed:
                    case TradeAmountModes.PercentWithMaximumFixed:
                        {
                            if (TradeAmountPercent <= 0 || TradeAmountPercent > 100)
                            {
                                result = "The trade amount percent was not valid.";
                            }
                            else
                            {
                                if (TradeAmountFixedValue <= 0 || TradeAmountFixedValue > InitialBaseCoinDeposit)
                                {
                                    result = "The trade amount fixed value was not valid.";
                                }
                            }
                        }
                        break;
                    case TradeAmountModes.Fixed:
                        {
                            if (TradeAmountFixedValue <= 0 || TradeAmountFixedValue > InitialBaseCoinDeposit)
                            {
                                result = "The trade amount fixed value was not valid.";
                            }
                        }
                        break;
                }
            }

            if (result == "")
            {
                if (Enter != null)
                {
                    result = Enter.Validation();
                }
                else
                {
                    result = "The enter reference value was not valid.";
                }
            }

            if (result == "")
            {
                if (ExitTakeProfit != null)
                {
                    result = ExitTakeProfit.Validation();
                }
                else
                {
                    result = "The exit take profit reference value was not valid.";
                }
            }

            if (result == "")
            {
                if (ExitStopLoss != null)
                {
                    result = ExitStopLoss.Validation();
                }
                else
                {
                    result = "The exit stop loss reference value was not valid.";
                }
            }

            return result;
        }

        public override string ToString()
        {
            var result = "";

            //
            result += string.Format("{0} = {1}", nameof(Id), Id) + Environment.NewLine;
            result += string.Format("{0} = {1}", nameof(Name), Name) + Environment.NewLine;
            result += string.Format("{0} = {1}", nameof(Symbol), Symbol) + Environment.NewLine;
            result += string.Format("{0} = {1}", nameof(FromDateTime), FromDateTime.ToString("yyyy/MM/dd HH:mm")) + Environment.NewLine;
            result += string.Format("{0} = {1}", nameof(ToDateTime), ToDateTime.ToString("yyyy/MM/dd HH:mm")) + Environment.NewLine;
            result += string.Format("{0} = {1}", nameof(Position), Position) + Environment.NewLine;
            result += string.Format("{0} = {1}", nameof(StrategyTestPriceMovementFlowMode), StrategyTestPriceMovementFlowMode) + Environment.NewLine;
            result += string.Format("{0} = {1}", nameof(InitialBaseCoinDeposit), InitialBaseCoinDeposit) + Environment.NewLine;
            result += string.Format("{0} = {1}", nameof(MarketFeePercent), MarketFeePercent) + Environment.NewLine;
            result += string.Format("{0} = {1}", nameof(Leverage), Leverage) + Environment.NewLine;
            result += string.Format("{0} = {1}", nameof(SaveProfitPercentOfWinPosition), SaveProfitPercentOfWinPosition) + Environment.NewLine;
            result += string.Format("{0} = {1}", nameof(TradeAmountMode), TradeAmountMode) + Environment.NewLine;

            switch (TradeAmountMode)
            {
                case TradeAmountModes.Percent:
                    result += string.Format("{0} = {1}", nameof(TradeAmountPercent), TradeAmountPercent) + Environment.NewLine;
                    break;
                case TradeAmountModes.PercentWithMinimumFixed:
                case TradeAmountModes.PercentWithMaximumFixed:
                    result += string.Format("{0} = {1}", nameof(TradeAmountPercent), TradeAmountPercent) + Environment.NewLine;
                    result += string.Format("{0} = {1}", nameof(TradeAmountFixedValue), TradeAmountFixedValue) + Environment.NewLine;
                    break;
                case TradeAmountModes.Fixed:
                    result += string.Format("{0} = {1}", nameof(TradeAmountFixedValue), TradeAmountFixedValue) + Environment.NewLine;
                    break;
            }

            result += Enter.ToString() + Environment.NewLine;
            result += ExitTakeProfit.ToString() + Environment.NewLine;
            result += ExitStopLoss.ToString() + Environment.NewLine;

            return result;
        }
    }
}
