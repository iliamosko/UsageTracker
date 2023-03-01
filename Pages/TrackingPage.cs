using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using UsageTracker.Background;
using Timer = System.Windows.Forms.Timer;
using UsageTracker.Entities;

namespace UsageTracker
{
    public partial class TrackingPage : Form
    {
        public static TimeSpan TimeDifference;
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;
        readonly DateTime start;
        readonly ActiveProcessListener activeWindowTracker;
        static int testNum = 1;

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn
        (
        int nLeftRect,
        int nTopRect,
        int nRightRect,
        int nBottomRect,
        int nWidthEllipse,
        int nHeightEllipse
        );

        public TrackingPage()
        {
            InitializeComponent();
            start = DateTime.UtcNow;
            IntialSetup();
            activeWindowTracker = new ActiveProcessListener(ProcessNamePanel, ProgressBarPanel, TimeSpentPanel,
                                                            ActiveTitleNamePanel, ActiveProgressBarPanel, ActiveTimePanel);
            StartTracking();
        }

        void IntialSetup()
        {
            Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 25, 25));
            pnlNav.Height = MenuButton1.Height;
            pnlNav.Top = MenuButton1.Top;
            pnlNav.Left = MenuButton1.Left;
            MenuButton1.BackColor = Color.FromArgb(46, 51, 73);
            exitButton.BackgroundImageLayout = ImageLayout.Stretch;
        }

        /// <summary>
        /// Creates a timer that will update every second.
        /// This is the entry point for the tracking process
        /// </summary>
        private void StartTracking()
        {
            var timer = new Timer
            {
                Interval = 1000
            };
            timer.Tick += new EventHandler(UpdateTime);
            timer.Start();
        }

        private void UpdateTime(object sender, EventArgs e)
        {
            var currentTime = DateTime.UtcNow;
            TimeDifference = currentTime - start;
            var hours = TimeDifference.Hours;
            var minutes = TimeDifference.Minutes;
            var seconds = TimeDifference.Seconds;
            TotalTimeLabel.Text = $"{hours:00}:{minutes:00}:{seconds:00}";

            // Everytime an update happens, update the active process/window
            activeWindowTracker.TrackProcess();
            activeWindowTracker.processManager.PlaceInOrder(ProcessNamePanel, ProgressBarPanel, TimeSpentPanel);

        }

        private void template_Load(object sender, EventArgs e)
        {

        }

        private void MenuButton1_Click(object sender, EventArgs e)
        {
            pnlNav.Height = MenuButton1.Height;
            pnlNav.Top = MenuButton1.Top;
            pnlNav.Left = MenuButton1.Left;
            MenuButton1.BackColor = Color.FromArgb(46, 51, 73);
        }
        private void MenuButton2_Click(object sender, EventArgs e)
        {
            pnlNav.Height = MenuButton2.Height;
            pnlNav.Top = MenuButton2.Top;
            MenuButton2.BackColor = Color.FromArgb(46, 51, 73);
            activeWindowTracker.processManager.PlaceInOrder(ProcessNamePanel, ProgressBarPanel, TimeSpentPanel);
        }

        private void MenuButton3_Click(object sender, EventArgs e)
        {
            pnlNav.Height = MenuButton3.Height;
            pnlNav.Top = MenuButton3.Top;
            MenuButton3.BackColor = Color.FromArgb(46, 51, 73);
            AddTempProcess();
        }

        private void MenuButton1_Leave(object sender, EventArgs e)
        {
            MenuButton1.BackColor = Color.FromArgb(24, 30, 54);
        }

        private void MenuButton2_Leave(object sender, EventArgs e)
        {
            MenuButton2.BackColor = Color.FromArgb(24, 30, 54);
        }

        private void MenuButton3_Leave(object sender, EventArgs e)
        {
            MenuButton3.BackColor = Color.FromArgb(24, 30, 54);
        }

        private void TrackingPage_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void navPanel_MouseDown(object sender, MouseEventArgs e)
        {
            TrackingPage_MouseDown(sender, e);
        }

        private void LogoPanel_MouseDown(object sender, MouseEventArgs e)
        {
            TrackingPage_MouseDown(sender, e);
        }

        private void exitButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void MainPanel_MouseDown(object sender, MouseEventArgs e)
        {
            TrackingPage_MouseDown(sender, e);
        }

        private void TopPanel_MouseDown(object sender, MouseEventArgs e)
        {
            TrackingPage_MouseDown(sender, e);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
            if (this.WindowState == FormWindowState.Minimized)
            {
                Hide();
                notifyIcon1.Visible = true;
            }
        }

        private void TrackingPage_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                Hide();
                notifyIcon1.Visible = true;
                notifyIcon1.ShowBalloonTip(1000);
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
            this.WindowState = FormWindowState.Normal;
            notifyIcon1.Visible = false;
        }

        private void AddTempProcess()
        {
            var ran = new Random();
            var process = new ActiveProcess($"test {testNum}", ProcessNamePanel, ProgressBarPanel, TimeSpentPanel);
            process.SetProgressBarValue(ran.Next(100));
            testNum++;

            activeWindowTracker.processManager.AddProcessWithoutTracking(process, ProcessNamePanel, ProgressBarPanel, TimeSpentPanel);

            if(testNum == 4)
            {
                //activeWindowTracker.processManager.PlaceInOrder(process);
            }

            //// ----- Adds a panel inside the ProcessNamePanel ----- //
            //Panel processPanel = new Panel();
            //Label processLabel = new Label();
            //processPanel.Size = new Size(120, 30);
            //processLabel.Text = "Test Process";
            //processLabel.ForeColor = Color.White;
            //processLabel.Location = new Point(32, 10);
            //processPanel.Controls.Add(processLabel);
            //processPanel.Dock = DockStyle.Top;
            //ProcessNamePanel.Controls.Add(processPanel);
            //ProcessNamePanel.Controls.SetChildIndex(processPanel, 0);

            //// ----- Adds a ProgressBar panel inside the ProcessBarPanel ----- //
            //Panel progressBarPanel = new Panel();
            //ProgressBar progressBar = new ProgressBar();
            //progressBarPanel.Size = new Size(411, 30);
            //progressBarPanel.Controls.Add(progressBar);
            //progressBar.Location = new Point(6, 4);
            //progressBar.Size = new Size(399, 23);
            //progressBarPanel.Dock = DockStyle.Top;
            //ProgressBarPanel.Controls.Add(progressBarPanel);
            //ProgressBarPanel.Controls.SetChildIndex(progressBarPanel, 0);

            //// ----- Adds a TimeSpent panel inside the PPanel ----- //
            //Panel timeSpentPanel = new Panel();
            //Label timeLabel = new Label();
            //timeSpentPanel.Size = new Size(117, 30);
            //timeSpentPanel.Controls.Add(timeLabel);
            //timeLabel.Text = "HH:MM:SS";
            //timeLabel.ForeColor = Color.White;
            //timeLabel.Location = new Point(32, 10);
            //timeSpentPanel.Dock = DockStyle.Top;
            //TimeSpentPanel.Controls.Add(timeSpentPanel);
            //TimeSpentPanel.Controls.SetChildIndex(timeSpentPanel, 0);


        }
    }
}
