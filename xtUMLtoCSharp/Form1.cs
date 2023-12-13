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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace UMLtoSourceCode
{
    public partial class Form1 : Form
    {
        private HelpWindow helpWindowInstance;
        public string singleJson;
        public string[] multiJsonFiles;
        public string dataType;
        public bool multiJson = false;

        StringBuilder SourceCodeBuilder = new StringBuilder();
        StringBuilder AssocBuilder = new StringBuilder();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        public void reset()
        {
            richTextBox1.Clear();
            richTextBox2.Clear();
            textBox1.Clear();
            label1.Text = "";
            singleJson = null;
            multiJsonFiles = null;
            saveButton.Enabled = false;
            btnConvert.Enabled = false;
        }

        public void converter(string umlDiagramJson)
        {
            JsonData json = JsonConvert.DeserializeObject<JsonData>(umlDiagramJson);

            SourceCodeBuilder.Clear();
            AssocBuilder.Clear();

            // Assoc Builder START
            StringBuilder aB = new StringBuilder();
            foreach (JsonData.Model model in json.model.Where(model => model.type == "association"))
            {
                if (model.model != null)
                {
                    foreach (JsonData.Class1 class1 in model.@class)
                    {
                        AssocBuilder.AppendLine($"      " +
                        $"{class1.class_name} public List<{model.model.class_name}> {model.model.class_name}List  " + "{ get; set; }");
                    }
                }
                if (model.model == null)
                {
                    foreach (JsonData.Class1 class1 in model.@class)
                    {
                        aB.Append($"{class1.class_name} {class1.class_multiplicity}" + " ");
                    }
                    string aB_ = aB.ToString();
                    string[] words = aB_.Split();

                    if (words.Length >= 4)
                    {
                        string fstC = words[0];
                        string fstM = words[1];
                        string sndC = words[2];
                        string sndM = words[3];
                        string assoc = null;
                        string assoc1 = null;
                        if (fstM == "1..*" || fstM == "0..*")
                        {
                            assoc = $"{fstC} public List<{sndC}> {sndC}List " + "{ get; set; }";
                        } 
                        else if (fstM == "1..1")
                        {
                            assoc = $"{fstC} public {sndC} {sndC} " + "{ get; set; }";
                        }

                        if (sndM == "1..*" || sndM == "0..*")
                        {
                            assoc1 = $"{sndC} public List<{fstC}> {fstC} " + "{ get; set; }";
                        }
                        else if (sndM == "1..1")
                        {
                            assoc1 = $"{sndC} public {fstC} {fstC} " + "{ get; set; }";
                        }

                        AssocBuilder.AppendLine(assoc);
                        AssocBuilder.AppendLine(assoc1);
                    }
                    aB.Clear();
                }
            }
            // Assoc Builder END
            SourceCodeBuilder.AppendLine("");
            SourceCodeBuilder.AppendLine($"// {json.sub_name}");
            SourceCodeBuilder.AppendLine($"namespace {json.sub_name}");
            SourceCodeBuilder.AppendLine("{");

            // Classes START
            foreach (JsonData.Model model in json.model)
            {
                if (model.type == "class" || model.type == "imported_class")
                {
                    var attrInfoList = new List<string>();

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
                            if (attr.attribute_type == "referential_attribute")
                            {
                                SourceCodeBuilder.AppendLine("      " +
                                    $"public {dataType} {attr.attribute_name} " + "{ get; set; } // Referential Attribute");
                            }
                            else
                            {
                                SourceCodeBuilder.AppendLine("      " +
                                $"public {dataType} {attr.attribute_name} " + "{ get; private set; }");
                            }
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

                    // STATES START
                    var states = new List<string>();
                    if (model.states != null)
                    {
                        SourceCodeBuilder.AppendLine("");
                        foreach (JsonData.State state in model.states)
                        {
                            string stateAdd = state.state_name;
                            states.Add(stateAdd);
                        }
                        SourceCodeBuilder.AppendLine("      " +
                            $"public enum {model.class_name}States" + "{" + string.Join(", ", states) + "}");
                    }
                    // STATES END

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
            string aBB = aB.ToString();
            richTextBox2.AppendText(aBB);
        }

        public void singleJsonConvert(string inputFile)
        {
            if (inputFile == null)
            {
                label1.Text = "No Json File selected!!";
                return;
            }

            textBox1.Clear();
            textBox1.Text = inputFile;

            string umlDiagramJson = File.ReadAllText(inputFile);

            label1.Text = "";
            richTextBox2.Clear();
            tabControl1.SelectTab(tabPage2);

            converter(umlDiagramJson);
        }

        public void multiJsonConvert(IEnumerable<string> inputFolder)
        {
            if (inputFolder == null)
            {
                label1.Text = "No Folder selected!!";
                return;
            }

            label1.Text = "";
            richTextBox2.Clear();
            tabControl1.SelectTab(tabPage2);
            textBox1.Clear();

            foreach (var jsonFile in inputFolder)
            {
                textBox1.AppendText($"{jsonFile}, ");
                string umlDiagramJson = File.ReadAllText(jsonFile);
                converter(umlDiagramJson);
            }

        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            if (multiJson == true)
            {
                CommonOpenFileDialog dialog = new CommonOpenFileDialog();
                dialog.Title = "Open folder containing Json Diagram files";
                dialog.IsFolderPicker = true;
                StringBuilder JsonFilesContent = new StringBuilder();
                JsonFilesContent.Clear();

                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    string folderPath = dialog.FileName;
                    multiJsonFiles = Directory.GetFiles(folderPath, "*.json");
                    foreach (var jsonFile in multiJsonFiles)
                    {
                        string umlDiagramJson = File.ReadAllText(jsonFile);
                        JsonFilesContent.AppendLine("");
                        JsonFilesContent.AppendLine(umlDiagramJson);
                    }
                    richTextBox1.Text = JsonFilesContent.ToString();
                    btnConvert.Enabled = true;
                }
            }
            else
            {
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Title = "Open Json Diagram File";
                dialog.Filter = "Json Diagram Files|*.json";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    singleJson = dialog.FileName;
                    string displayJson = File.ReadAllText(singleJson);
                    tabControl1.SelectTab(tabPage1);
                    richTextBox1.Text = displayJson;
                    label1.Text = "";
                    btnConvert.Enabled = true;
                }
            }
        }
        
        private void btnConvert_Click(object sender, EventArgs e)
        {
            if (multiJson == true)
            {
                multiJsonConvert(multiJsonFiles);
            }
            else
            {
                singleJsonConvert(singleJson);
            }
            saveButton.Enabled = true;
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            reset();
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();

            dialog.Title = "Save into C# Source code";
            dialog.DefaultExt = "cs";
            dialog.Filter = "C# Source code (*.cs)|*.cs|C# Source code (*.*)|*.*";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string fileName = dialog.FileName;

                File.WriteAllText(fileName, richTextBox2.Text);
            }
        }

        private void multiJsonSwitch_CheckedChanged(object sender, EventArgs e)
        {
            multiJson = multiJsonSwitch.Checked;

            if (multiJson == true)
            {
                btnBrowse.Text = "Open Folder";
                reset();
            }
            else
            {
                btnBrowse.Text = "Select File";
                reset();
            }
        }

        private void HelpBtn_Click(object sender, EventArgs e)
        {
            if (helpWindowInstance == null || helpWindowInstance.IsDisposed)
            {
                helpWindowInstance = new HelpWindow();
                helpWindowInstance.Show();
            }
            else
            {
                helpWindowInstance.BringToFront();
            }
        }
    }
}
