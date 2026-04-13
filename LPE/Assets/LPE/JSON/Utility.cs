using System.Text;


namespace LPE.JSON {
    public static class Utility {
        public const string indent = "\t";

        public static void AddIndent(StringBuilder sb, int indentLevel) {
            for (int i = 0; i < indentLevel; i++) {
                sb.Append(indent);
            }
        }
    }
}
