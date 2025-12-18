using Microsoft.Win32;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Globalization;

namespace NumericalMethodsApp.LeastSquaresMethod
{
  public partial class LeastSquaresView : Window, ILeastSquaresView
  {
    public event RoutedEventHandler DimensionChanged;
    public event RoutedEventHandler ApplyDimensionClicked;
    public event RoutedEventHandler CalculateClicked;
    public event RoutedEventHandler ClearAllClicked;
    public event RoutedEventHandler RandomGenerateClicked;
    public event RoutedEventHandler ImportCsvClicked;
    public event RoutedEventHandler ImportGoogleSheetsClicked;
    public event RoutedEventHandler HelpClicked;
    public event EventHandler<DataGridCellEditEndingEventArgs> CellEditEnding;

    private LeastSquaresPresenter presenter;

    public LeastSquaresView()
    {
      InitializeComponent();
      InitializePlot();

      presenter = new LeastSquaresPresenter(this);
    }

    public string DimensionText
    {
      get => DimensionTextBox.Text;
      set => DimensionTextBox.Text = value;
    }

    public string RangeStartText
    {
      get => RangeStartTextBox.Text;
      set => RangeStartTextBox.Text = value;
    }

    public string RangeEndText
    {
      get => RangeEndTextBox.Text;
      set => RangeEndTextBox.Text = value;
    }

    public string PrecisionText
    {
      get => PrecisionTextBox.Text;
      set => PrecisionTextBox.Text = value;
    }

    public string ResultText
    {
      get => ResultTextBox.Text;
      set => ResultTextBox.Text = value;
    }

    public bool IsDataGridEnabled
    {
      get => PointsDataGrid.IsEnabled;
      set => PointsDataGrid.IsEnabled = value;
    }

    private void InitializePlot()
    {
      var plotModel = new PlotModel { Title = "" };

      plotModel.Axes.Add(new LinearAxis
      {
        Position = AxisPosition.Bottom,
        Title = "x",
        FontSize = 14,
        MajorGridlineStyle = LineStyle.Solid,
        MajorGridlineColor = OxyColor.FromArgb(40, 0, 0, 0),
        MinorGridlineStyle = LineStyle.Dot,
        MinorGridlineColor = OxyColor.FromArgb(20, 0, 0, 0)
      });

      plotModel.Axes.Add(new LinearAxis
      {
        Position = AxisPosition.Left,
        Title = "y",
        FontSize = 14,
        MajorGridlineStyle = LineStyle.Solid,
        MajorGridlineColor = OxyColor.FromArgb(40, 0, 0, 0),
        MinorGridlineStyle = LineStyle.Dot,
        MinorGridlineColor = OxyColor.FromArgb(20, 0, 0, 0)
      });

      MainPlot.Model = plotModel;
    }

    public void UpdateDataGrid(List<GridDataPoint> dataPoints)
    {
      PointsDataGrid.ItemsSource = dataPoints;
      PointsDataGrid.Items.Refresh();
    }

    public void UpdatePlot(List<DataPoint> points, double[] coefficients)
    {
      var plotModel = new PlotModel { Title = "" };

      plotModel.Axes.Add(new LinearAxis
      {
        Position = AxisPosition.Bottom,
        Title = "x",
        FontSize = 14,
        MajorGridlineStyle = LineStyle.Solid,
        MajorGridlineColor = OxyColor.FromArgb(40, 0, 0, 0),
        MinorGridlineStyle = LineStyle.Dot,
        MinorGridlineColor = OxyColor.FromArgb(20, 0, 0, 0)
      });

      plotModel.Axes.Add(new LinearAxis
      {
        Position = AxisPosition.Left,
        Title = "y",
        FontSize = 14,
        MajorGridlineStyle = LineStyle.Solid,
        MajorGridlineColor = OxyColor.FromArgb(40, 0, 0, 0),
        MinorGridlineStyle = LineStyle.Dot,
        MinorGridlineColor = OxyColor.FromArgb(20, 0, 0, 0)
      });

      if (points.Count > 0)
      {
        var scatterSeries = new ScatterSeries
        {
          Title = "Точки",
          MarkerType = MarkerType.Circle,
          MarkerSize = 6,
          MarkerFill = OxyColors.Red,
          MarkerStroke = OxyColors.DarkRed,
          MarkerStrokeThickness = 1
        };

        foreach (var point in points)
        {
          scatterSeries.Points.Add(new ScatterPoint(point.X, point.Y));
        }

        plotModel.Series.Add(scatterSeries);
      }

      if (points.Count >= 2 && coefficients != null)
      {
        var lineSeries = new LineSeries
        {
          Title = "Аппроксимация",
          Color = OxyColors.Blue,
          StrokeThickness = 2
        };

        double minX = points.Min(point => point.X);
        double maxX = points.Max(point => point.X);
        double step = (maxX - minX) / 200;

        for (double xValue = minX; xValue <= maxX; xValue += step)
        {
          double yValue = EvaluatePolynomial(xValue, coefficients);
          lineSeries.Points.Add(new OxyPlot.DataPoint(xValue, yValue));
        }

        plotModel.Series.Add(lineSeries);
      }

      MainPlot.Model = plotModel;
    }

    private double EvaluatePolynomial(double xValue, double[] coefficients)
    {
      double result = 0;
      for (int coefficientIndex = 0; coefficientIndex < coefficients.Length; ++coefficientIndex)
      {
        result += coefficients[coefficientIndex] * Math.Pow(xValue, coefficientIndex);
      }
      return result;
    }

    public MessageBoxResult ShowMessage(string message, string caption, MessageBoxType messageBoxType)
    {
      MessageBoxImage image = messageBoxType switch
      {
        MessageBoxType.Information => MessageBoxImage.Information,
        MessageBoxType.Error => MessageBoxImage.Error,
        MessageBoxType.Question => MessageBoxImage.Question,
        _ => MessageBoxImage.None
      };

      MessageBoxButton button = messageBoxType == MessageBoxType.Question ?
          MessageBoxButton.YesNo : MessageBoxButton.OK;

      return MessageBox.Show(message, caption, button, image);
    }

    public void ClearPlot()
    {
      InitializePlot();
    }

    private void DimensionTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
      DimensionChanged?.Invoke(sender, e);
    }

    private void ApplyDimensionButton_Click(object sender, RoutedEventArgs e)
    {
      ApplyDimensionClicked?.Invoke(sender, e);
    }

    private void CalculateButton_Click(object sender, RoutedEventArgs e)
    {
      CalculateClicked?.Invoke(sender, e);
    }

    private void ClearAllButton_Click(object sender, RoutedEventArgs e)
    {
      ClearAllClicked?.Invoke(sender, e);
    }

    private void RandomGenerateButton_Click(object sender, RoutedEventArgs e)
    {
      RandomGenerateClicked?.Invoke(sender, e);
    }

    private void ImportCsvButton_Click(object sender, RoutedEventArgs e)
    {
      ImportCsvClicked?.Invoke(sender, e);
    }

    private void ImportGoogleSheetsButton_Click(object sender, RoutedEventArgs e)
    {
      ImportGoogleSheetsClicked?.Invoke(sender, e);
    }

    private void HelpButton_Click(object sender, RoutedEventArgs e)
    {
      HelpClicked?.Invoke(sender, e);
    }

    private void PointsDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
    {
      CellEditEnding?.Invoke(sender, e);
    }
  }
}
