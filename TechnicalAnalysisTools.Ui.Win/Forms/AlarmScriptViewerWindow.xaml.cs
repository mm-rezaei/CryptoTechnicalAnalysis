using System.Windows;

namespace TechnicalAnalysisTools.Ui.Win.Forms
{
    internal partial class AlarmScriptViewerWindow : Window
    {
        public AlarmScriptViewerWindow(string name, string script)
        {
            InitializeComponent();

            TextBoxAlarmRules.Text = script;

            Title = Title + " : " + name;
        }
    }
}
