using System.Windows.Threading;

namespace InteractiveTray; 
public class TrayAppContext: System.Windows.Forms.ApplicationContext {
   private static NLog.Logger logger = NLog.LogManager.GetLogger("TrayElement");

   private Dispatcher dispatcher;
   private System.Windows.Forms.NotifyIcon trayIcon;
   private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
   private System.Drawing.Font font = new System.Drawing.Font("Arial", 14, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
   /// <summary>
   /// Win tool strip items + interactive items 
   /// </summary>
   private List<(System.Windows.Forms.ToolStripMenuItem t, TrayToolstripItemInteractive ti)> rtsItems = new List<(System.Windows.Forms.ToolStripMenuItem t, TrayToolstripItemInteractive ti)>();
   private bool started = false;

   private Action? onInit, onExit;

   /// <summary>
   /// Call this constructor from the UI thread
   /// </summary>
   /// <param name="iconPath"></param>
   /// <param name="name">Name that will appear on icon hover</param>
   public TrayAppContext(string iconPath, string name, Action? onInit = null, Action? onExit = null) {
      dispatcher = Dispatcher.CurrentDispatcher;
      trayIcon = new System.Windows.Forms.NotifyIcon();
      trayIcon.Text = name;
      trayIcon.Icon = new System.Drawing.Icon(iconPath);

      contextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
      this.onInit = onInit;
      this.onExit = onExit;
   }

   private int nextId = 0;
   /// <summary>
   /// 
   /// </summary>
   /// <param name="initialText"></param>
   /// <param name="bgColor">If null, use default </param>
   /// <param name="clickCb"></param>
   /// <returns></returns>
   /// <exception cref="InvalidOperationException"></exception>
   public TrayToolstripItemInteractive RegisterTrayMenuItem(string initialText, System.Drawing.Color? bgColor, Action<TrayToolstripItemInteractive>? clickCb) {
      if (started)
         throw new InvalidOperationException("Cannot register tray menu item after starting");

      System.Windows.Forms.ToolStripMenuItem tsmi = new System.Windows.Forms.ToolStripMenuItem();
      tsmi.Font = font;
      if (bgColor != null)
         tsmi.BackColor = bgColor.Value;
      tsmi.Name = $"ctxMenu{nextId++}";
      tsmi.Text = initialText;

      var tsmiInteractive = new TrayToolstripItemInteractive(initialText: initialText, userClickedCb: clickCb,  setText: (text) => {
         _ = Task.Run(async () => {
            try {
               await dispatcher.BeginInvoke(() => {
                  contextMenuStrip.SuspendLayout();
                  tsmi.Text = text;
                  contextMenuStrip.ResumeLayout();
               });
            } catch (Exception ex) {
               logger.Error($"Error in TrayAppContext.RegisterTrayMenuItem: {ex.Message}");
            }
         });
      
      });
      tsmi.Click += (o, e) => { tsmiInteractive.NotifyClick(); };

      rtsItems.Add((tsmi, tsmiInteractive));
      return tsmiInteractive;
   }
   /// <summary>
   /// 
   /// </summary>
   /// <param name="newIconPath"></param>
   /// <returns>self</returns>
   public TrayAppContext UpdateIcon(string newIconPath) {
      _ = Task.Run(async () => {
         try {
            await dispatcher.BeginInvoke(() => {
               contextMenuStrip.SuspendLayout();
               trayIcon.Icon = new System.Drawing.Icon(newIconPath);
               contextMenuStrip.ResumeLayout();
            });
         } catch (Exception ex) {
            logger.Error($"Error in TrayAppContext.UpdateIcon: {ex.Message}");
         }
      });
      return this;
   }

   private bool initialized = false;   
   public void InitializeComponent() {
      if (initialized) {
         throw new InvalidOperationException("Cannot initialize tray app context more than once");
      }
      initialized = true;
      started = true;

      contextMenuStrip.SuspendLayout();
      contextMenuStrip.Items.AddRange(rtsItems.ConvertAll(x => x.t).ToArray());
      contextMenuStrip.Name = "trayIconContextMenu";
      contextMenuStrip.Size = new System.Drawing.Size(152, 70);

      contextMenuStrip.ResumeLayout(false);
      trayIcon.ContextMenuStrip = contextMenuStrip;
      trayIcon.Visible = true;
      onInit?.Invoke();
   }

   public void OnApplicationExit() {
      trayIcon.Visible = false;
      onExit?.Invoke();
   }
}
