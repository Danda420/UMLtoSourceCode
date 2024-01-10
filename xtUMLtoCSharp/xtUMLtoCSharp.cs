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
using System.Collections;
using System.Net.NetworkInformation;
using System.Windows.Controls;

namespace UMLtoSourceCode
{
    public partial class xtUMLtoCSharp : Form
    {
        private string[] JSONfiles;
        public string dataType;

        StringBuilder parsingResult = new StringBuilder();

        StringBuilder SourceCodeBuilder = new StringBuilder();
        StringBuilder AssocBuilder = new StringBuilder();

        public xtUMLtoCSharp()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        public StringBuilder GetMessageBox()
        {
            return parsingResult;
        }

        public void reset()
        {
            textBox1.Clear();
            textBox2.Clear();
            textBox3.Clear();
            JSONfiles = null;
        }
        

        public void converterJSONtoCSharp(string umlDiagramJson)
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

            // STATES START
            foreach (JsonData.Model model in json.model)
            {
                var states = new List<string>();
                if (model.states != null)
                {
                    foreach (JsonData.State state in model.states)
                    {
                        string stateAdd = state.state_name.Replace(" ", "");
                        states.Add(stateAdd);
                    }
                    SourceCodeBuilder.AppendLine("   " +
                        $"public enum {model.class_name}States" + "\n   {");
                    foreach (var state in states)
                    {
                        SourceCodeBuilder.AppendLine("      " +
                            $"{state},");
                    }
                    SourceCodeBuilder.AppendLine("   }");
                    SourceCodeBuilder.AppendLine("");
                }
            }
            // STATES END

            SourceCodeBuilder.AppendLine("   public class Timer");
            SourceCodeBuilder.AppendLine("   {");
            SourceCodeBuilder.AppendLine("   }");

            // Classes START
            foreach (JsonData.Model model in json.model)
            {
                SourceCodeBuilder.AppendLine("");
                if (model.type == "class" || model.type == "imported_class")
                {
                    var attrInfoList = new List<string>();

                    string stateAttribute = null;

                    SourceCodeBuilder.AppendLine($"   public class {model.class_name}");
                    SourceCodeBuilder.AppendLine("   {");

                    foreach (JsonData.Attribute1 attr in model.attributes)
                    {
                        string dataType = attr.data_type;
                        if (dataType == "id")
                        {
                            dataType = "char";
                        }
                        else if (dataType == "integer")
                        {
                            dataType = "int";
                        }
                        else if (dataType == "real")
                        {
                            dataType = "double";
                        }

                        if (attr.default_value != null)
                        {
                            stateAttribute = attr.attribute_name;
                            string input = attr.default_value;
                            int dot = input.IndexOf('.');
                            if (dot != -1)
                            {
                                string state = input.Substring(dot + 1);
                                SourceCodeBuilder.AppendLine("      " +
                                    $"public {model.class_name}States {attr.attribute_name} " + "{ get; set; }" + $" = {model.class_name}States.{state}" + ";");
                            }
                            else
                            {
                                {
                                    SourceCodeBuilder.AppendLine("      " +
                                        $"public {model.class_name}States {attr.attribute_name} " + "{ get; set; }" + $" = {model.class_name}States.{input}" + ";");
                                }
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
                                $"public {dataType} {attr.attribute_name} " + "{ get; set; }");
                            }
                        }
                        else if (attr.data_type == "inst_event")
                        {
                            SourceCodeBuilder.AppendLine("");
                            string cName = null;
                            foreach (JsonData.Model modell in json.model)
                            {
                                if (modell.class_id == attr.class_id)
                                {
                                    cName = modell.class_name;
                                }
                            }
                            SourceCodeBuilder.AppendLine("      " +
                                $"public void {attr.event_name}({cName} {cName})");
                            SourceCodeBuilder.AppendLine("      " +
                                "{");
                            SourceCodeBuilder.AppendLine("         " +
                                $"{cName}.status = {cName}States.{attr.state_name};");
                            SourceCodeBuilder.AppendLine("      " +
                                "}");
                            SourceCodeBuilder.AppendLine("");
                        }
                        else if (attr.data_type == "inst_ref")
                        {
                            SourceCodeBuilder.AppendLine("      " +
                                $"public {attr.related_class_name} {attr.attribute_name}Ref" + " { get; set; }");
                        }
                        else if (attr.data_type == "inst_ref_set")
                        {
                            SourceCodeBuilder.AppendLine("      " +
                                $"public {attr.related_class_name}[] {attr.attribute_name}RefSet" + " { get; set; }");
                        }
                        else if (attr.data_type == "inst_ref_<timer>")
                        {
                            SourceCodeBuilder.AppendLine("      " +
                                $"public {attr.related_class_name} {attr.attribute_name}" + " { get; set; }");
                        }
                        else
                        {
                            SourceCodeBuilder.AppendLine("      " +
                                $"public {dataType} {attr.attribute_name} " + "{ get; set; }");
                        }

                        if (attr.data_type != "state" && attr.data_type != "inst_event" && attr.data_type != "inst_ref" && attr.data_type != "inst_ref_set" && attr.data_type != "inst_ref_<timer>")
                        {
                            if (attr.attribute_type != "referential_attribute")
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

                    // STATES EVENT AND FUNCTIONS START
                    if (model.states != null)
                    {
                        SourceCodeBuilder.AppendLine("");
                        SourceCodeBuilder.AppendLine("      " +
                                        $"public void onStateAction()");
                        SourceCodeBuilder.AppendLine("      {");
                        SourceCodeBuilder.AppendLine("           " +
                            $"switch({stateAttribute})");
                        SourceCodeBuilder.AppendLine("           {");
                        foreach (JsonData.State statess in model.states)
                        {
                            SourceCodeBuilder.AppendLine("              " +
                                $"case {model.class_name}States.{statess.state_name.Replace(" ", "")}:");
                            SourceCodeBuilder.AppendLine("                  " +
                                "// implementations code here");
                            if (statess.transitions != null)
                            {
                                foreach (var transition in statess.transitions)
                                {
                                    string targetState = null;
                                    foreach (JsonData.State states in model.states)
                                    {
                                        if (states.state_id == transition.target_state_id)
                                        {
                                            targetState = states.state_event.ToString();
                                        }
                                    }
                                    SourceCodeBuilder.AppendLine("                  " +
                                        $"if ({stateAttribute} == {model.class_name}States.{transition.target_state.Replace(" ", "")})");
                                    SourceCodeBuilder.AppendLine("                  {");
                                    SourceCodeBuilder.AppendLine("                      " +
                                        $"{targetState}();");
                                    SourceCodeBuilder.AppendLine("                  }");
                                }
                            }
                            SourceCodeBuilder.AppendLine("                  " +
                                "break;");
                        }
                        SourceCodeBuilder.AppendLine("              " +
                                $"default:");
                        SourceCodeBuilder.AppendLine("                  " +
                                "break;");
                        SourceCodeBuilder.AppendLine("           }");
                        SourceCodeBuilder.AppendLine("      }");
                        foreach (JsonData.State state in model.states)
                        {
                            if (state.state_function != null)
                            {
                                foreach (var stateFunction in state.state_function)
                                {
                                    SourceCodeBuilder.AppendLine("");
                                    SourceCodeBuilder.AppendLine("      " +  
                                        $"public void {stateFunction}()");
                                    SourceCodeBuilder.AppendLine("      {");
                                    foreach (JsonData.Attribute1 attr in model.attributes)
                                    {
                                        if (attr.data_type == "state")
                                        {
                                            SourceCodeBuilder.AppendLine("           " +
                                                    $"if ({attr.attribute_name} != {model.class_name}States.{state.state_name.Replace(" ", "")})");
                                            SourceCodeBuilder.AppendLine("           {");
                                            SourceCodeBuilder.AppendLine("               " +
                                                $"{attr.attribute_name} = {model.class_name}States.{state.state_name.Replace(" ", "")};");
                                            SourceCodeBuilder.AppendLine("           }");
                                        }
                                    }
                                    SourceCodeBuilder.AppendLine("      }");
                                }
                            }

                            void stateEventBuilder(string stateEvent)
                            {
                                SourceCodeBuilder.AppendLine("");
                                SourceCodeBuilder.AppendLine("      " +
                                        $"public void {stateEvent}()");
                                SourceCodeBuilder.AppendLine("      {");

                                foreach (JsonData.Attribute1 attr in model.attributes)
                                {
                                    if (attr.data_type == "state")
                                    {
                                        SourceCodeBuilder.AppendLine("           " +
                                                $"if ({attr.attribute_name} != {model.class_name}States.{state.state_name.Replace(" ", "")})");
                                        SourceCodeBuilder.AppendLine("           {");
                                        SourceCodeBuilder.AppendLine("               " +
                                            $"{attr.attribute_name} = {model.class_name}States.{state.state_name.Replace(" ", "")};");
                                        SourceCodeBuilder.AppendLine("           }");

                                    }
                                }
                                SourceCodeBuilder.AppendLine("      }");
                            }

                            if (state.state_event != null)
                            {
                                var stateEventArray = state.state_event as JArray;
                                if (stateEventArray != null)
                                {
                                    foreach (var item in stateEventArray)
                                    {
                                        string stateEvent = item.ToString();
                                        if (!stateEvent.StartsWith("on", StringComparison.OrdinalIgnoreCase))
                                        {
                                            stateEventBuilder(stateEvent);
                                        }
                                    }
                                }
                                else if (state.state_event is string)
                                {
                                    string stateEvent = state.state_event.ToString();
                                    stateEventBuilder(stateEvent);
                                }
                            }
                        }
                    }
                    // STATES EVENT AND FUNCTIONS END

                    SourceCodeBuilder.AppendLine("");

                    string constructor = string.Join(", ", attrInfoList);
                    SourceCodeBuilder.AppendLine("      " +
                        $"public {model.class_name} ({constructor})");
                    SourceCodeBuilder.AppendLine("       {");
                    foreach (JsonData.Attribute1 attr in model.attributes)
                    {
                        if (attr.data_type != "state" 
                            && attr.data_type != "inst_event" 
                            && attr.data_type != "inst_ref" 
                            && attr.data_type != "inst_ref_set"
                            && attr.attribute_type != "referential_attribute")
                        {
                            if (attr.data_type == "inst_ref_<timer>")
                            {
                                SourceCodeBuilder.AppendLine("           " +
                                    $"{attr.attribute_name} = new {attr.related_class_name}();");
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

            SourceCodeBuilder.AppendLine("");
            SourceCodeBuilder.AppendLine("  class Program");
            SourceCodeBuilder.AppendLine("  {");
            SourceCodeBuilder.AppendLine("      static void Main(string[] args)");
            SourceCodeBuilder.AppendLine("      {");
            SourceCodeBuilder.AppendLine("          // Write your code here");
            SourceCodeBuilder.AppendLine("      }");
            SourceCodeBuilder.AppendLine("  }");

            SourceCodeBuilder.AppendLine("}");

            string SourceCode = SourceCodeBuilder.ToString();
            textBox3.AppendText(SourceCode);
        }

        public void JSONtoCSharp(string[] inputFolder)
        {
            if (inputFolder == null)
            {
                MessageBox.Show("Please select a folder containing JSON files first.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            label1.Text = "";
            textBox3.Clear();

            foreach (var JsonFile in JSONfiles)
            {
                string umlDiagramJson = File.ReadAllText(JsonFile);
                converterJSONtoCSharp(umlDiagramJson);
            }
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.Title = "Open folder containing Json Diagram files";
            dialog.IsFolderPicker = true;
            StringBuilder JsonFilesContent = new StringBuilder();
            JsonFilesContent.Clear();

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                string folderPath = dialog.FileName;
                textBox1.Text = folderPath;
                JSONfiles = Directory.GetFiles(folderPath, "*.json");
                foreach (var jsonFile in JSONfiles)
                {
                    string umlDiagramJson = File.ReadAllText(jsonFile);
                    JsonFilesContent.AppendLine("");
                    JsonFilesContent.AppendLine(umlDiagramJson);
                }
                textBox2.Text = JsonFilesContent.ToString();
                ProcessFilesInFolder(folderPath);
            }
        }
        
        private void btnTranslate_Click(object sender, EventArgs e)
        {
            JSONtoCSharp(JSONfiles);
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(textBox3.Text);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            reset();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();

            dialog.Title = "Save into C# Source code";
            dialog.DefaultExt = "cs";
            dialog.Filter = "C# Source code (*.cs)|*.cs|C# Source code (*.*)|*.*";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string fileName = dialog.FileName;

                File.WriteAllText(fileName, textBox3.Text);
            }
        }


        // PARSING


        private void ProcessFilesInFolder(string folderPath)
        {
            try
            {
                this.JSONfiles = Directory.GetFiles(folderPath, "*.json");

                if (this.JSONfiles.Length == 0)
                {
                    MessageBox.Show("No JSON files found in this folder.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);

                }
                this.ProcessJson(this.JSONfiles);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private JArray ProcessJson(string[] JSONfiles)
        {
            List<string> jsonArrayList = new List<string>();

            foreach (var fileName in JSONfiles)
            {
                try
                {
                    string jsonContent = File.ReadAllText(fileName);
                    jsonArrayList.Add(jsonContent);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error reading the file {Path.GetFileName(fileName)}: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                }
            }

            JArray jsonArray = new JArray(jsonArrayList.Select(JToken.Parse));

            return jsonArray;
        }

        private void CheckJsonComp1(object sender, EventArgs e)
        {
            try
            {
                if (JSONfiles == null || JSONfiles.Length == 0)
                {
                    MessageBox.Show("Please select a folder containing JSON files first.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    return;
                }

                foreach (var fileName in this.JSONfiles)
                {
                    string jsonContent = File.ReadAllText(fileName);
                    CheckJsonCompliance(jsonContent);
                }
            }
            catch (Exception ex)
            {
                HandleError($"Error: {ex.Message}");
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnParse_Click(object sender, EventArgs e)
        {
            if (JSONfiles == null || JSONfiles.Length == 0)
            {
                MessageBox.Show("Please select a folder containing JSON files first.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);

                return;
            }

            JArray jsonArray = this.ProcessJson(JSONfiles);

            parsingResult.Clear();

            CheckParsing15.Point1(this, jsonArray);
            CheckParsing15.Point2(this, jsonArray);
            CheckParsing15.Point3(this, jsonArray);
            CheckParsing15.Point4(this, jsonArray);
            CheckParsing15.Point5(this, jsonArray);
            CheckParsing610.Point6(this, jsonArray);
            CheckParsing610.Point7(this, jsonArray);
            CheckParsing610.Point8(this, jsonArray);
            CheckParsing610.Point9(this, jsonArray);
            CheckParsing610.Point10(this, jsonArray);
            CheckParsing1115.Point11(this, jsonArray);
            CheckParsing1115.Point13(this, jsonArray);
            CheckParsing1115.Point14(this, jsonArray);
            CheckParsing1115.Point15(this, jsonArray);

            CheckJsonComp1(sender, e);

            ParsingPoint.Point25(this, jsonArray);
            ParsingPoint.Point27(this, jsonArray);
            ParsingPoint.Point28(this, jsonArray);
            ParsingPoint.Point29(this, jsonArray);
            ParsingPoint.Point30(this, jsonArray);
            ParsingPoint.Point34(this, jsonArray);
            ParsingPoint.Point35(this, jsonArray);

            CheckParsing1115.Point99(this, jsonArray);

            if (string.IsNullOrEmpty(parsingResult.ToString()))
            {
                MessageBox.Show("Model has successfully passed parsing", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show(parsingResult.ToString(), "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void HandleError(string errorMessage)
        {
            parsingResult.AppendLine(errorMessage);
        }

        private void CheckJsonCompliance(string jsonContent)
        {
            try
            {
                JObject jsonObj = JObject.Parse(jsonContent);

                // Dictionary to store state model information
                Dictionary<string, string> stateModels = new Dictionary<string, string>();
                HashSet<string> usedKeyLetters = new HashSet<string>();
                HashSet<int> stateNumbers = new HashSet<int>();

                JToken subsystemsToken = jsonObj["subsystems"];
                if (subsystemsToken != null && subsystemsToken.Type == JTokenType.Array)
                {
                    // Iterasi untuk setiap subsystem dalam subsystemsToken
                    foreach (var subsystem in subsystemsToken)
                    {
                        JToken modelToken = subsystem["model"];
                        if (modelToken != null && modelToken.Type == JTokenType.Array)
                        {
                            foreach (var model in modelToken)
                            {
                                ValidateClassModel(model, stateModels, usedKeyLetters, stateNumbers);
                            }
                        }
                    }

                    // Setelah memvalidasi semua model, panggil ValidateEventDirectedToStateModelHelper untuk setiap subsystem
                    foreach (var subsystem in subsystemsToken)
                    {
                        ValidateEventDirectedToStateModelHelper(subsystem["model"], stateModels, null);
                    }
                }

                ValidateTimerModel(jsonObj, usedKeyLetters);
            }
            catch (Exception ex)
            {
                HandleError($"Error: {ex.Message}");
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ValidateClassModel(JToken model, Dictionary<string, string> stateModels, HashSet<string> usedKeyLetters, HashSet<int> stateNumbers)
        {
            string objectType = model["type"]?.ToString();
            string objectName = model["class_name"]?.ToString();
            Console.WriteLine($"Running CheckKeyLetterUniqueness for {objectName}");

            if (objectType == "class")
            {
                Console.WriteLine($"Checking class: {objectName}");

                string assignerStateModelName = $"{objectName}_ASSIGNER";
                JToken assignerStateModelToken = model[assignerStateModelName];

                if (assignerStateModelToken == null || assignerStateModelToken.Type != JTokenType.Object)
                {
                    HandleError($"Syntax error 16: Assigner state model not found for {objectName}.");
                    return;
                }

                string keyLetter = model["KL"]?.ToString();

                // Pemanggilan CheckKeyLetterUniqueness
                CheckKeyLetterUniqueness(usedKeyLetters, keyLetter, objectName);

                // Check if KeyLetter is correct
                JToken keyLetterToken = assignerStateModelToken?["KeyLetter"];
                if (keyLetterToken != null && keyLetterToken.ToString() != keyLetter)
                {
                    HandleError($"Syntax error 17: KeyLetter for {objectName} does not match the rules.");
                }

                // Check uniqueness of states
                CheckStateUniqueness(stateModels, assignerStateModelToken?["states"], objectName, assignerStateModelName);

                // Check uniqueness of state numbers
                CheckStateNumberUniqueness(stateNumbers, assignerStateModelToken?["states"], objectName);

                // Store state model information
                string stateModelKey = $"{objectName}.{assignerStateModelName}";
                stateModels[stateModelKey] = objectName;
            }
        }

        private void CheckStateUniqueness(Dictionary<string, string> stateModels, JToken statesToken, string objectName, string assignerStateModelName)
        {
            if (statesToken is JArray states)
            {
                HashSet<string> uniqueStates = new HashSet<string>();

                foreach (var state in states)
                {
                    string stateName = state["state_name"]?.ToString();
                    string stateModelName = $"{objectName}.{stateName}";

                    // Check uniqueness of state model
                    if (!uniqueStates.Add(stateModelName))
                    {
                        HandleError($"Syntax error 18: State {stateModelName} is not unique in {assignerStateModelName}.");
                    }
                }
            }
        }

        private void CheckStateNumberUniqueness(HashSet<int> stateNumbers, JToken statesToken, string objectName)
        {
            if (statesToken is JArray stateArray)
            {
                foreach (var state in stateArray)
                {
                    int stateNumber = state["state_number"]?.ToObject<int>() ?? 0;

                    if (!stateNumbers.Add(stateNumber))
                    {
                        HandleError($"Syntax error 19: State number {stateNumber} is not unique.");
                    }
                }
            }
        }

        private void CheckKeyLetterUniqueness(HashSet<string> usedKeyLetters, string keyLetter, string objectName)
        {
            string expectedKeyLetter = $"{keyLetter}_A";
            Console.WriteLine("Running ValidateClassModel");
            Console.WriteLine($"Checking KeyLetter uniqueness: {expectedKeyLetter} for {objectName}");

            if (!usedKeyLetters.Add(expectedKeyLetter))
            {
                HandleError($"Syntax error 20: KeyLetter for {objectName} is not unique.");
            }
        }

        private void ValidateTimerModel(JObject jsonObj, HashSet<string> usedKeyLetters)
        {
            string timerKeyLetter = jsonObj["subsystems"]?[0]?["model"]?[0]?["KL"]?.ToString();
            string timerStateModelName = $"{timerKeyLetter}_ASSIGNER";

            JToken timerModelToken = jsonObj["subsystems"]?[0]?["model"]?[0];
            JToken timerStateModelToken = jsonObj["subsystems"]?[0]?["model"]?[0]?[timerStateModelName];

            // Check if Timer state model exists
            if (timerStateModelToken == null || timerStateModelToken.Type != JTokenType.Object)
            {
                HandleError($"Syntax error 21: Timer state model not found for TIMER.");
                return;
            }

            // Check KeyLetter of Timer state model
            JToken keyLetterToken = timerStateModelToken?["KeyLetter"];
            if (keyLetterToken == null || keyLetterToken.ToString() != timerKeyLetter)
            {
                HandleError($"Syntax error 21: KeyLetter for TIMER does not match the rules.");
            }
        }

        private void ValidateEventDirectedToStateModelHelper(JToken modelsToken, Dictionary<string, string> stateModels, string modelName)
        {
            if (modelsToken != null && modelsToken.Type == JTokenType.Array)
            {
                foreach (var model in modelsToken)
                {
                    string modelType = model["type"]?.ToString();
                    string className = model["class_name"]?.ToString();

                    if (modelType == "class")
                    {
                        JToken assignerToken = model[$"{className}_ASSIGNER"];

                        if (assignerToken != null)
                        {
                            Console.WriteLine($"assignerToken.Type: {assignerToken.Type}");

                            if (assignerToken.Type == JTokenType.Object)
                            {
                                JToken statesToken = assignerToken["states"];

                                if (statesToken != null && statesToken.Type == JTokenType.Array)
                                {
                                    JArray statesArray = (JArray)statesToken;

                                    foreach (var stateItem in statesArray)
                                    {
                                        string stateName = stateItem["state_name"]?.ToString();
                                        string stateModelName = $"{modelName}.{stateName}";

                                        JToken eventsToken = stateItem["events"];
                                        if (eventsToken is JArray events)
                                        {
                                            foreach (var evt in events)
                                            {
                                                string eventName = evt["event_name"]?.ToString();
                                                JToken targetsToken = evt["targets"];

                                                if (targetsToken is JArray targets)
                                                {
                                                    foreach (var target in targets)
                                                    {
                                                        string targetStateModel = target?.ToString();

                                                        // Check if target state model is in the state models dictionary
                                                        if (!stateModels.ContainsKey(targetStateModel))
                                                        {
                                                            HandleError($"Syntax error 24: Event '{eventName}' in state '{stateModelName}' targets non-existent state model '{targetStateModel}'.");
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void btnHelp_Click(object sender, EventArgs e)
        {
            StringBuilder helpMessage = new StringBuilder();
            helpMessage.AppendLine("User Guide for Translator:");
            helpMessage.AppendLine("");
            helpMessage.AppendLine("1. Find Folder Path:");
            helpMessage.AppendLine("   - Click the 'Browse' button to select the folder with JSON file inside you want to check.");
            helpMessage.AppendLine("   - After selecting the folder, the folder path will be displayed in the path box.");
            helpMessage.AppendLine("2. Translating JSON to C#");
            helpMessage.AppendLine("   - Press 'Translate' button to convert your selected json file into C# source code");
            helpMessage.AppendLine("   - Output will be displayed on Text Box");
            helpMessage.AppendLine("3. Saving Translated code");
            helpMessage.AppendLine("   - Press 'Save' button to save the output into a file or Press 'Copy' button to copy output");
            helpMessage.AppendLine("4. Reset all");
            helpMessage.AppendLine("   - Press 'Reset' button to reset input, output, and selected folder");
            helpMessage.AppendLine("");
            helpMessage.AppendLine("User Guide for Parser");
            helpMessage.AppendLine("");
            helpMessage.AppendLine("1. Find Folder Path:");
            helpMessage.AppendLine("   - Click the 'Browse' button to select the folder with JSON file inside you want to check.");
            helpMessage.AppendLine("   - After selecting the folder, the folder path will be displayed in the path box.");
            helpMessage.AppendLine("2. Initiate Checking:");
            helpMessage.AppendLine("   - After choosing the folder, click the 'Check' button to start the checking process.");
            helpMessage.AppendLine("   - The JSON source code will be displayed on JSON text box and the result of parsing will be displayed on the message box.");
            helpMessage.AppendLine("3. Interpret Checking Results:");
            helpMessage.AppendLine("   - Review the checking results on the message box.");
            helpMessage.AppendLine("   - If there are errors, error messages will be provided to guide the corrections.");

            MessageBox.Show(helpMessage.ToString(), "User Guide", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
