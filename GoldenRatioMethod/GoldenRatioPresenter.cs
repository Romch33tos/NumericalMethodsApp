using System;
using NumericalMethodsApp.Models;
using NumericalMethodsApp.Views;

namespace NumericalMethodsApp.Presenters
{
  public class GoldenRatioPresenter
  {
    private readonly IGoldenRatioView _view;
    private readonly GoldenRatioModel _model;

    public GoldenRatioPresenter(IGoldenRatioView view)
    {
      _view = view;
      _model = new GoldenRatioModel();
      SubscribeToEvents();
    }

    private void SubscribeToEvents()
    {
      _view.CalculateRequested += OnCalculateRequested;
      _view.ClearAllRequested += OnClearAllRequested;
      _view.HelpRequested += OnHelpRequested;
      _view.ModeChanged += OnModeChanged;
    }

    private void OnCalculateRequested(object sender, EventArgs e)
    {
    }

    private void OnClearAllRequested(object sender, EventArgs e)
    {
    }

    private void OnHelpRequested(object sender, EventArgs e)
    {
    }

    private void OnModeChanged(object sender, EventArgs e)
    {
    }
  }
}
