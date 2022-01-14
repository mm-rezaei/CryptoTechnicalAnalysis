using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using TechnicalAnalysisTools.Shared.DataModels;
using TechnicalAnalysisTools.Shared.Enumerations;
using TechnicalAnalysisTools.Shared.Helpers;

namespace TechnicalAnalysisTools.Ui.Win.Forms
{
    internal partial class AlarmFactoryWindow : Window
    {
        public AlarmFactoryWindow(AlarmItemDataModel treeRoot = null, string name = "", SymbolTypes symbol = SymbolTypes.BtcUsdt, PositionTypes position = PositionTypes.Long)
        {
            InitializeComponent();

            InitializeControls();

            InitializeByTreeRoot(treeRoot, name, symbol, position);
        }

        private AlarmItemDataModel TreeRoot { get; set; }

        private Tuple<TextBlock[], TextBox[], ComboBox[]> MainControls { get; set; }

        private Tuple<TextBlock[], TextBox[], ComboBox[]> NewLogicalControls { get; set; }

        private Tuple<TextBlock[], TextBox[], ComboBox[]>[] Controls { get; } = new Tuple<TextBlock[], TextBox[], ComboBox[]>[3];

        private void InitializeControls()
        {
            //
            ComboBoxMainOperation.ItemsSource = OperationConditionHelper.LogicalOperations;
            ComboBoxMainSymbol.ItemsSource = OperationConditionHelper.Symbols;
            ComboBoxMainPosition.ItemsSource = OperationConditionHelper.Positions;

            ComboBoxNewLogicalOperation.ItemsSource = OperationConditionHelper.LogicalOperations;

            ComboBoxNewOperation.ItemsSource = OperationConditionHelper.NonLogicalOperations;
            ComboBoxNewOperationSymbol.ItemsSource = OperationConditionHelper.Symbols;
            ComboBoxNewOperationTimeFrame.ItemsSource = OperationConditionHelper.TimeFrames;
            ComboBoxNewOperationCandleNumber.ItemsSource = OperationConditionHelper.CandleNumbers;

            //
            ComboBoxMainOperation.SelectedIndex = 0;
            ComboBoxMainSymbol.SelectedIndex = 0;
            ComboBoxMainPosition.SelectedIndex = 0;

            ComboBoxNewLogicalOperation.SelectedIndex = 0;

            ComboBoxNewOperation.SelectedIndex = 0;

            ComboBoxNewOperationSymbol.SelectedIndex = 0;
            ComboBoxNewOperationTimeFrame.SelectedIndex = 0;
            ComboBoxNewOperationCandleNumber.SelectedIndex = 0;

            //
            SetEnabledProperties();

            //
            Controls[0] = new Tuple<TextBlock[], TextBox[], ComboBox[]>
                (
                new TextBlock[] { TextBlockMainOperand1, TextBlockMainOperand2, TextBlockMainOperand3 },
                new TextBox[] { TextBoxMainOperand1, TextBoxMainOperand2, TextBoxMainOperand3 },
                new ComboBox[] { ComboBoxMainOperand1, ComboBoxMainOperand2, ComboBoxMainOperand3 }
                );

            Controls[1] = new Tuple<TextBlock[], TextBox[], ComboBox[]>
                (
                new TextBlock[] { TextBlockNewLogicalOperationOperand1, TextBlockNewLogicalOperationOperand2, TextBlockNewLogicalOperationOperand3 },
                new TextBox[] { TextBoxNewLogicalOperationOperand1, TextBoxNewLogicalOperationOperand2, TextBoxNewLogicalOperationOperand3 },
                new ComboBox[] { ComboBoxNewLogicalOperationOperand1, ComboBoxNewLogicalOperationOperand2, ComboBoxNewLogicalOperationOperand3 }
                );

            Controls[2] = new Tuple<TextBlock[], TextBox[], ComboBox[]>
                (
                new TextBlock[] { TextBlockNewOperationOperand1, TextBlockNewOperationOperand2, TextBlockNewOperationOperand3 },
                new TextBox[] { TextBoxNewOperationOperand1, TextBoxNewOperationOperand2, TextBoxNewOperationOperand3 },
                new ComboBox[] { ComboBoxNewOperationOperand1, ComboBoxNewOperationOperand2, ComboBoxNewOperationOperand3 }
                );

            foreach (var tuple in Controls)
            {
                foreach (var control in tuple.Item1)
                {
                    control.Visibility = Visibility.Hidden;
                }

                foreach (var control in tuple.Item2)
                {
                    control.Visibility = Visibility.Hidden;
                }

                foreach (var control in tuple.Item3)
                {
                    control.Visibility = Visibility.Hidden;
                }
            }

            //
            SetOperationParameterControls(ComboBoxMainOperation, 0);
            SetOperationParameterControls(ComboBoxNewLogicalOperation, 1);
            SetOperationParameterControls(ComboBoxNewOperation, 2);
        }

        private void InitializeByTreeRoot(AlarmItemDataModel treeRoot, string name, SymbolTypes symbol, PositionTypes position)
        {
            if (treeRoot != null)
            {
                TreeRoot = treeRoot;

                TextBoxMainName.Text = name;
                ComboBoxMainSymbol.SelectedItem = symbol;
                ComboBoxMainPosition.SelectedItem = position;

                TreeRoot.PostTitle = GetRootPostTitle();

                TreeViewAlarmRules.Items.Add(TreeRoot);

                SetEnabledProperties();

                GenerateAlarmScript();
            }
        }

        private void SetEnabledProperties()
        {
            if (TreeRoot == null)
            {
                GroupBoxMain.IsEnabled = true;
                GroupBoxNewLogical.IsEnabled = false;
                GroupBoxNew.IsEnabled = false;

                ButtonMainAddOperation.IsEnabled = true;
                ButtonMainChangeOperation.IsEnabled = false;
            }
            else
            {
                if (TreeViewAlarmRules.SelectedItem == null)
                {
                    GroupBoxMain.IsEnabled = true;
                    GroupBoxNewLogical.IsEnabled = false;
                    GroupBoxNew.IsEnabled = false;

                    ButtonMainAddOperation.IsEnabled = false;
                    ButtonMainChangeOperation.IsEnabled = true;
                }
                else
                {
                    var selectedNode = (AlarmItemDataModel)TreeViewAlarmRules.SelectedItem;

                    if (selectedNode == TreeRoot)
                    {
                        if (OperationConditionHelper.UnaryOperations.Contains(selectedNode.Operation) && selectedNode.Items.Count > 0)
                        {
                            GroupBoxMain.IsEnabled = true;
                            GroupBoxNewLogical.IsEnabled = true;
                            GroupBoxNew.IsEnabled = true;

                            ButtonMainAddOperation.IsEnabled = false;
                            ButtonMainChangeOperation.IsEnabled = true;

                            ButtonAddNewLogicalOperation.IsEnabled = false;
                            ButtonChangeNewLogicalOperation.IsEnabled = false;
                            ButtonRemoveNewLogicalOperation.IsEnabled = false;

                            ButtonAddNewOperation.IsEnabled = false;
                            ButtonChangeNewOperation.IsEnabled = false;
                            ButtonRemoveNewOperation.IsEnabled = false;
                        }
                        else
                        {
                            GroupBoxMain.IsEnabled = true;
                            GroupBoxNewLogical.IsEnabled = true;
                            GroupBoxNew.IsEnabled = true;

                            ButtonMainAddOperation.IsEnabled = false;
                            ButtonMainChangeOperation.IsEnabled = true;

                            ButtonAddNewLogicalOperation.IsEnabled = true;
                            ButtonChangeNewLogicalOperation.IsEnabled = false;
                            ButtonRemoveNewLogicalOperation.IsEnabled = false;

                            ButtonAddNewOperation.IsEnabled = true;
                            ButtonChangeNewOperation.IsEnabled = false;
                            ButtonRemoveNewOperation.IsEnabled = false;
                        }
                    }
                    else
                    {
                        if (OperationConditionHelper.IsLogicalOperation(selectedNode.Operation))
                        {
                            if (OperationConditionHelper.UnaryOperations.Contains(selectedNode.Operation) && selectedNode.Items.Count > 0)
                            {
                                GroupBoxMain.IsEnabled = false;
                                GroupBoxNewLogical.IsEnabled = true;
                                GroupBoxNew.IsEnabled = true;

                                ButtonAddNewLogicalOperation.IsEnabled = false;
                                ButtonChangeNewLogicalOperation.IsEnabled = true;
                                ButtonRemoveNewLogicalOperation.IsEnabled = true;

                                ButtonAddNewOperation.IsEnabled = false;
                                ButtonChangeNewOperation.IsEnabled = false;
                                ButtonRemoveNewOperation.IsEnabled = false;
                            }
                            else
                            {
                                GroupBoxMain.IsEnabled = false;
                                GroupBoxNewLogical.IsEnabled = true;
                                GroupBoxNew.IsEnabled = true;

                                ButtonAddNewLogicalOperation.IsEnabled = true;
                                ButtonChangeNewLogicalOperation.IsEnabled = true;
                                ButtonRemoveNewLogicalOperation.IsEnabled = true;

                                ButtonAddNewOperation.IsEnabled = true;
                                ButtonChangeNewOperation.IsEnabled = false;
                                ButtonRemoveNewOperation.IsEnabled = false;
                            }
                        }
                        else
                        {
                            GroupBoxMain.IsEnabled = false;
                            GroupBoxNewLogical.IsEnabled = false;
                            GroupBoxNew.IsEnabled = true;

                            ButtonAddNewLogicalOperation.IsEnabled = false;
                            ButtonChangeNewLogicalOperation.IsEnabled = false;
                            ButtonRemoveNewLogicalOperation.IsEnabled = false;

                            ButtonAddNewOperation.IsEnabled = false;
                            ButtonChangeNewOperation.IsEnabled = true;
                            ButtonRemoveNewOperation.IsEnabled = true;
                        }
                    }
                }
            }
        }

        private void SetOperationParameterControls(ComboBox comboBox, int controlsNumber)
        {
            if (comboBox.SelectedIndex != -1)
            {
                if (Controls[controlsNumber] != null)
                {
                    var operation = (ConditionOperations)comboBox.SelectedItem;

                    var parameters = OperationConditionHelper.GetOperationParameter(operation);

                    int index = 0;

                    for (; index < parameters.Length; index++)
                    {
                        Controls[controlsNumber].Item1[index].Visibility = Visibility.Visible;
                        Controls[controlsNumber].Item1[index].Text = parameters[index].Name + ":";

                        if (parameters[index].Type.IsEnum)
                        {
                            Controls[controlsNumber].Item2[index].Visibility = Visibility.Hidden;
                            Controls[controlsNumber].Item3[index].Visibility = Visibility.Visible;

                            Controls[controlsNumber].Item3[index].ItemsSource = Enum.GetValues(parameters[index].Type);
                            Controls[controlsNumber].Item3[index].SelectedIndex = 0;
                        }
                        else if (parameters[index].Type == typeof(string))
                        {
                            Controls[controlsNumber].Item2[index].Visibility = Visibility.Visible;
                            Controls[controlsNumber].Item3[index].Visibility = Visibility.Hidden;

                            Controls[controlsNumber].Item2[index].Text = "";
                        }
                        else
                        {
                            Controls[controlsNumber].Item2[index].Visibility = Visibility.Visible;
                            Controls[controlsNumber].Item3[index].Visibility = Visibility.Hidden;

                            Controls[controlsNumber].Item2[index].Text = "0";
                        }
                    }

                    for (; index < 3; index++)
                    {
                        Controls[controlsNumber].Item1[index].Visibility = Visibility.Hidden;
                        Controls[controlsNumber].Item2[index].Visibility = Visibility.Hidden;
                        Controls[controlsNumber].Item3[index].Visibility = Visibility.Hidden;
                    }
                }
            }
        }

        private string GetRootPostTitle()
        {
            string result;

            string symbol;
            string position;

            if (ComboBoxMainSymbol.SelectedIndex != -1)
            {
                symbol = ((SymbolTypes)ComboBoxMainSymbol.SelectedValue).ToString();
            }
            else
            {
                symbol = "Symbol?";
            }

            if (ComboBoxMainPosition.SelectedIndex != -1)
            {
                position = ((PositionTypes)ComboBoxMainPosition.SelectedValue).ToString();
            }
            else
            {
                position = "Position?";
            }

            result = string.Format("{0}, {1}", symbol, position);

            return result;
        }

        private object[] GetParameterValue(ConditionOperations operation, int controlsNumber)
        {
            object[] result = new object[0];

            var parameters = OperationConditionHelper.GetOperationParameter(operation);

            if (parameters.Length != 0)
            {
                result = new object[parameters.Length];

                for (int index = 0; index < parameters.Length; index++)
                {
                    if (parameters[index].Type.IsEnum)
                    {
                        result[index] = Controls[controlsNumber].Item3[index].SelectedItem;
                    }
                    else if (parameters[index].Type == typeof(string))
                    {
                        var textValue = Controls[controlsNumber].Item2[index].Text;

                        if (!string.IsNullOrWhiteSpace(textValue) && !textValue.Contains(" "))
                        {
                            result[index] = textValue;
                        }
                        else
                        {
                            result = null;

                            MessageBox.Show("Input text is wrong. This input is not applied. Please enter a valid text.", "Failed Operation", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else if (parameters[index].Type == typeof(int))
                    {
                        int value;

                        var textValue = Controls[controlsNumber].Item2[index].Text;

                        if (int.TryParse(textValue, out value))
                        {
                            result[index] = value;
                        }
                        else
                        {
                            result = null;

                            MessageBox.Show("Input number is wrong. This input is not applied. Please enter a valid number.", "Failed Operation", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else if (parameters[index].Type == typeof(float))
                    {
                        float value;

                        var textValue = Controls[controlsNumber].Item2[index].Text;

                        if (float.TryParse(textValue, out value))
                        {
                            result[index] = value;
                        }
                        else
                        {
                            result = null;

                            MessageBox.Show("Input number is wrong. This input is not applied. Please enter a valid number.", "Failed Operation", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else if (parameters[index].Type == typeof(byte))
                    {
                        byte value;

                        var textValue = Controls[controlsNumber].Item2[index].Text;

                        if (byte.TryParse(textValue, out value))
                        {
                            result[index] = value;
                        }
                        else
                        {
                            result = null;

                            MessageBox.Show("Input number is wrong. This input is not applied. Please enter a valid number.", "Failed Operation", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }

            return result;
        }

        private void GenerateAlarmScript()
        {
            if (TreeRoot != null)
            {
                TextBoxAlarmRules.Text = AlarmHelper.ConvertAlarmItemToString(TreeRoot, TextBoxMainName.Text, (SymbolTypes)ComboBoxMainSymbol.SelectedItem, (PositionTypes)ComboBoxMainPosition.SelectedItem);
            }
        }

        private bool ValidateTree(AlarmItemDataModel treeNode)
        {
            var result = true;

            if (treeNode != null)
            {
                //
                var list = new List<AlarmItemDataModel>();
                var stack = new Stack<AlarmItemDataModel>();

                stack.Push(treeNode);

                while (stack.Count != 0)
                {
                    var dataModel = stack.Pop();

                    if (dataModel.Items != null && dataModel.Items.Count != 0)
                    {
                        foreach (var d in dataModel.Items)
                        {
                            stack.Push(d);
                        }
                    }
                    else
                    {
                        list.Add(dataModel);
                    }
                }

                //
                foreach (var node in list)
                {
                    var numberPeriodicOperation = 0;
                    var timeFramePeriodicOperation = 0;

                    var currentNode = node;

                    while (currentNode != null)
                    {
                        if (OperationConditionHelper.NumberPeriodicOperations.Contains(currentNode.Operation))
                        {
                            numberPeriodicOperation++;
                        }

                        if (OperationConditionHelper.TimeFramePeriodicOperations.Contains(currentNode.Operation))
                        {
                            timeFramePeriodicOperation++;
                        }

                        currentNode = currentNode.Parent;
                    }

                    if (numberPeriodicOperation > 1 || timeFramePeriodicOperation > 1)
                    {
                        result = false;
                    }
                }
            }

            return result;
        }

        private void OnPropertyChanged(AlarmItemDataModel treeNode, string property)
        {
            if (treeNode != null)
            {
                //
                var list = new List<AlarmItemDataModel>();
                var stack = new Stack<AlarmItemDataModel>();

                stack.Push(treeNode);

                while (stack.Count != 0)
                {
                    var dataModel = stack.Pop();

                    if (dataModel.Items != null && dataModel.Items.Count != 0)
                    {
                        foreach (var d in dataModel.Items)
                        {
                            stack.Push(d);
                        }
                    }
                    else
                    {
                        if (OperationConditionHelper.IsNonLogicalOperation(dataModel.Operation))
                        {
                            list.Add(dataModel);
                        }
                    }
                }

                //
                foreach (var node in list)
                {
                    node.OnPropertyChanged(property);
                }
            }
        }

        private void TreeViewAlarmRules_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            SetEnabledProperties();
        }

        private void ComboBoxMainOperation_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetOperationParameterControls(ComboBoxMainOperation, 0);
        }

        private void ButtonMainAddOperation_Click(object sender, RoutedEventArgs e)
        {
            var operation = (ConditionOperations)ComboBoxMainOperation.SelectedItem;

            var parameterValues = GetParameterValue(operation, 0);

            if (parameterValues != null)
            {
                TreeRoot = new AlarmItemDataModel(null) { PostTitle = GetRootPostTitle(), Operation = operation };

                foreach (var value in parameterValues)
                {
                    TreeRoot.Parameters.Add(value);
                }

                TreeViewAlarmRules.Items.Add(TreeRoot);

                SetEnabledProperties();
            }

            GenerateAlarmScript();
        }

        private void ButtonMainChangeOperation_Click(object sender, RoutedEventArgs e)
        {
            var operation = (ConditionOperations)ComboBoxMainOperation.SelectedItem;

            var parameterValues = GetParameterValue(operation, 0);

            if (parameterValues != null)
            {
                if (OperationConditionHelper.UnaryOperations.Contains(operation) && TreeRoot.Items.Count > 1)
                {
                    MessageBox.Show("Change operation to " + operation.ToString() + " operation is not possible. First, reduce childs to one or fewer!", "Failed Operation", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    var oldPostTitle = TreeRoot.PostTitle;
                    var oldOperation = TreeRoot.Operation;

                    TreeRoot.PostTitle = GetRootPostTitle();
                    TreeRoot.Operation = operation;

                    if (ValidateTree(TreeRoot))
                    {
                        TreeRoot.Parameters.Clear();

                        foreach (var value in parameterValues)
                        {
                            TreeRoot.Parameters.Add(value);
                        }

                        OnPropertyChanged(TreeRoot, nameof(AlarmItemDataModel.Title));
                    }
                    else
                    {
                        TreeRoot.PostTitle = oldPostTitle;
                        TreeRoot.Operation = oldOperation;

                        MessageBox.Show("Change operation to " + operation.ToString() + " operation is not possible!", "Failed Operation", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }

                SetEnabledProperties();
            }

            GenerateAlarmScript();
        }

        private void ButtonMainSaveOperation_Click(object sender, RoutedEventArgs e)
        {
            var fileDialog = new SaveFileDialog();

            fileDialog.FileName = TextBoxMainName.Text;

            fileDialog.Filter = "Text file (*.txt)|*.txt";
            fileDialog.CheckPathExists = true;

            var fileDialogResult = fileDialog.ShowDialog();

            if (fileDialogResult.HasValue && fileDialogResult.Value)
            {
                if (File.Exists(fileDialog.FileName))
                {
                    File.Delete(fileDialog.FileName);
                }

                GenerateAlarmScript();

                File.WriteAllText(fileDialog.FileName, TextBoxAlarmRules.Text);
            }
        }

        private void ComboBoxNewLogicalOperation_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetOperationParameterControls(ComboBoxNewLogicalOperation, 1);
        }

        private void ButtonAddNewLogicalOperation_Click(object sender, RoutedEventArgs e)
        {
            if (TreeViewAlarmRules.SelectedItem != null)
            {
                var selectedNode = (AlarmItemDataModel)TreeViewAlarmRules.SelectedItem;

                if (OperationConditionHelper.IsLogicalOperation(selectedNode.Operation))
                {
                    var operation = (ConditionOperations)ComboBoxNewLogicalOperation.SelectedItem;

                    var parameterValues = GetParameterValue(operation, 1);

                    if (parameterValues != null)
                    {
                        var newNode = new AlarmItemDataModel(selectedNode) { Operation = operation };

                        foreach (var value in parameterValues)
                        {
                            newNode.Parameters.Add(value);
                        }

                        selectedNode.Items.Add(newNode);

                        if (!ValidateTree(TreeRoot))
                        {
                            selectedNode.Items.Remove(newNode);

                            MessageBox.Show("Adding " + operation.ToString() + " operation is not possible!", "Failed Operation", MessageBoxButton.OK, MessageBoxImage.Error);
                        }

                        SetEnabledProperties();
                    }
                }
            }

            GenerateAlarmScript();
        }

        private void ButtonChangeNewLogicalOperation_Click(object sender, RoutedEventArgs e)
        {
            if (TreeViewAlarmRules.SelectedItem != null)
            {
                var selectedNode = (AlarmItemDataModel)TreeViewAlarmRules.SelectedItem;

                if (OperationConditionHelper.IsLogicalOperation(selectedNode.Operation))
                {
                    var operation = (ConditionOperations)ComboBoxNewLogicalOperation.SelectedItem;

                    var parameterValues = GetParameterValue(operation, 1);

                    if (parameterValues != null)
                    {
                        if (OperationConditionHelper.UnaryOperations.Contains(operation) && selectedNode.Items.Count > 1)
                        {
                            MessageBox.Show("Change operation to " + operation.ToString() + " operation is not possible. First, reduce childs to one or fewer!", "Failed Operation", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        else
                        {
                            var oldOperation = selectedNode.Operation;

                            selectedNode.Operation = operation;

                            if (ValidateTree(TreeRoot))
                            {
                                selectedNode.Parameters.Clear();

                                foreach (var value in parameterValues)
                                {
                                    selectedNode.Parameters.Add(value);
                                }

                                OnPropertyChanged(TreeRoot, nameof(AlarmItemDataModel.Title));
                            }
                            else
                            {
                                selectedNode.Operation = oldOperation;

                                MessageBox.Show("Change operation to " + operation.ToString() + " operation is not possible!", "Failed Operation", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }

                        SetEnabledProperties();
                    }
                }
            }

            GenerateAlarmScript();
        }

        private void ButtonRemoveNewLogicalOperation_Click(object sender, RoutedEventArgs e)
        {
            if (TreeViewAlarmRules.SelectedItem != null)
            {
                var selectedNode = (AlarmItemDataModel)TreeViewAlarmRules.SelectedItem;

                if (OperationConditionHelper.IsLogicalOperation(selectedNode.Operation))
                {
                    if (selectedNode.Parent != null)
                    {
                        selectedNode.Parent.Items.Remove(selectedNode);

                        SetEnabledProperties();
                    }
                }
            }

            GenerateAlarmScript();
        }

        private void ComboBoxNewOperation_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetOperationParameterControls(ComboBoxNewOperation, 2);
        }

        private void ButtonAddNewOperation_Click(object sender, RoutedEventArgs e)
        {
            if (TreeViewAlarmRules.SelectedItem != null)
            {
                var selectedNode = (AlarmItemDataModel)TreeViewAlarmRules.SelectedItem;

                if (OperationConditionHelper.IsLogicalOperation(selectedNode.Operation))
                {
                    var operation = (ConditionOperations)ComboBoxNewOperation.SelectedItem;

                    var parameterValues = GetParameterValue(operation, 2);

                    if (parameterValues != null)
                    {
                        if (ComboBoxNewOperationSymbol.SelectedIndex == -1 || ComboBoxNewOperationTimeFrame.SelectedIndex == -1 || ComboBoxNewOperationCandleNumber.SelectedIndex == -1)
                        {
                            MessageBox.Show("Adding operation needs some datas. First, fill the form, and then add new operation!", "Failed Operation", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        else
                        {
                            var newNode = new AlarmItemDataModel(selectedNode) { Operation = operation, Symbol = (SymbolTypes)ComboBoxNewOperationSymbol.SelectedItem, TimeFrame = (TimeFrames)ComboBoxNewOperationTimeFrame.SelectedItem, CandleNumber = (int)ComboBoxNewOperationCandleNumber.SelectedItem };

                            foreach (var value in parameterValues)
                            {
                                newNode.Parameters.Add(value);
                            }

                            selectedNode.Items.Add(newNode);
                        }

                        SetEnabledProperties();
                    }
                }
            }

            GenerateAlarmScript();
        }

        private void ButtonChangeNewOperation_Click(object sender, RoutedEventArgs e)
        {
            if (TreeViewAlarmRules.SelectedItem != null)
            {
                var selectedNode = (AlarmItemDataModel)TreeViewAlarmRules.SelectedItem;

                if (OperationConditionHelper.IsNonLogicalOperation(selectedNode.Operation))
                {
                    var operation = (ConditionOperations)ComboBoxNewOperation.SelectedItem;

                    var parameterValues = GetParameterValue(operation, 2);

                    if (parameterValues != null)
                    {
                        if (ComboBoxNewOperationSymbol.SelectedIndex == -1 || ComboBoxNewOperationTimeFrame.SelectedIndex == -1 || ComboBoxNewOperationCandleNumber.SelectedIndex == -1)
                        {
                            MessageBox.Show("Adding operation needs some datas. First, fill the form, and then add new operation!", "Failed Operation", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        else
                        {
                            selectedNode.Operation = operation;
                            selectedNode.Symbol = (SymbolTypes)ComboBoxNewOperationSymbol.SelectedItem;
                            selectedNode.TimeFrame = (TimeFrames)ComboBoxNewOperationTimeFrame.SelectedItem;
                            selectedNode.CandleNumber = (int)ComboBoxNewOperationCandleNumber.SelectedItem;

                            selectedNode.Parameters.Clear();

                            foreach (var value in parameterValues)
                            {
                                selectedNode.Parameters.Add(value);
                            }
                        }

                        SetEnabledProperties();
                    }
                }
            }

            GenerateAlarmScript();
        }

        private void ButtonRemoveNewOperation_Click(object sender, RoutedEventArgs e)
        {
            if (TreeViewAlarmRules.SelectedItem != null)
            {
                var selectedNode = (AlarmItemDataModel)TreeViewAlarmRules.SelectedItem;

                if (OperationConditionHelper.IsNonLogicalOperation(selectedNode.Operation))
                {
                    if (selectedNode.Parent != null)
                    {
                        selectedNode.Parent.Items.Remove(selectedNode);

                        SetEnabledProperties();
                    }
                }
            }

            GenerateAlarmScript();
        }
    }
}
