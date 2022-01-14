using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Media;
using TechnicalAnalysisTools.Shared.Enumerations;
using TechnicalAnalysisTools.Shared.Helpers;

namespace TechnicalAnalysisTools.Shared.DataModels
{
    [Serializable]
    public class AlarmItemDataModel : INotifyPropertyChanged, ICloneable
    {
        public AlarmItemDataModel(AlarmItemDataModel parent)
        {
            Id = Guid.NewGuid();

            Parent = parent;

            Items = new ObservableCollection<AlarmItemDataModel>();

            Parameters = new ObservableCollection<object>();

            Parameters.CollectionChanged += (sender, e) => { OnPropertyChanged(nameof(Title)); };
        }

        private Guid _Id;

        private string _PostTitle;

        private SymbolTypes _Symbol;

        private TimeFrames _TimeFrame;

        private int _CandleNumber;

        private ConditionOperations _Operation;

        private object _Tag;

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

        public string Title
        {
            get
            {
                if (OperationConditionHelper.IsLogicalOperation(Operation))
                {
                    var result = Operation.ToString();

                    if (Parameters.Count != 0)
                    {
                        var parameters = "";

                        foreach (var param in Parameters)
                        {
                            if (parameters != "")
                            {
                                parameters += ", ";
                            }

                            parameters += param.ToString();
                        }

                        result += string.Format(" ({0})", parameters);
                    }

                    if (!string.IsNullOrWhiteSpace(PostTitle))
                    {
                        result += ", " + PostTitle;
                    }

                    return result;
                }
                else
                {
                    string result = "";

                    if (Parent != null)
                    {
                        var isParentNumberPeriodicOperation = false;
                        var isParentTimeFramePeriodicOperation = false;

                        var currentParent = Parent;

                        while (currentParent != null)
                        {
                            if (OperationConditionHelper.NumberPeriodicOperations.Contains(currentParent.Operation))
                            {
                                isParentNumberPeriodicOperation = true;
                            }

                            if (OperationConditionHelper.TimeFramePeriodicOperations.Contains(currentParent.Operation))
                            {
                                isParentTimeFramePeriodicOperation = true;
                            }

                            currentParent = currentParent.Parent;
                        }

                        if (isParentNumberPeriodicOperation && isParentTimeFramePeriodicOperation)
                        {
                            result = Symbol + " : " + "N" + " : " + "T" + " : " + Operation;
                        }
                        else if (isParentNumberPeriodicOperation)
                        {
                            result = Symbol + " : " + "N" + " : " + TimeFrame + " : " + Operation;
                        }
                        else if (isParentTimeFramePeriodicOperation)
                        {
                            result = Symbol + " : " + CandleNumber + " : " + "T" + " : " + Operation;
                        }
                    }

                    if (result == "")
                    {
                        result = Symbol + " : " + CandleNumber + " : " + TimeFrame + " : " + Operation;
                    }

                    if (Parameters.Count != 0)
                    {
                        var parameters = "";

                        foreach (var param in Parameters)
                        {
                            if (parameters != "")
                            {
                                parameters += ", ";
                            }

                            parameters += param.ToString();
                        }

                        result += string.Format(" ({0})", parameters);
                    }

                    return result;
                }
            }
        }

        public string PostTitle
        {
            get { return _PostTitle; }
            set
            {
                if (_PostTitle != value)
                {
                    _PostTitle = value;

                    OnPropertyChanged(nameof(PostTitle));
                    OnPropertyChanged(nameof(Title));
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
                    OnPropertyChanged(nameof(Title));
                }
            }
        }

        public TimeFrames TimeFrame
        {
            get { return _TimeFrame; }
            set
            {
                if (_TimeFrame != value)
                {
                    _TimeFrame = value;

                    OnPropertyChanged(nameof(TimeFrame));
                    OnPropertyChanged(nameof(Title));
                }
            }
        }

        public int CandleNumber
        {
            get { return _CandleNumber; }
            set
            {
                if (_CandleNumber != value)
                {
                    _CandleNumber = value;

                    OnPropertyChanged(nameof(CandleNumber));
                    OnPropertyChanged(nameof(Title));
                }
            }
        }

        public ConditionOperations Operation
        {
            get { return _Operation; }
            set
            {
                if (_Operation != value)
                {
                    _Operation = value;

                    OnPropertyChanged(nameof(Operation));
                    OnPropertyChanged(nameof(Title));
                    OnPropertyChanged(nameof(TreeItemColor));
                }
            }
        }

        public object Tag
        {
            get { return _Tag; }
            set
            {
                if (_Tag != value)
                {
                    _Tag = value;

                    OnPropertyChanged(nameof(Tag));
                }
            }
        }

        public SolidColorBrush TreeItemColor
        {
            get
            {
                if (OperationConditionHelper.IsLogicalOperation(Operation))
                {
                    return Brushes.Red;
                }
                else
                {
                    return Brushes.Gray;
                }
            }
        }

        public AlarmItemDataModel Parent { get; set; }

        public ObservableCollection<object> Parameters { get; set; }

        public ObservableCollection<AlarmItemDataModel> Items { get; set; }

        [field: NonSerialized()]
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public object Clone()
        {
            AlarmItemDataModel result;

            var formatter = new BinaryFormatter();

            using (var stream = new MemoryStream())
            {
                formatter.Serialize(stream, this);

                stream.Seek(0, SeekOrigin.Begin);

                result = (AlarmItemDataModel)formatter.Deserialize(stream);
            }

            result.Parameters.Clear();
            result.Items.Clear();

            foreach (var p in Parameters)
            {
                result.Parameters.Add(p);
            }

            foreach (var i in Items)
            {
                var item = (AlarmItemDataModel)i.Clone();

                item.Parent = result;

                result.Items.Add(item);
            }

            return result;
        }
    }
}
