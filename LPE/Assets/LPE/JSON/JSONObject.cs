using System.Text;
using System.Collections.Generic;


namespace LPE.JSON {
    public class JSONObject  {
        public Dictionary<string, JSONValue> data = new Dictionary<string, JSONValue>();

        public JSONValue this[string key] {
            get {
                if (!data.ContainsKey(key)) {
                    data.Add(key, new JSONValue());
                }
                return data[key];
            }
        }

        public override string ToString() {
            return ToString(null, 0);
        }

        public void SetKV(string key, JSONValue value) {
            if (!data.ContainsKey(key)) {
                data.Add(key, value);
            }
            else {
                data[key] = value;
            }
        }

        public bool ContainsKey(string key) {
            return data.ContainsKey(key);
        }

        public void RemoveKey(string key) {
            if (data.ContainsKey(key)) {
                data.Remove(key);
            }
        }
        public void Clear() {
            data.Clear();
        }
     
        /// <summary>
        /// Indent = -1 for no whitespace (non-pretty)
        /// </summary>
        public string ToString(StringBuilder sb = null, int indent = 0) {
            sb = sb ?? new StringBuilder();
            if (indent < 0) {
                // stop from going to 0 when incrementing
                indent = -3;
            }
            // openning brace
            if (indent >= 0) {
                sb.Append("{\n");
                Utility.AddIndent(sb, indent + 1);
            }
            else {
                sb.Append("{");
            }
            // properties
            bool first = true; // use for commas
            foreach (var k in data.Keys) {
                // seperators
                if (!first) {
                    if (indent >= 0) {
                        // comma, newline, indent
                        sb.Append(",\n");
                        Utility.AddIndent(sb, indent + 1);
                    }
                    else {
                        // comma only
                        sb.Append(",");
                    }
                }
                // key
                sb.Append("\"");
                sb.Append(k);
                sb.Append(indent >= 0 ? "\" : " :  "\":");
                // value
                data[k].BuildString(sb, indent + 1);
                first = false;
            }

            // closing brace (requires indents)
            if (indent >= 0) {
                sb.Append("\n");
                Utility.AddIndent(sb, indent);
                sb.Append("}");
            }
            else {
                sb.Append("}");
            }
            return sb.ToString();
        }
    }
}
