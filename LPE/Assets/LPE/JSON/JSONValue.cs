using System.Text;
using UnityEngine;

namespace LPE.JSON {
    public class JSONValue {
        /// <summary>
        /// shortcut for 'this.AsObject()[key]'
        /// </summary>
        public JSONValue this[string key] {
            get {
                return AsObject()[key];
            }
        }

        float f;
        string s;
        bool b;
        JSONObject o;
        JsonArray a;

        int mode = -1;

        public bool IsString() {
            return mode == 1;
        }
        
        public bool IsObject() {
            return mode == 3;
        }

        public void SetValue(float f) {
            this.f = f;
            mode = 0;
        }
        public void SetValue(string s) {
            this.s = s;
            mode = 1;
        }
        public void SetValue(bool b) {
            this.b = b;
            mode = 2;
        }
        public JSONObject SetAsObject() {
            o ??= new JSONObject();
            o.Clear();
            mode = 3;
            return o;
        }
        public JSONObject SetAsObject(JSONObject o) {
            this.o = o;
            mode = 3;
            return o;
        }
        public JsonArray SetAsArray() {
            a = a ?? new JsonArray();
            a.entries.Clear();
            mode = 4;
            return a;
        }
        public JsonArray SetAsArray(JsonArray a) {
            this.a = a;
            mode = 4;
            return a;
        }
        public void SetAsNull() {
            mode = -1;
        }

        public virtual float GetFloatValue() {
            if(mode != 0) {
                throw new JSONException();
            }
            return f;
        }
        public virtual string GetStringValue() {
            if (mode != 1) {
                throw new JSONException();
            }
            return s;
        }
        public virtual bool GetBoolValue() {
            if (mode != 2) {
                throw new JSONException();
            }
            return b;
        }
        public virtual JSONObject AsObject() {
            if (mode != 3) {
                throw new JSONException();
            }
            return o;
        }
        public virtual JsonArray AsArray() {
            if (mode != 4) {
                throw new JSONException();
            }
            return a;
        }

        public bool IsArray() {
            return mode == 4;
        }

        public void BuildString(StringBuilder sb, int indent) {
            if (indent < 0) {
                // stop from going to 0 when incrementing
                indent = -3;
            }
            switch (mode) {
                case -1:
                    sb.Append("null");
                    break;
                case 0:
                    sb.Append(f);
                    break;
                case 1:
                    sb.Append('"');
                    sb.Append(s);
                    sb.Append('"');
                    break;
                case 2:
                    sb.Append(b ? "true" : "false");
                    break;
                case 3:
                    o.ToString(sb, indent);
                    break;
                case 4:
                    BuildArrayString(sb, indent);
                    break;
            }
        }
        void BuildArrayString(StringBuilder sb, int indent) {
            // openning brace
            if (indent >= 0) {
                sb.Append("[\n");
                Utility.AddIndent(sb, indent + 1);
            }
            else {
                sb.Append("[");
            }
            // properties
            bool first = true; // use for commas
            foreach (var v in a.entries) {
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
                // value
                v.BuildString(sb, indent + 1);
                first = false;
            }

            // closing brace (requires indents)
            if (indent >= 0) {
                sb.Append("\n");
                Utility.AddIndent(sb, indent);
                sb.Append("]");
            }
            else {
                sb.Append("]");
            }
        }

        /// <summary>
        /// Shortcut for 'this.AsArray.Add()'
        /// </summary>
        public JSONValue ArrayAdd() {
            return AsArray().Add();
        }
        /// <summary>
        /// Shortcut for 'this.AsArray.Add().SetAsObject()'
        /// </summary>
        public JSONObject ArrayAddObject() {
            return AsArray().Add().SetAsObject();
        }
        /// <summary>
        /// Shortcut for 'this.AsArray.Add().SetAsArray()'
        /// </summary>
        public JsonArray ArrayAddArray() {
            return AsArray().Add().SetAsArray();
        }
        /// <summary>
        /// Shortcut for 'this.AsArray.Add().SetValue(f)'
        /// </summary>
        public void ArrayAdd(float f) {
            AsArray().Add().SetValue(f);
        }
        /// <summary>
        /// Shortcut for 'this.AsArray.Add().SetValue(s)'
        /// </summary>
        public void ArrayAdd(string s) {
            AsArray().Add().SetValue(s);
        }
        /// <summary>
        /// Shortcut for 'this.AsArray.Add().SetValue(b)'
        /// </summary>
        public void ArrayAdd(bool b) {
            AsArray().Add().SetValue(b);
        }

        /// <summary>
        /// Shortcut for 'this.AsArray.Last()'
        /// </summary>
        public JSONValue ArrayLast() {
            return AsArray().Last();
        }
    }
}
