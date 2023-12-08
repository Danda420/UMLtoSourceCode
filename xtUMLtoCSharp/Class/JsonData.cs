using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UMLtoSourceCode.Class
{
    public class JsonData
    {
        public string type { get; set; }
        public string sub_id { get; set; }
        public string sub_name { get; set; }
        public Model[] model { get; set; }

        public class Model
        {
            public string type { get; set; }
            public string class_id { get; set; }
            public string class_name { get; set; }
            public string KL { get; set; }
            public Attribute1[] attributes { get; set; }
            public State[] states { get; set; }
            public string name { get; set; }
            public Class1[] @class { get; set; }
            public Model1 model { get; set; }
        }

        public class Model1
        {
            public string type { get; set; }
            public string class_id { get; set; }
            public string class_name { get; set; }
            public string KL { get; set; }
            public Attribute[] attributes { get; set; }
        }

        public class Attribute
        {
            public string attribute_type { get; set; }
            public string attribute_name { get; set; }
            public string data_type { get; set; }
        }

        public class Attribute1
        {
            public string attribute_type { get; set; }
            public string attribute_name { get; set; }
            public string data_type { get; set; }
            public string default_value { get; set; }
        }

        public class State
        {
            public string state_id { get; set; }
            public string state_name { get; set; }
            public string state_value { get; set; }
            public string state_type { get; set; }
            public string[] state_event { get; set; }
            public string[] state_function { get; set; }
        }

        public class Class1
        {
            public string class_id { get; set; }
            public string class_name { get; set; }
            public string class_multiplicity { get; set; }
        }
    }
}
