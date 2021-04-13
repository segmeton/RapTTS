using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RapTTS
{
    public partial class Form1 : Form
    {
        private RapGenerator rapGenerator;

        public Form1()
        {
            InitializeComponent();
            rapGenerator = new RapGenerator();
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            Console.WriteLine("start");
            rapGenerator.StartTTS(true);
            
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            Console.WriteLine("stop");
            rapGenerator.Stop();
        }
    }
}
