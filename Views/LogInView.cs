using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TimeTracker.Entities;
using UsageTracker.BackEnd;

namespace UsageTracker.Pages
{
    public partial class LogInView : Form
    {
        public LogInView()
        {
            SqlDatabase.CreateDatasource("Host=localhost;Username=postgres;Password=passwordpassword;Database=TimeTracker");
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SHA256 hash = SHA256.Create();

            var passwordBytes = Encoding.Default.GetBytes(PasswordTextBox.Text);

            var hashBytes = hash.ComputeHash(passwordBytes);
            var result = Convert.ToBase64String(hashBytes);

            CheckCredentials(textBox1.Text, PasswordTextBox.Text);

        }

        private void CheckCredentials(string email, string password)
        {
            var users = SqlDatabase.ExecuteCommand("select * from accounts");
            User? activeUser = null;

            foreach(var user in users)
            {
                if (activeUser is null && Authenticate(user, email, password))
                {
                    activeUser = user;
                    break;
                }
            }

            if (activeUser is not null)
            {
                OpenTrackingPage(activeUser);
            
            }
            else
            {
                // show alert box of invalid user
            }
        }

        private bool Authenticate(User user, string email, string password)
        {
            if (user.GetEmail().Equals(email))
            {
                if (user.GetPassword().Equals(password))
                {
                    return true;
                }
            }
            return false;
        }

        private void OpenTrackingPage(User activeUser)
        {
            Hide();
            var trackingPage = new TrackingView(activeUser);
            trackingPage.FormClosed += (s, e) => Close();
            trackingPage.Show();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Hide();
            var trackingPage = new TrackingView(new User(0, "Guest","","",""));
            trackingPage.FormClosed += (s, e) => Close();
            trackingPage.Show();
        }
    }
}
