
namespace LPE {
    public static class Utility {
        public static void Switch<T>(ref T a, ref T b) {
            T c = a;
            a = b;
            b = c;
        }
    }

    
}