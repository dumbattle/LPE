using UnityEngine;
using UnityEngine.EventSystems;
using System;

namespace LPE {
    public class LPEButtonBehaviour : MonoBehaviour, IPointerDownHandler, IPointerClickHandler {
        public bool Clicked { get; private set; }
        public bool Down{ get; private set; }
        
        event Action OnDown;
        event Action OnClick;

        void IPointerDownHandler.OnPointerDown(PointerEventData ped) {
            OnDown?.Invoke();
            Down = true;
        }
        void IPointerClickHandler.OnPointerClick(PointerEventData ped) {
            OnClick?.Invoke();
            Clicked = true;
        }

        
        
        public void SetClickListener(Action cb) {
            OnClick = cb;
        }
        public void SetDownListener(Action cb) {
            OnDown = cb;
        }

        void LateUpdate() {
            Clicked = false;
            Down = false;
        }
    }
}