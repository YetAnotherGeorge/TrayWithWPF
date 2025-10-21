using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace InteractiveTray; 
public class TrayToolstripItemInteractive {
   private static NLog.Logger logger = NLog.LogManager.GetLogger("TrayElement");

   public string CurrentText { get; private set; }
   public void SetText(string text) {
      setText(text);
      CurrentText = text;
   }

   /// <summary>
   /// Does not throw, how user gets click events
   /// </summary>
   private Action<TrayToolstripItemInteractive>? userClickedCb;
   internal void NotifyClick() {
      if (userClickedCb == null)
         return;
      try {
         userClickedCb(this);
      } catch (Exception ex) {
         logger.Error($"Error in NotifyClick: {ex.Message}");
      }
   }
   /// <summary>
   ///  Set by TrayAppContext
   /// </summary>
   private Action<string> setText;
   internal TrayToolstripItemInteractive(string initialText, Action<TrayToolstripItemInteractive>? userClickedCb, Action<string> setText) {
      this.CurrentText = initialText;
      this.userClickedCb = userClickedCb;
      this.setText = setText;
   }
}
