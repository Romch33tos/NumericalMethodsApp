using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;

namespace NumericalMethodsApp
{
  public partial class LinearEquationsView : Window, ILinearEquationsView
  {
    private DataTable _matrixATable;
    private DataTable _vectorBTable;
    private DataTable _vectorXTable;
    private DataTable _executionResultsTable;

    public LinearEquationsView()
    {
      InitializeComponent();
      InitializeDataTables();

      UpdateControlsState(false);

      var model = new LinearEquationsModel();
      var presenter = new LinearEquationsPresenter(this, model);
    }

    public int MatrixRows
    {
      get => int.TryParse(RowsTextBox.Text, out int result) ? result : 0;
      set => RowsTextBox.Text = value.ToString();
    }

    public int MatrixCols
    {
      get => int.TryParse(ColsTextBox.Text, out int result) ? result : 0;
      set => ColsTextBox.Text = value.ToString();
    }

    public double[,] MatrixA
    {
      get => GetMatrixFromDataTable(_matrixATable);
      set => SetMatrixToDataTable(value, _matrixATable);
    }

    public double[] VectorB
    {
      get => GetVectorFromDataTable(_vectorBTable);
      set => SetVectorToDataTable(value, _vectorBTable);
    }

    public double[] VectorX
    {
      get => GetVectorFromDataTable(_vectorXTable);
      set => SetVectorToDataTable(value, _vectorXTable);
    }

    public bool IsGaussSelected
    {
      get => GaussCheckBox.IsChecked == true;
      set => GaussCheckBox.IsChecked = value;
    }

    public bool IsJordanGaussSelected
    {
      get => JordanGaussCheckBox.IsChecked == true;
      set => JordanGaussCheckBox.IsChecked = value;
    }

    public bool IsCramerSelected
    {
      get => CramerCheckBox.IsChecked == true;
      set => CramerCheckBox.IsChecked = value;
    }

    public event Action ApplyDimensionsClicked;
    public event Action RandomGenerationClicked;
    public event Action ImportFromCsvClicked;
    public event Action SolveClicked;
    public event Action ExportToCsvClicked;
    public event Action ClearAllClicked;
    public event Action HelpClicked;

    public void DisplaySolution(double[] solution)
    {
      VectorX = solution;
    }

    public void DisplayExecutionResults(List<ExecutionResult> results)
    {
      _executionResultsTable.Rows.Clear();
      foreach (var result in results)
      {
        string details = result.DetailedStatus ?? result.ErrorMessage ?? result.Status;
        _executionResultsTable.Rows.Add(
            result.MethodName,
            Math.Round(result.ExecutionTimeMs, 3),
            result.Status,
            details);
      }
    }

    public void ShowMessage(string message, string title, MessageBoxImage icon)
    {
      MessageBox.Show(message, title, MessageBoxButton.OK, icon);
    }

    public bool ShowConfirmation(string message, string title)
    {
      return MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
    }

    public void ClearAllData()
    {
      _matrixATable.Clear();
      _vectorBTable.Clear();
      _vectorXTable.Clear();
      _executionResultsTable.Clear();

      UpdateControlsState(false);
      RowsTextBox.Text = "";
      ColsTextBox.Text = "";
    }

    public void UpdateControlsState(bool isEnabled)
    {
      MatrixADataGrid.IsEnabled = isEnabled;
      VectorBDataGrid.IsEnabled = isEnabled;
      VectorXDataGrid.IsEnabled = isEnabled;
      RandomGenerationButton.IsEnabled = isEnabled;
      GaussCheckBox.IsEnabled = isEnabled;
      JordanGaussCheckBox.IsEnabled = isEnabled;
      CramerCheckBox.IsEnabled = isEnabled;
      SolveButton.IsEnabled = isEnabled;
      ExecutionResultsDataGrid.IsEnabled = isEnabled;
      ExportToCsvButton.IsEnabled = isEnabled;
    }

    private void InitializeDataTables()
    {
      _matrixATable = new DataTable();
      MatrixADataGrid.ItemsSource = _matrixATable.DefaultView;
      MatrixADataGrid.AutoGeneratingColumn += (sender, eventArgs) => { };

      _vectorBTable = new DataTable();
      _vectorBTable.Columns.Add("Value", typeof(double));
      VectorBDataGrid.ItemsSource = _vectorBTable.DefaultView;
      VectorBDataGrid.AutoGeneratingColumn += (sender, eventArgs) => { };

      _vectorXTable = new DataTable();
      _vectorXTable.Columns.Add("Value", typeof(double));
      VectorXDataGrid.ItemsSource = _vectorXTable.DefaultView;
      VectorXDataGrid.IsReadOnly = true;
      VectorXDataGrid.AutoGeneratingColumn += (sender, eventArgs) => { };

      _executionResultsTable = new DataTable();
      _executionResultsTable.Columns.Add("Метод", typeof(string));
      _executionResultsTable.Columns.Add("Время (мс)", typeof(double));
      _executionResultsTable.Columns.Add("Статус", typeof(string));
      _executionResultsTable.Columns.Add("Подробности", typeof(string));
      ExecutionResultsDataGrid.ItemsSource = _executionResultsTable.DefaultView;
      ExecutionResultsDataGrid.IsReadOnly = true;
      ExecutionResultsDataGrid.AutoGeneratingColumn += (sender, eventArgs) => { };
    }

    private double[,] GetMatrixFromDataTable(DataTable table)
    {
      if (table.Rows.Count == 0) return new double[0, 0];

      int rows = table.Rows.Count;
      int cols = table.Columns.Count;
      var matrix = new double[rows, cols];

      for (int rowIndex = 0; rowIndex < rows; ++rowIndex)
      {
        for (int colIndex = 0; colIndex < cols; ++colIndex)
        {
          if (double.TryParse(table.Rows[rowIndex][colIndex]?.ToString(), out double value))
          {
            matrix[rowIndex, colIndex] = Math.Round(value, 3);
          }
        }
      }

      return matrix;
    }

    private void SetMatrixToDataTable(double[,] matrix, DataTable table)
    {
      table.Clear();
      table.Columns.Clear();

      if (matrix == null) return;

      int rows = matrix.GetLength(0);
      int cols = matrix.GetLength(1);

      for (int colIndex = 0; colIndex < cols; ++colIndex)
      {
        table.Columns.Add($"X{colIndex + 1}", typeof(double));
      }

      for (int rowIndex = 0; rowIndex < rows; ++rowIndex)
      {
        var row = table.NewRow();
        for (int colIndex = 0; colIndex < cols; ++colIndex)
        {
          row[colIndex] = Math.Round(matrix[rowIndex, colIndex], 3);
        }
        table.Rows.Add(row);
      }

      MatrixADataGrid.ItemsSource = null;
      MatrixADataGrid.ItemsSource = table.DefaultView;
    }

    private double[] GetVectorFromDataTable(DataTable table)
    {
      if (table.Rows.Count == 0) return new double[0];

      var vector = new double[table.Rows.Count];
      for (int index = 0; index < table.Rows.Count; ++index)
      {
        if (double.TryParse(table.Rows[index][0]?.ToString(), out double value))
        {
          vector[index] = Math.Round(value, 3);
        }
      }

      return vector;
    }

    private void SetVectorToDataTable(double[] vector, DataTable table)
    {
      table.Clear();
      if (vector == null) return;

      foreach (var value in vector)
      {
        table.Rows.Add(Math.Round(value, 3));
      }

      if (table == _vectorBTable)
      {
        VectorBDataGrid.ItemsSource = null;
        VectorBDataGrid.ItemsSource = table.DefaultView;
      }
      else if (table == _vectorXTable)
      {
        VectorXDataGrid.ItemsSource = null;
        VectorXDataGrid.ItemsSource = table.DefaultView;
      }
    }

    private void DimensionTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
      if (!char.IsDigit(e.Text, 0))
      {
        e.Handled = true;
      }
    }

    private void DimensionTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
      var textBox = sender as TextBox;

      bool hasRows = !string.IsNullOrWhiteSpace(RowsTextBox.Text) && RowsTextBox.Text != "0";
      bool hasCols = !string.IsNullOrWhiteSpace(ColsTextBox.Text) && ColsTextBox.Text != "0";

      ApplyDimensionsButton.IsEnabled = hasRows && hasCols;
    }

    private void ApplyDimensionsButton_Click(object sender, RoutedEventArgs e)
    {
      ApplyDimensionsClicked?.Invoke();
    }

    private void RandomGenerationButton_Click(object sender, RoutedEventArgs e)
    {
      RandomGenerationClicked?.Invoke();
    }

    private void ImportFromCsvButton_Click(object sender, RoutedEventArgs e)
    {
      ImportFromCsvClicked?.Invoke();
    }

    private void SolveButton_Click(object sender, RoutedEventArgs e)
    {
      SolveClicked?.Invoke();
    }

    private void ExportToCsvButton_Click(object sender, RoutedEventArgs e)
    {
      ExportToCsvClicked?.Invoke();
    }

    private void ClearAllButton_Click(object sender, RoutedEventArgs e)
    {
      ClearAllClicked?.Invoke();
    }

    private void HelpButton_Click(object sender, RoutedEventArgs e)
    {
      HelpClicked?.Invoke();
    }
  }
}
