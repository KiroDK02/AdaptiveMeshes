using System.Windows;
using Services.ProblemFactories;
using Services.ScriptCompilers;
using ViewModels;
using Views.Windows;
using Views.Windows.WindowServices;

namespace Views;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
        
        var windowService = new WindowService();
        var compiler = new CSharpScriptingCompiler();
        var problemFactory = new ProblemFactory();

        var mainVm = new MainWindowViewModel(
            windowService, 
            compiler, 
            problemFactory);

        var mainWindow = new MainWindow(mainVm);
        mainWindow.Show();
    }
}