using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace TestTrayWithWPF; 
public static class Startup {
   private static Dispatcher? UIDispatcher { get; set; }
   [STAThread]
   public static void Main() {
      ApplicationConfiguration.Initialize();
      UIDispatcher = Dispatcher.CurrentDispatcher;

      var trayComponent = new InteractiveTray.TrayAppContext(
         iconPath: Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Res", "favicon.ico"), 
         name: "TestName");
      // Register Test Label
      trayComponent.RegisterTrayMenuItem("Label", Color.Gray, null);
      // Register Test Button
      trayComponent.RegisterTrayMenuItem("Click Me", Color.LightGreen, (ti) => {
         ti.SetText("Clicked at " + DateTime.Now.ToLongTimeString());
         var wnd = new MainWindow();
         wnd.Show();
      });
      // Register exit button
      trayComponent.RegisterTrayMenuItem("Quit", Color.Red, (ti) => {
         Environment.Exit(0);
      });

      // Start WPF app event loop
      _ = Task.Run(async () => {
         await UIDispatcher!.BeginInvoke(() => { 
            TestTrayWithWPF.App app = new TestTrayWithWPF.App();
            app.InitializeComponent();
            app.Run();
         });
      });

      // Start WinForms tray component
      trayComponent.InitializeComponent();
      System.Windows.Forms.Application.Run(trayComponent);
   }
}
