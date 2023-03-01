using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using static System.Windows.Forms.Control;

namespace TimeTracker
{
    public class TrackingProcess
    {
        /// <summary>
        /// Gets the process name
        /// </summary>
        public string ProcessName { get; set; }

        /// <summary>
        /// Gets the process controls
        /// </summary>
        private ControlCollection Controls { get; }

        /// <summary>
        /// Gets the location of the label
        /// </summary>
        private Point Location { get; }

        // NOTE:
        // Stopewatch does not match timer in From2 class. Should change it to timer to match the total time.
        public Stopwatch Stopwatch { get; set; }

        public Label ProcessLabel;
        public Label TimeSpent;
        public ProgressBar ProcessBar;
        public int processNum;

        /// <summary>
        /// Creates tracking information with the given process name, panel controls and the location.
        /// </summary>
        /// <param name="processName">The process name.</param>
        /// <param name="controls">The controls for the process</param>
        /// <param name="location">The location of the components</param>
        public TrackingProcess(string processName, ControlCollection controls, Point location, int processID)
        {
            ProcessName = processName;
            Controls = controls;
            Location = location;
            Stopwatch = new Stopwatch();
            processNum = processID;
            InstantiateTracking();
        }

        public int GetProcessBarHeight()
        {
            return ProcessBar.Size.Height;
        }

        public int GetProgressBarLength()
        {
            return ProcessBar.Value;
        }

        public void SetProgressBarValue(int value)
        {
            ProcessBar.Value = value;
        }

        public void StartStopwatch()
        {
            Stopwatch.Start();
        }

        public void StopStopwatch()
        {
            Stopwatch.Stop();
        }

        public void UpdateTime()
        {
            // When updatating time there is a lag of ~1 second, even when switching processes.
            TimeSpent.Text = $"{Stopwatch.Elapsed.Hours:00}:{Stopwatch.Elapsed.Minutes:00}:{Math.Ceiling((decimal)Stopwatch.Elapsed.Seconds):00}";
            UpdateProgressBar();
        }

        private void UpdateProgressBar()
        {
            //Debug.WriteLine(Tracking.TimeDifference);
            //Debug.WriteLine("Total percentage of usage:" + Convert.ToInt32(Stopwatch.Elapsed.TotalMilliseconds/Tracking.TimeDifference.TotalMilliseconds * 100));
            //SetProgressBarValue(Convert.ToInt32(Stopwatch.Elapsed.TotalMilliseconds / Tracking.TimeDifference.TotalMilliseconds * 100));


        }
        /// <summary>
        /// Renders Process name label and progress bar
        /// </summary>
        private void InstantiateTracking()
        {
            ProcessLabel = new Label
            {
                Location = Location,
                Text = ProcessName,
                AutoSize = true
            };

            ProcessBar = new ProgressBar
            {
                Location = new Point(Location.X + ProcessLabel.Size.Width, Location.Y),
                Size = new Size(300, 20),
                Value = 0,
                AutoSize = true
            };

            TimeSpent = new Label
            {
                Location = new Point(ProcessBar.Location.X + ProcessBar.Size.Width + 10, Location.Y),
                Text = $"00:00:00",
                AutoSize = true
            };


            Controls.Add(ProcessBar);
            Controls.Add(ProcessLabel);
            Controls.Add(TimeSpent);
        }

        public void Swap(TrackingProcess process)
        {
            var tempName = ProcessName;
            ProcessName = process.ProcessName;
            process.ProcessName = tempName;
            //Swap process name
            var prevText = ProcessLabel.Text;
            ProcessLabel.Text = process.ProcessLabel.Text;
            process.ProcessLabel.Text = prevText;

            //Swap process bar progress
            var prevBarValue = ProcessBar.Value;
            ProcessBar.Value = process.ProcessBar.Value;
            process.ProcessBar.Value = prevBarValue;

            //Swap StopWatch & Timespent label
            StopStopwatch();
            var oldStopWatch = Stopwatch;
            var oldTimeLabelText = TimeSpent.Text;
            

            Stopwatch = process.Stopwatch;
            TimeSpent.Text = process.TimeSpent.Text;

            // NOTE:
            // after switch, process will not track time correctly MAKE SURE TO FIX
            process.Stopwatch = oldStopWatch;
            process.TimeSpent.Text = oldTimeLabelText;
        }

        public Point DestroyProcessTracker()
        {
            TimeSpent.Dispose();
            ProcessLabel.Dispose();
            ProcessBar.Dispose();

            return Location;
        }
    }
}
