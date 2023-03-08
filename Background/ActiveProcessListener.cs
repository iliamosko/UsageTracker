using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UsageTracker.Entities;

namespace UsageTracker.Background
{
    internal class ActiveProcessListener
    {
        Panel processNamePanel;
        Panel progressBarPanel;
        Panel timeSpentPanel;
        Panel activeProcessNamePanel;
        Panel activeProgressBarPanel;
        Panel activeTimeSpentPanel;

        public ProcessManager processManager;

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        public ActiveProcessListener(Panel processNamePanel, Panel progressBarPanel, Panel timeSpentPanel,
            Panel activeProcessNamePanel, Panel activeProgressBarPanel, Panel activeTimeSpentPanel)
        {
            this.processNamePanel = processNamePanel;
            this.progressBarPanel = progressBarPanel;
            this.timeSpentPanel = timeSpentPanel;
            this.activeProcessNamePanel = activeProcessNamePanel;
            this.activeProgressBarPanel = activeProgressBarPanel;
            this.activeTimeSpentPanel = activeTimeSpentPanel;

            processManager = new ProcessManager();
        }

        public void TrackProcess()
        {
            var activeProcess = GetActiveWindowTitle();

            if (!processManager.ContainsProcess(activeProcess))
            {
                AddProcess(activeProcess);
            } 
            else
            {
                UpdateProcess(activeProcess);
            }
        }

        void AddProcess(string processName)
        {
            var proc = new ActiveProcess(processName, processNamePanel, progressBarPanel, timeSpentPanel); // ActiveProcess should not have panel dependencies
            var activeProc = processManager.GetActiveProcess();

            if (activeProc != null)
            {
                SetInactivePanels(activeProc);
                processManager.ChangeActiveProcess(proc);
                SetActivePanels(proc);
            }
            else
            {
                SetActivePanels(proc);
                processManager.AddProcess(proc);
            }
        }

        void UpdateProcess(string processName)
        {
            var activeProcess = processManager.GetActiveProcess();
            //look at processtracker, this needs to update the timers for the active processes
            if (!processName.Equals(activeProcess.ProcessName))
            {
                // change active process
                SetInactivePanels(activeProcess);
                processManager.ChangeActiveProcess(processManager.GetProcessFromStorage(processName));
                SetActivePanels(processManager.GetActiveProcess());
                processManager.UpdateActiveProcessBar();
            }
            else
            {
                processManager.UpdateActiveProcessBar();
            }
        }

        private void SetActivePanels(ActiveProcess proc)
        {
            activeProcessNamePanel.Controls.Add(proc.ProcessNamePanel);
            activeProgressBarPanel.Controls.Add(proc.ProcessProgressPanel);
            activeTimeSpentPanel.Controls.Add(proc.ProcessTimePanel);
        }

        private void SetInactivePanels(ActiveProcess proc)
        {
            processNamePanel.Controls.Add(proc.ProcessNamePanel);
            progressBarPanel.Controls.Add(proc.ProcessProgressPanel);
            timeSpentPanel.Controls.Add(proc.ProcessTimePanel);
        }

        private string GetActiveWindowTitle()
        {
            const int nChars = 256;
            StringBuilder Buff = new StringBuilder(nChars);
            IntPtr handle = GetForegroundWindow();

            if (GetWindowText(handle, Buff, nChars) > 0)
            {
                //string buffString = Buff.ToString();
                //string trimmedString = String.Concat(buffString.Where(c => !Char.IsWhiteSpace(c)));
                //string[] windowTitleArray = trimmedString.Split('-');
                //string windowTitle = windowTitleArray[windowTitleArray.Length - 1] + windowTitleArray[windowTitleArray.Length - 2];

                //return windowTitle;
                return Buff.ToString();
            }
            return null;
        }
    }
}
