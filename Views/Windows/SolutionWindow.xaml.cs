using System.Windows;

namespace Views.Windows;

public partial class SolutionWindow : Window
{
    public SolutionWindow()
    {
        InitializeComponent();
        
        Loaded += (sender, args) =>
        {
            SolutionPlot.UserInputProcessor.DoubleLeftClickBenchmark(false);
            SolutionPlot.Plot.Axes.SquareUnits();
        };
    }
}