using System.Collections.Generic;


namespace LPE.JSON {
    public class JsonArray {
        public List<JSONValue> entries = new List<JSONValue>();

        public JSONValue Add() {
            entries.Add(new JSONValue());
            return Last();
        }

        public JSONValue Last() {
            return entries[entries.Count - 1];
        }
    }
}
