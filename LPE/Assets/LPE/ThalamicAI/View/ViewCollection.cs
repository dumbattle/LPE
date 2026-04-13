using System.Collections.Generic;

namespace LPE.AI.LPE {
    public class ViewCollection<T> {
        static ObjectPool<ViewElement<T>> elementPool = new(() => new ViewElement<T>());

        internal List<ViewElement<T>> elements = new();


        public ViewCollection() {
        }
        public ViewCollection(List<T> data) {
            SetElements(data);
        }



        internal void SetElements(List<T> items) {
            foreach (var e in elements) {
                e.data = default;
                elementPool.Return(e);
            }
            elements.Clear(); 


            foreach (var t in items) {
                var e = elementPool.Get();
                e.data = t;

                elements.Add(e);
            }
        }

        public struct DataEnumerable {
            List<ViewElement<T>> src;
            AttentionLevel minValid;


            internal DataEnumerable(List<ViewElement<T>> elements, AttentionLevel minValid) {
                src = elements;
                this.minValid = minValid;
            }


            public DataEnumerator GetEnumerator() {
                return new DataEnumerator(src, minValid);
            }
        }

        public struct DataEnumerator {
            public T Current => src[index].data;


            List<ViewElement<T>> src;
            int index;
            AttentionLevel minValid;

            internal DataEnumerator(List<ViewElement<T>> elements, AttentionLevel minValid) {
                src = elements;
                this.minValid = minValid;
                index = -1;
            }

            public bool MoveNext() {
                while (index < src.Count) {
                    index++;

                    if (index >= src.Count) {
                        return false;
                    }


                    if (minValid <= src[index].attentionLevel) {
                        return true;
                    }
                }

                return false;
            }
        }





        public struct FullEnumerable {
            List<ViewElement<T>> src;
            AttentionLevel minValid;


            internal FullEnumerable(List<ViewElement<T>> elements, AttentionLevel minValid) {
                src = elements;
                this.minValid = minValid;
            }


            public FullEnumerator GetEnumerator() {
                return new FullEnumerator(src, minValid);
            }
        }

        public struct FullEnumerator {
            public (ViewElement<T> element, T data) Current => (src[index], src[index].data);


            List<ViewElement<T>> src;
            int index;
            AttentionLevel minValid;

            internal FullEnumerator(List<ViewElement<T>> elements, AttentionLevel minValid) {
                src = elements;
                this.minValid = minValid;
                index = -1;
            }

            public bool MoveNext() {
                while (index < src.Count) {
                    index++;

                    if (index >= src.Count) {
                        return false;
                    }


                    if (minValid <= Current.element.attentionLevel) {
                        return true;
                    }
                }

                return false;
            }
        }
    }



}
