//Author: Richard Bunt
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ExampleReceiver
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                string address = string.Format(
                "http://127.0.0.1:8080/telemachus/datalink?alt={0}",
                Uri.EscapeDataString("v.altitude"));
                string text;
                using (WebClient client = new WebClient())
                {
                    text = client.DownloadString(address);
                    Console.WriteLine(text);
                }
            }
            catch (Exception d)
            {
                Console.WriteLine(d.Message);
            }
        }


    }
}
