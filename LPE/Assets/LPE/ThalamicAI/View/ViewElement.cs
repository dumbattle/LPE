namespace LPE.AI.LPE {
    internal interface IViewElement {
        AttentionLevel attentionLevel { get; set; }
    }
    public class ViewElement<T> : IViewElement {
        internal T data;


        AttentionLevel IViewElement.attentionLevel { get => attentionLevel; set => attentionLevel = value; }
        internal AttentionLevel attentionLevel = AttentionLevel.peripheral;


        public ViewElement() {
        }
        public ViewElement(T data) {
            this.data = data;
        }
    }



}
