using Services.WindowServices;
using ViewModels.PlotViewModels;

namespace Views.Windows.WindowServices;

public class WindowService : IWindowService
{
    public void ShowSolutionWindow(object vm)
    {
        if (vm is not SolutionPlotViewModel solutionPlotViewModel)
            return;
        
        var win = new SolutionWindow()
        {
            DataContext = solutionPlotViewModel
        };
        
        solutionPlotViewModel.SetPlot(win.SolutionPlot);
        win.Show();
    }
}