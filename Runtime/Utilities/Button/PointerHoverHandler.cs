using SAS.Utilities;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SAS.StateMachineGraph.Utilities
{
    [RequireComponent(typeof(Actor))]
    public class PointerHoverHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IHoverHandler
    {
        public Action<PointerEventData, bool> OnHover { get; set ; }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            OnHover?.Invoke(eventData, true);
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            OnHover?.Invoke(eventData, false);
        }
    }
}
