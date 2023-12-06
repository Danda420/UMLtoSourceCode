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
            richTextBox1.SelectionFont = new Font(richTextBox1.Font, FontStyle.Bold);
            richTextBox1.AppendText("EN : \n");
            richTextBox1.SelectionFont = new Font(richTextBox1.Font, FontStyle.Regular);
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
                "\n" +
                "============================================\n" +
                "\n");
            richTextBox1.SelectionFont = new Font(richTextBox1.Font, FontStyle.Bold);
            richTextBox1.AppendText("ID : \n");
            richTextBox1.SelectionFont = new Font(richTextBox1.Font, FontStyle.Regular);
            richTextBox1.AppendText("Tekan Tombol 'Select File' untuk memilih file .json yang akan di convert \n" +
                "\n" +
                "Centang checkbox 'Multiple Json' untuk membuka folder. ini akan membuat tombol 'Select File' berganti ke tombol 'Open Folder' untuk memilih semua file .json yang ada di folder tersebut \n" +
                "\n" +
                "Tekan tombol 'Convert to source code' untuk menconvert file yang dipilih ke c# source code \n" +
                "\n" +
                "Output nya akan di displaykan pada tab 'Output' \n" +
                "\n" +
                "Tekan tombol 'Download file' untuk menyimpan outputnya ke sebuah file \n" +
                "\n" +
                "Tekan tombol 'Reset' untuk mereset input, output, dan file yang dipilih" +
                "\n" +
                "\n");
        }
    }
}
