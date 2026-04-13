namespace LPE {
    public static class Singleton<T> where T : new(){
        static T instance;

        public static T Get() {
            if (instance == null) {
                instance = new T();
            }
            return instance;
        }
    }
}