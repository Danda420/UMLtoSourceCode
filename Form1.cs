using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace UMLtoSourceCode
{
    public partial class Form1 : Form
    {
        public string umlDiagramTxt;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "Open Text File";
            dialog.Filter = "TXT files|*.txt";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                umlDiagramTxt = File.ReadAllText(dialog.FileName);
                richTextBox1.Text = umlDiagramTxt;
            }
        }

        private void btnConvert_Click(object sender, EventArgs e)
        {
            string umlDiagram = umlDiagramTxt;
            richTextBox2.Text = umlDiagram;

        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
            richTextBox2.Clear();
        }
    }
}
