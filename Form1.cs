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
using Newtonsoft.Json.Linq;

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
                tabControl1.SelectTab(tabPage1);
                richTextBox1.Text = umlDiagramJson;
            }
        }
        
        private void btnConvert_Click(object sender, EventArgs e)
        {
            richTextBox2.Clear();
            tabControl1.SelectTab(tabPage2);
            if (umlDiagramJson == null)
            {
                richTextBox2.Clear();
                richTextBox2.Text = "No file supplied!";
            }
            else
            {
                JsonData json = JsonConvert.DeserializeObject<JsonData>(umlDiagramJson);
                StringBuilder SourceCodeBuilder = new StringBuilder();

                SourceCodeBuilder.AppendLine($"namespace {json.sub_name}");
                SourceCodeBuilder.AppendLine("{");

                foreach (JsonData.Model model in json.model)
                {
                    // Classes START
                    if (model.type == "class")
                    {
                        var attrInfoList = new List<string>();
                        var states = new List<string>();

                        if (model.states != null)
                        {
                            foreach (JsonData.State state in model.states)
                            {
                                string stateAdd = $"        {state.state_name}";
                                states.Add(stateAdd);
                            }
                            SourceCodeBuilder.AppendLine($"   public enum {model.class_name}States");
                            SourceCodeBuilder.AppendLine("   {");
                            SourceCodeBuilder.AppendLine(string.Join(",\n", states));
                            SourceCodeBuilder.AppendLine("   }");
                            SourceCodeBuilder.AppendLine("");
                        }

                        SourceCodeBuilder.AppendLine($"   public class {model.class_name}");
                        SourceCodeBuilder.AppendLine("   {");
                        foreach (JsonData.Attribute1 attr in model.attributes)
                        {
                            string dataType = attr.data_type;
                            if ((dataType == "id") || (dataType == "integer"))
                            {
                                dataType = "int";
                            }
                            else if (dataType == "real")
                            {
                                dataType = "float";
                            }

                            if (attr.default_value != null)
                            {
                                string input = attr.default_value;
                                int dot = input.IndexOf('.');
                                if (dot != -1)
                                {
                                    string state = input.Substring(dot + 1);
                                    SourceCodeBuilder.AppendLine($"      " +
                                        $"public {model.class_name}States {attr.attribute_name} " + "{ get; set; }" + $" = {model.class_name}States.{state}" + ";");
                                }
                            }
                            else
                            {
                                SourceCodeBuilder.AppendLine($"      " +
                                    $"public {dataType} {attr.attribute_name} " + "{ get; set; }");
                            }

                            if (attr.data_type != "state")
                            {
                                string attrInfo = $"{dataType} {attr.attribute_name}";
                                attrInfoList.Add(attrInfo);
                            }
                        }
                        SourceCodeBuilder.AppendLine("");

                        string constructor = string.Join(", ", attrInfoList);
                        SourceCodeBuilder.AppendLine($"      " +
                            $"public {model.class_name} ({constructor})");
                        SourceCodeBuilder.AppendLine("       {");
                        foreach (JsonData.Attribute1 attr in model.attributes)
                        {
                            if (attr.data_type != "state")
                            {
                                SourceCodeBuilder.AppendLine($"           " +
                                    $"this.{attr.attribute_name} = {attr.attribute_name};");
                            }
                        }
                        SourceCodeBuilder.AppendLine("       }");
                        SourceCodeBuilder.AppendLine("   }");
                        SourceCodeBuilder.AppendLine("");
                    }
                    // Classes END

                    // Associations START
                    if (model.type == "association" && model.model != null)
                    {
                        SourceCodeBuilder.AppendLine($"   " +
                            $"public class {model.model.class_name}");
                        SourceCodeBuilder.AppendLine("   {");

                        var assocAttrname = new List<string>();
                        foreach (JsonData.Attribute assocAttr in model.model.attributes)
                        {
                            string dataType = assocAttr.data_type;
                            if ((dataType == "id") || (dataType == "integer"))
                            {
                                dataType = "int";
                            }
                            else if (dataType == "real")
                            {
                                dataType = "float";
                            }
                            SourceCodeBuilder.AppendLine($"      " +
                                $"public {dataType} {assocAttr.attribute_name} " + "{ get; set; }");

                            string attrInfo = $"{dataType} {assocAttr.attribute_name}";
                            assocAttrname.Add(attrInfo);
                        }
                        SourceCodeBuilder.AppendLine("");

                        string assocAttrconstructor = string.Join(", ", assocAttrname);
                        SourceCodeBuilder.AppendLine($"      " +
                            $"public {model.model.class_name} ({assocAttrconstructor})");
                        SourceCodeBuilder.AppendLine("       {");
                        foreach (JsonData.Attribute assocAttr in model.model.attributes)
                        {
                            SourceCodeBuilder.AppendLine($"           " +
                                $"this.{assocAttr.attribute_name} = {assocAttr.attribute_name};");
                        }
                        SourceCodeBuilder.AppendLine("       }");

                        SourceCodeBuilder.AppendLine("");
                        SourceCodeBuilder.AppendLine("   }");
                        SourceCodeBuilder.AppendLine("");
                    }

                    if (model.name != null)
                    {
                        SourceCodeBuilder.AppendLine($"   " +
                            $"public class {model.name}");
                        SourceCodeBuilder.AppendLine("   {");
                        foreach (JsonData.Class1 asoc_class in model.@class)
                        {
                            if (asoc_class.class_multiplicity == "1..1")
                            {
                                SourceCodeBuilder.AppendLine($"      " +
                                    $"public {asoc_class.class_name} {asoc_class.class_name} " + "{ get; set; }");
                            }
                            else
                            {
                                SourceCodeBuilder.AppendLine($"      " +
                                    $"public List<{asoc_class.class_name}> {asoc_class.class_name}s " + "{ get; set; }");
                            }
                        }
                        SourceCodeBuilder.AppendLine("   }");
                        SourceCodeBuilder.AppendLine("");
                    }
                    // Associations END
                }
                SourceCodeBuilder.AppendLine("}");

                string SourceCode = SourceCodeBuilder.ToString();
                richTextBox2.AppendText(SourceCode);
            }
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
            richTextBox2.Clear();
        }
    }
}
