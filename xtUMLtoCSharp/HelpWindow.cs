using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;

namespace UMLtoSourceCode
{
    public partial class HelpWindow : Form
    {
        public HelpWindow()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void HelpWindow_Load(object sender, EventArgs e)
        {
            richTextBox1.AppendText("Press 'Select File' button to select .json file to convert \n" +
                "\n" +
                "Check 'Multiple Json' checkbox to open folder. this will make the 'Select File' button changes to 'Open Folder' button to read all .json files contained in that folder \n" +
                "\n" +
                "Press 'Convert to source code' button to convert your selected json file(s) into c# source code \n" +
                "\n" +
                "Output will be displayed on 'Output' tab \n" +
                "\n" +
                "Press 'Download file' button to save the output into a file \n" +
                "\n" +
                "Press 'Reset' button to reset input, output, and selected file(s)" +
                "\n" +
                "\n");
        }
    }
}
