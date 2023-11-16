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
using static System.Windows.Forms.AxHost;
using static UMLtoSourceCode.Class.JsonData;

namespace UMLtoSourceCode
{
    public partial class Form1 : Form
    {
        public string umlDiagramJson;
        public string dataType;

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
                tabControl1.SelectTab(tabPage2);
                richTextBox1.Text = umlDiagramJson;
            }
        }
        
        private void btnConvert_Click(object sender, EventArgs e)
        {
            string umlDiagram = umlDiagramJson;
            richTextBox2.Clear();
            tabControl1.SelectTab(tabPage2);
            if (umlDiagram == null)
            {
                richTextBox2.Clear();
                richTextBox2.Text = "No file supplied!";
            }
            else
            {
                JsonData json = JsonConvert.DeserializeObject<JsonData>(umlDiagram);
                richTextBox2.AppendText($"namespace {json.sub_name}\n");
                richTextBox2.AppendText("{\n");
                foreach (JsonData.Model model in json.model)
                {
                    if (model.type == "class")
                    {
                        var attrInfoList = new List<string>();
                        richTextBox2.AppendText($"   public class {model.class_name}\n");
                        richTextBox2.AppendText("   {\n");
                        foreach (JsonData.Attribute1 attr in model.attributes)
                        {
                            dataType = attr.data_type;
                            if ((dataType == "id") || (dataType == "integer"))
                            {
                                dataType = "int";
                            } 
                            else if (dataType == "real")
                            {
                                dataType = "float";
                            }
                            else if (dataType == "state")
                            {
                                dataType = "string";
                            }
                            if (attr.default_value != null)
                            {
                                richTextBox2.AppendText($"      public {dataType} {attr.attribute_name} " + "{ get; set; }"+ " = " + '"' + attr.default_value + '"' + ";\n");
                            } else
                            {
                                richTextBox2.AppendText($"      public {dataType} {attr.attribute_name} " + "{ get; set; }\n");
                            }
                            string attrInfo = $"{dataType} {attr.attribute_name}";
                            attrInfoList.Add(attrInfo);
                        }
                        richTextBox2.AppendText("\n");

                        string constructor = string.Join(", ", attrInfoList);
                        richTextBox2.AppendText($"      public {model.class_name} ({constructor})\n");
                        richTextBox2.AppendText("       {\n");
                        foreach (JsonData.Attribute1 attr in model.attributes)
                        {
                            richTextBox2.AppendText($"           this.{attr.attribute_name} = {attr.attribute_name};\n");
                        }
                        richTextBox2.AppendText("       }\n");
                        richTextBox2.AppendText("   }\n");
                        richTextBox2.AppendText("\n");
                    }
                    if (model.type == "association")
                    {
                        richTextBox2.AppendText($"   association : {model.name}\n");
                        foreach (JsonData.Attribute attr_asoc in model.model.attributes)
                        {
                            richTextBox2.AppendText($"   association {model.name} : {attr_asoc.data_type} {attr_asoc.attribute_name};\n");
                        }
                        richTextBox2.AppendText("\n");
                    }
                }
                richTextBox2.AppendText("}\n");
            }
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
            richTextBox2.Clear();
        }
    }
}
