using SAS.TagSystem;
using SAS.Utilities;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace SAS.StateMachineGraph.Utilities
{
    [RequireComponent(typeof(Actor))]
    public class PointerPressHandler : MonoBase, IPointerDownHandler, IPointerUpHandler, IPressHandler
    {
        public Action<PointerEventData, bool> OnPress { get; set; }

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            OnPress?.Invoke(eventData, true);
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            OnPress?.Invoke(eventData, false);
        }
    }
}
