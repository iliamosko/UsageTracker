using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UsageTracker.Pages
{
    public partial class LogInPage : Form
    {
        public LogInPage()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SHA256 hash = SHA256.Create();

            var passwordBytes = Encoding.Default.GetBytes(PasswordTextBox.Text);

            var hashBytes = hash.ComputeHash(passwordBytes);
            var result = Convert.ToBase64String(hashBytes);

            Console.WriteLine(result);

        }
    }
}
