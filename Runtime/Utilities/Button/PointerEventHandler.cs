using System;
using UnityEngine.EventSystems;

namespace SAS.StateMachineGraph.Utilities
{
    interface IPointerEventHandler
    { 
    }

    interface IPressHandler: IPointerEventHandler
    {
        Action<PointerEventData, bool> OnPress { get; set; }
    }

    interface IHoverHandler : IPointerEventHandler
    {
        Action<PointerEventData, bool> OnHover { get; set; }
    }

    interface ISelectionHandler : IPointerEventHandler
    {
        Action<BaseEventData, bool> OnSelect { get; set; }
    }
}