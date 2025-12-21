using System.Windows;
using ViewModels;

namespace Views.Windows;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow(MainWindowViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
        vm.MeshPlotViewModel.SetWpfPlot(MainMeshPlot);
        
        Loaded += (sender, args) =>
        {
            MainMeshPlot.UserInputProcessor.DoubleLeftClickBenchmark(false);
            MainMeshPlot.Plot.Axes.SquareUnits();
        };
    }
}