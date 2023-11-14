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
using Newtonsoft.Json;
using UMLtoSourceCode.Class;

namespace UMLtoSourceCode
{
    public partial class Form1 : Form
    {
        public string umlDiagramJson;

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
            dialog.Title = "Open Json Diagram File";
            dialog.Filter = "Json Diagram Files|*.json";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                umlDiagramJson = File.ReadAllText(dialog.FileName);
                richTextBox1.Text = umlDiagramJson;
            }
        }
        
        private void btnConvert_Click(object sender, EventArgs e)
        {
            string umlDiagram = umlDiagramJson;
            JsonData json = JsonConvert.DeserializeObject<JsonData>(umlDiagram);
            richTextBox2.Clear();
            foreach (JsonData.Model model in json.model)
            {
                if (model.type == "class" )
                {
                    richTextBox2.AppendText("class " + model.class_name + "\n");
                    richTextBox2.AppendText("{\n");
                    foreach (JsonData.Attribute1 attribute in model.attributes)
                    {
                        richTextBox2.AppendText("   public " + attribute.data_type + " " + attribute.attribute_name + ";\n");
                    }
                    richTextBox2.AppendText("}\n");
                    richTextBox2.AppendText("\n");
                }
                if (model.type == "association")
                {
                    richTextBox2.AppendText("association : " + model.name + "\n");
                    foreach (JsonData.Attribute attribute_asoc in model.model.attributes)
                    {
                        richTextBox2.AppendText("association " + model.name + " : " + attribute_asoc.data_type + " " + attribute_asoc.attribute_name + ";\n");
                    }
                    richTextBox2.AppendText("\n");
                }
            }
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
            richTextBox2.Clear();
        }
    }
}
