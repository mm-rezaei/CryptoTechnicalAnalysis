using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using TechnicalAnalysisTools.Shared.Enumerations;

namespace TechnicalAnalysisTools.Shared.DataModels
{
    [Serializable]
    public class TradeSubOrderDataModel : INotifyPropertyChanged
    {
        private TradeSubOrderTypes _TradeSubOrderType;

        private TradeSubOrderTriggerModes _TradeSubOrderTriggerMode;

        private string _Alarm;

        private float _PercentAmount;

        private float _FixedAmount;

        private TradeSubOrderModes _TradeSubOrderMode;

        public TradeSubOrderTypes TradeSubOrderType
        {
            get { return _TradeSubOrderType; }
            set
            {
                if (_TradeSubOrderType != value)
                {
                    _TradeSubOrderType = value;

                    OnPropertyChanged(nameof(TradeSubOrderType));
                }
            }
        }

        public TradeSubOrderTriggerModes TradeSubOrderTriggerMode
        {
            get { return _TradeSubOrderTriggerMode; }
            set
            {
                if (_TradeSubOrderTriggerMode != value)
                {
                    _TradeSubOrderTriggerMode = value;

                    OnPropertyChanged(nameof(TradeSubOrderTriggerMode));
                }
            }
        }

        public string Alarm
        {
            get { return _Alarm; }
            set
            {
                if (_Alarm != value)
                {
                    _Alarm = value;

                    OnPropertyChanged(nameof(Alarm));
                }
            }
        }

        public float PercentAmount
        {
            get { return _PercentAmount; }
            set
            {
                if (_PercentAmount != value)
                {
                    _PercentAmount = value;

                    OnPropertyChanged(nameof(PercentAmount));
                }
            }
        }

        public float FixedAmount
        {
            get { return _FixedAmount; }
            set
            {
                if (_FixedAmount != value)
                {
                    _FixedAmount = value;

                    OnPropertyChanged(nameof(FixedAmount));
                }
            }
        }

        public TradeSubOrderModes TradeSubOrderMode
        {
            get { return _TradeSubOrderMode; }
            set
            {
                if (_TradeSubOrderMode != value)
                {
                    _TradeSubOrderMode = value;

                    OnPropertyChanged(nameof(TradeSubOrderMode));
                }
            }
        }

        public TrailingOrderDataModel TrailingOrder { get; } = new TrailingOrderDataModel();

        public GridOrderDataModel GridOrder { get; } = new GridOrderDataModel();

        [field: NonSerialized()]
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string Validation()
        {
            var result = "";

            switch (TradeSubOrderType)
            {
                case TradeSubOrderTypes.Enter:
                    {
                        if (TradeSubOrderTriggerMode != TradeSubOrderTriggerModes.Alarm)
                        {
                            result = "The enter trigger mode should be alarm.";
                        }
                        else
                        {
                            if (string.IsNullOrWhiteSpace(Alarm))
                            {
                                result = "The enter alarm was not spesified.";
                            }
                            else if (!File.Exists(Alarm))
                            {
                                result = "The enter alarm file does not exist.";
                            }

                            if (string.IsNullOrWhiteSpace(result))
                            {
                                if (TradeSubOrderMode == TradeSubOrderModes.TrailingOrder)
                                {
                                    if (TrailingOrder != null)
                                    {
                                        result = string.Format(TrailingOrder.Validation(), "enter");
                                    }
                                    else
                                    {
                                        result = "The enter trailing order reference value was not valid.";
                                    }
                                }
                            }

                            if (string.IsNullOrWhiteSpace(result))
                            {
                                if (TradeSubOrderMode == TradeSubOrderModes.GridOrder)
                                {
                                    if (GridOrder != null)
                                    {
                                        result = string.Format(GridOrder.Validation(), "enter");
                                    }
                                    else
                                    {
                                        result = "The enter grid order reference value was not valid.";
                                    }
                                }
                            }
                        }
                    }
                    break;
                case TradeSubOrderTypes.ExitTakeProfit:
                    {
                        switch (TradeSubOrderTriggerMode)
                        {
                            case TradeSubOrderTriggerModes.Alarm:
                                {
                                    if (string.IsNullOrWhiteSpace(Alarm))
                                    {
                                        result = "The take profit alarm was not spesified.";
                                    }
                                    else if (!File.Exists(Alarm))
                                    {
                                        result = "The take profit alarm file does not exist.";
                                    }
                                }
                                break;
                            case TradeSubOrderTriggerModes.Percent:
                                {
                                    if (PercentAmount <= 0 || PercentAmount > 100)
                                    {
                                        result = "The take profit percent validation was failed.";
                                    }
                                }
                                break;
                            case TradeSubOrderTriggerModes.Fixed:
                                {
                                    if (FixedAmount <= 0)
                                    {
                                        result = "The take profit fixed amount validation was failed.";
                                    }
                                }
                                break;
                        }

                        if (string.IsNullOrWhiteSpace(result))
                        {
                            if (TradeSubOrderMode == TradeSubOrderModes.TrailingOrder)
                            {
                                if (TrailingOrder != null)
                                {
                                    result = string.Format(TrailingOrder.Validation(), "take profit");
                                }
                                else
                                {
                                    result = "The take profit trailing order reference value was not valid.";
                                }
                            }
                        }

                        if (string.IsNullOrWhiteSpace(result))
                        {
                            if (TradeSubOrderMode == TradeSubOrderModes.GridOrder)
                            {
                                if (GridOrder != null)
                                {
                                    result = string.Format(GridOrder.Validation(), "take profit");
                                }
                                else
                                {
                                    result = "The take profit grid order reference value was not valid.";
                                }
                            }
                        }
                    }
                    break;
                case TradeSubOrderTypes.ExitStopLoss:
                    {
                        switch (TradeSubOrderTriggerMode)
                        {
                            case TradeSubOrderTriggerModes.Alarm:
                                {
                                    if (string.IsNullOrWhiteSpace(Alarm))
                                    {
                                        result = "The stop loss alarm was not spesified.";
                                    }
                                    else if (!File.Exists(Alarm))
                                    {
                                        result = "The stop loss alarm file does not exist.";
                                    }
                                }
                                break;
                            case TradeSubOrderTriggerModes.Percent:
                                {
                                    if (PercentAmount <= 0 || PercentAmount > 100)
                                    {
                                        result = "The stop loss percent validation was failed.";
                                    }
                                }
                                break;
                            case TradeSubOrderTriggerModes.Fixed:
                                {
                                    if (FixedAmount <= 0)
                                    {
                                        result = "The stop loss fixed amount validation was failed.";
                                    }
                                }
                                break;
                        }
                    }
                    break;
            }

            return result;
        }

        public override string ToString()
        {
            var result = "";

            switch (TradeSubOrderType)
            {
                case TradeSubOrderTypes.Enter:
                    result += "Enter Sub Order" + Environment.NewLine;
                    break;
                case TradeSubOrderTypes.ExitTakeProfit:
                    result += "Take Profit Sub Order" + Environment.NewLine;
                    break;
                case TradeSubOrderTypes.ExitStopLoss:
                    result += "Stop Loss Sub Order" + Environment.NewLine;
                    break;
            }

            result += "{" + Environment.NewLine;

            switch (TradeSubOrderType)
            {
                case TradeSubOrderTypes.Enter:
                    {
                        result += string.Format("   {0} = {1}", nameof(Alarm), Alarm) + Environment.NewLine;
                        result += string.Format("   {0} = {1}", nameof(TradeSubOrderMode), TradeSubOrderMode);

                        if (TradeSubOrderMode == TradeSubOrderModes.TrailingOrder)
                        {
                            var trailingOrderLines = TrailingOrder.ToString().Split(new string[] { Environment.NewLine }, StringSplitOptions.None).Select(p => "   " + p);

                            var trailingOrder = trailingOrderLines.Aggregate("", (current, next) => current + Environment.NewLine + next);

                            result += trailingOrder + Environment.NewLine;
                        }
                        else if (TradeSubOrderMode == TradeSubOrderModes.GridOrder)
                        {
                            var gridOrderLines = GridOrder.ToString().Split(new string[] { Environment.NewLine }, StringSplitOptions.None).Select(p => "   " + p);

                            var gridOrder = gridOrderLines.Aggregate("", (current, next) => current + Environment.NewLine + next);

                            result += gridOrder + Environment.NewLine;
                        }
                        else
                        {
                            result += Environment.NewLine;
                        }
                    }
                    break;
                case TradeSubOrderTypes.ExitTakeProfit:
                    {
                        switch (TradeSubOrderTriggerMode)
                        {
                            case TradeSubOrderTriggerModes.Alarm:
                                {
                                    result += string.Format("   {0} = {1}", nameof(Alarm), Alarm) + Environment.NewLine;
                                }
                                break;
                            case TradeSubOrderTriggerModes.Percent:
                                {
                                    result += string.Format("   {0} = {1}", nameof(PercentAmount), PercentAmount) + Environment.NewLine;
                                }
                                break;
                            case TradeSubOrderTriggerModes.Fixed:
                                {
                                    result += string.Format("   {0} = {1}", nameof(FixedAmount), FixedAmount) + Environment.NewLine;
                                }
                                break;
                        }

                        result += string.Format("   {0} = {1}", nameof(TradeSubOrderMode), TradeSubOrderMode);

                        if (TradeSubOrderMode == TradeSubOrderModes.TrailingOrder)
                        {
                            var trailingOrderLines = TrailingOrder.ToString().Split(new string[] { Environment.NewLine }, StringSplitOptions.None).Select(p => "   " + p);

                            var trailingOrder = trailingOrderLines.Aggregate("", (current, next) => current + Environment.NewLine + next);

                            result += trailingOrder + Environment.NewLine;
                        }
                        else if (TradeSubOrderMode == TradeSubOrderModes.GridOrder)
                        {
                            var gridOrderLines = GridOrder.ToString().Split(new string[] { Environment.NewLine }, StringSplitOptions.None).Select(p => "   " + p);

                            var gridOrder = gridOrderLines.Aggregate("", (current, next) => current + Environment.NewLine + next);

                            result += gridOrder + Environment.NewLine;
                        }
                        else
                        {
                            result += Environment.NewLine;
                        }
                    }
                    break;
                case TradeSubOrderTypes.ExitStopLoss:
                    {
                        switch (TradeSubOrderTriggerMode)
                        {
                            case TradeSubOrderTriggerModes.Alarm:
                                {
                                    result += string.Format("   {0} = {1}", nameof(Alarm), Alarm) + Environment.NewLine;
                                }
                                break;
                            case TradeSubOrderTriggerModes.Percent:
                                {
                                    result += string.Format("   {0} = {1}", nameof(PercentAmount), PercentAmount) + Environment.NewLine;
                                }
                                break;
                            case TradeSubOrderTriggerModes.Fixed:
                                {
                                    result += string.Format("   {0} = {1}", nameof(FixedAmount), FixedAmount) + Environment.NewLine;
                                }
                                break;
                        }
                    }
                    break;
            }

            result += "}";

            return result;
        }
    }
}
