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
                label1.Text = "";
            }
        }
        
        private void btnConvert_Click(object sender, EventArgs e)
        {
            label1.Text = "";
            richTextBox2.Clear();
            tabControl1.SelectTab(tabPage2);
            if (umlDiagramJson == null)
            {
                label1.Text = "No file supplied!";
            }
            else
            {
                JsonData json = JsonConvert.DeserializeObject<JsonData>(umlDiagramJson);
                StringBuilder SourceCodeBuilder = new StringBuilder();
                StringBuilder AssocBuilder = new StringBuilder();

                foreach (JsonData.Model model in json.model)
                {
                    if (model.model != null)
                    {
                        foreach (JsonData.Class1 class1 in model.@class)
                        {
                            AssocBuilder.AppendLine($"      " +
                            $"{class1.class_name} public List<{model.model.class_name}> {model.model.class_name}List  " + "{ get; set; }");
                        }
                    }
                }

                SourceCodeBuilder.AppendLine($"namespace {json.sub_name}");
                SourceCodeBuilder.AppendLine("{");

                // Classes START
                foreach (JsonData.Model model in json.model)
                {
                    if (model.type == "class")
                    {
                        var attrInfoList = new List<string>();
                        var states = new List<string>();

                        SourceCodeBuilder.AppendLine($"   public class {model.class_name}");
                        SourceCodeBuilder.AppendLine("   {");

                        if (model.states != null)
                        {
                            foreach (JsonData.State state in model.states)
                            {
                                string stateAdd = state.state_name;
                                states.Add(stateAdd);
                            }
                            SourceCodeBuilder.AppendLine("      " +
                                $"public enum {model.class_name}States" + "{" + string.Join(", ", states) + "}");
                            SourceCodeBuilder.AppendLine("");
                        }

                        foreach (JsonData.Attribute1 attr in model.attributes)
                        {
                            string dataType = attr.data_type;
                            if ((dataType == "id") || (dataType == "integer"))
                            {
                                dataType = "int";
                            }
                            else if (dataType == "real")
                            {
                                dataType = "double";
                            }

                            if (attr.data_type == "id" && attr.attribute_type != "referential_attribute")
                            {
                                SourceCodeBuilder.AppendLine("      " +
                                    $"private static int lastAssigned{attr.attribute_name} = 0;");
                            }

                            if (attr.default_value != null)
                            {
                                string input = attr.default_value;
                                int dot = input.IndexOf('.');
                                if (dot != -1)
                                {
                                    string state = input.Substring(dot + 1);
                                    SourceCodeBuilder.AppendLine("      " +
                                        $"public {model.class_name}States {attr.attribute_name} " + "{ get; set; }" + $" = {model.class_name}States.{state}" + ";");
                                }
                            }
                            else if (attr.data_type == "id")
                            {
                                SourceCodeBuilder.AppendLine("      " +
                                    $"public {dataType} {attr.attribute_name} " + "{ get; private set; }");
                            }
                            else
                            {
                                SourceCodeBuilder.AppendLine("      " +
                                    $"public {dataType} {attr.attribute_name} " + "{ get; set; }");
                            }

                            if (attr.data_type != "state")
                            {
                                if (attr.data_type != "id" && attr.attribute_type != "referential_attribute")
                                {
                                    string attrInfo = $"{dataType} {attr.attribute_name}";
                                    attrInfoList.Add(attrInfo);
                                }
                            }
                        }
                        // Associations START
                        foreach (var assoc in AssocBuilder.ToString().Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            string assocTrim = assoc.Trim();
                            int stSpace = assocTrim.IndexOf(' ');

                            if (assocTrim.StartsWith($"{model.class_name} ", StringComparison.OrdinalIgnoreCase))
                            {
                                SourceCodeBuilder.AppendLine("      " +
                                    assocTrim.Substring(stSpace + 1));
                            }
                        }
                        // Associations END

                        SourceCodeBuilder.AppendLine("");

                        string constructor = string.Join(", ", attrInfoList);
                        SourceCodeBuilder.AppendLine("      " +
                            $"public {model.class_name} ({constructor})");
                        SourceCodeBuilder.AppendLine("       {");
                        foreach (JsonData.Attribute1 attr in model.attributes)
                        {
                            if (attr.data_type != "state")
                            {
                                if (attr.data_type == "id" && attr.attribute_type != "referential_attribute")
                                {
                                    SourceCodeBuilder.AppendLine("           " +
                                    $"{attr.attribute_name} = ++lastAssigned{attr.attribute_name};");
                                }
                                else
                                {
                                    SourceCodeBuilder.AppendLine("           " +
                                    $"this.{attr.attribute_name} = {attr.attribute_name};");
                                }
                            }
                        }
                        SourceCodeBuilder.AppendLine("       }");
                        SourceCodeBuilder.AppendLine("   }");
                        SourceCodeBuilder.AppendLine("");
                    }
                    // Classes END

                    // Associations1 START
                    if (model.type == "association" && model.model != null)
                    {
                        SourceCodeBuilder.AppendLine("   " +
                            $"public class {model.model.class_name}");
                        SourceCodeBuilder.AppendLine("   {");

                        foreach (JsonData.Attribute assocAttr in model.model.attributes)
                        {
                            string dataType = assocAttr.data_type;
                            if ((dataType == "id") || (dataType == "integer"))
                            {
                                dataType = "int";
                            }
                            else if (dataType == "real")
                            {
                                dataType = "double";
                            }
                            if (assocAttr.attribute_type == "referential_attribute")
                            {
                                SourceCodeBuilder.AppendLine("      " +
                                    $"public {dataType} {assocAttr.attribute_name} " + "{ get; set; } // Referential Attribute");
                            }
                            else
                            {
                                SourceCodeBuilder.AppendLine("      " +
                                    $"public {dataType} {assocAttr.attribute_name} " + "{ get; set; }");
                            }
                        }
                        SourceCodeBuilder.AppendLine("");
                        foreach (JsonData.Class1 asoc_class in model.@class)
                        {
                            if (asoc_class.class_multiplicity == "1..1")
                            {
                                SourceCodeBuilder.AppendLine("      " +
                                    $"public {asoc_class.class_name} {asoc_class.class_name} " + "{ get; set; }");
                            }
                            else
                            {
                                SourceCodeBuilder.AppendLine("      " +
                                    $"public List<{asoc_class.class_name}> {asoc_class.class_name}List " + "{ get; set; }");
                            }
                        }
                        SourceCodeBuilder.AppendLine("   }");
                        SourceCodeBuilder.AppendLine("");
                    }
                }
                // Associations1 END

                SourceCodeBuilder.AppendLine("  class Program");
                SourceCodeBuilder.AppendLine("  {");
                SourceCodeBuilder.AppendLine("      static void Main(string[] args)");
                SourceCodeBuilder.AppendLine("      {");
                SourceCodeBuilder.AppendLine("          // Write your code here");
                SourceCodeBuilder.AppendLine("      }");
                SourceCodeBuilder.AppendLine("  }");

                SourceCodeBuilder.AppendLine("}");

                string SourceCode = SourceCodeBuilder.ToString();
                richTextBox2.AppendText(SourceCode);
            }
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
            richTextBox2.Clear();
            label1.Text = "";
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();

            dialog.Title = "Save into C# Source code";
            dialog.DefaultExt = "cs";
            dialog.Filter = "C# Source code (*.cs)|*.cs|C# Source code (*.*)|*.*";

            if (richTextBox2.Text != "")
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    string fileName = dialog.FileName;

                    File.WriteAllText(fileName, richTextBox2.Text);
                }
            }
            else
            {
                label1.Text = "There's no code generated!";
            }
        }
    }
}
