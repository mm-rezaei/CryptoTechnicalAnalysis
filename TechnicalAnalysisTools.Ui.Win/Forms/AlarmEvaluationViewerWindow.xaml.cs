using System;
using System.Windows;
using TechnicalAnalysisTools.Shared.DataModels;
using TechnicalAnalysisTools.Shared.Enumerations;
using TechnicalAnalysisTools.Shared.Helpers;

namespace TechnicalAnalysisTools.Ui.Win.Forms
{
    internal partial class AlarmEvaluationViewerWindow : Window
    {
        public AlarmEvaluationViewerWindow(AlarmItemDataModel alarmItem, string name, SymbolTypes symbol, DateTime triggerDateTime)
        {
            InitializeComponent();

            Title = Title + " : " + name + " : " + symbol.ToString() + " : " + DateTimeHelper.ConvertDateTimeToString(triggerDateTime);

            TreeViewAlarmRules.Items.Add(alarmItem);
        }
    }
}
