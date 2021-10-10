using SAS.TagSystem;
using SAS.Utilities;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SAS.StateMachineGraph.Utilities
{
    [RequireComponent(typeof(Actor))]
    public class PointerSelectHandler : MonoBase, ISelectHandler, IDeselectHandler, ISelectionHandler
    {
        public Action<BaseEventData, bool> OnSelect { get; set ; }

        void ISelectHandler.OnSelect(BaseEventData eventData)
        {
            OnSelect?.Invoke(eventData, true);
        }

        void IDeselectHandler.OnDeselect(BaseEventData eventData)
        {
            OnSelect?.Invoke(eventData, false);
        }
    }
}
