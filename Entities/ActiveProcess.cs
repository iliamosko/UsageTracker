using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UsageTracker.Entities
{
    internal class ActiveProcess
    {
        public string ProcessName { get; set; }

        Stopwatch stopwatch;

        Label timeSpent;
        ProgressBar processBar;

        public Panel ProcessNamePanel { get; set; }

        public Panel ProcessProgressPanel { get; set; }

        public Panel ProcessTimePanel { get; set; }

        public ActiveProcess(string processName, Panel processNamePanel, Panel progressBarPanel, Panel timeSpentPanel)
        {
            ProcessName = processName;
            stopwatch = new Stopwatch();
            CreateNamePanel(processNamePanel);
            CreateProgressPanel(progressBarPanel);
            CreateTimePanel(timeSpentPanel);
        }

        public void Start()
        {
            stopwatch.Start();
        }

        public void Stop()
        {
            if (stopwatch.IsRunning)
                stopwatch.Stop();
        }

        public void UpdateTime()
        {
            // When updatating time there is a lag of ~1 second, even when switching processes.
            timeSpent.Text = $"{stopwatch.Elapsed.Hours:00}:{stopwatch.Elapsed.Minutes:00}:{stopwatch.Elapsed.Seconds:00}";
            UpdateProgressBar();
        }

        private void UpdateProgressBar()
        {
            Debug.WriteLine("Process: " + ProcessName);
            Debug.WriteLine(TrackingPage.TimeDifference);
            Debug.WriteLine("Total percentage of usage:" + Convert.ToInt32(stopwatch.Elapsed.TotalMilliseconds / TrackingPage.TimeDifference.TotalMilliseconds * 100));
            SetProgressBarValue(Convert.ToInt32(stopwatch.Elapsed.TotalMilliseconds / TrackingPage.TimeDifference.TotalMilliseconds * 100));
        }

        // set to private afterwards
        public void SetProgressBarValue(int value)
        {
            processBar.Value = value;
        }

        public int GetProgressBarValue() => processBar.Value;

        void CreateNamePanel(Panel processNamePanel)
        {
            ProcessNamePanel = new Panel
            {
                Size = new Size(120, 30),
                Dock = DockStyle.Top
            };

            Label processLabel = new Label
            {
                Text = ProcessName,
                ForeColor = Color.White,
                Location = new Point(32, 10)
            };

            ProcessNamePanel.Controls.Add(processLabel);
            //processNamePanel.Controls.Add(ProcessPanel);
        }

        void CreateProgressPanel(Panel progressBarPanel)
        {
            ProcessProgressPanel = new Panel
            {
                Size = new Size (411, 30),
                Dock=DockStyle.Top,
                Name = ProcessName
            };
            processBar = new ProgressBar
            {
                Location = new Point(6, 4),
                Size = new Size(399, 23),
                Value = 0,
            };

            ProcessProgressPanel.Controls.Add(processBar);
            //progressBarPanel.Controls.Add(ProcessProgressPanel);
        }

        void CreateTimePanel(Panel timeSpentPanel)
        {
            ProcessTimePanel = new Panel
            {
                Size= new Size(117, 30),
                Dock = DockStyle.Top,
                Name = ProcessName
            };
            timeSpent = new Label
            {
                Text = "00:00:00",
                ForeColor = Color.White,
                Location = new Point(32, 10)
            };

            ProcessTimePanel.Controls.Add(timeSpent);
            //timeSpentPanel.Controls.Add(ProcessTimePanel);
        }
    }
}
