using SAS.TagSystem;
using SAS.Utilities;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace SAS.StateMachineGraph.Utilities
{
    [RequireComponent(typeof(Actor))]
    public class SASButton : MonoBase, IPointerClickHandler, IActivatable
    {
        [SerializeField] private bool m_Interactable = true;
        public UnityEvent onClick;

        [FieldRequiresSelf] private Actor _actor;
        [FieldRequiresSelf] private IPointerEventHandler[] _pointerEventHandlers;

        void OnEnable() => Register();
        void OnDisable() => UnRegister();
        private void OnButtonHover(PointerEventData eventData, bool status) => _actor.SetBool("OnHover", status);
        private void OnButtonPress(PointerEventData eventData, bool status) => _actor.SetBool("OnPress", status);
        private void OnButtonSelect(BaseEventData eventData, bool status) => _actor.SetBool("OnSelect", status);
        void IActivatable.SetActive(bool active) => enabled = active;
        void IPointerClickHandler.OnPointerClick(PointerEventData eventData) 
        {
            _actor.SetTrigger("OnClick");
            onClick?.Invoke();
        }
 
        public virtual bool IsInteractable { get { return m_Interactable; } set { m_Interactable = value; } }

        private void Register()
        {
            foreach (var pointerEventHandler in _pointerEventHandlers)
            {
                if (pointerEventHandler is IHoverHandler)
                    (pointerEventHandler as IHoverHandler).OnHover += OnButtonHover;
                if (pointerEventHandler is IPressHandler)
                    (pointerEventHandler as IPressHandler).OnPress += OnButtonPress;
                if (pointerEventHandler is ISelectionHandler)
                    (pointerEventHandler as ISelectionHandler).OnSelect += OnButtonSelect;
            }
        }

        private void UnRegister()
        {
            foreach (var pointerEventHandler in _pointerEventHandlers)
            {
                if (pointerEventHandler is IHoverHandler)
                    (pointerEventHandler as IHoverHandler).OnHover -= OnButtonHover;
                if (pointerEventHandler is IPressHandler)
                    (pointerEventHandler as IPressHandler).OnPress -= OnButtonPress;
                if (pointerEventHandler is ISelectionHandler)
                    (pointerEventHandler as ISelectionHandler).OnSelect -= OnButtonSelect;
            }
        }

        public void OnClickAddListener(UnityAction call) => onClick.AddListener(call);
        public void OnClickRemoveListener(UnityAction call) => onClick.RemoveListener(call);
        public void OnPressAddListener(Action<PointerEventData, bool> call)
        {
            foreach (var pointerEventHandler in _pointerEventHandlers)
            {
                if (pointerEventHandler is IPressHandler)
                {
                    (pointerEventHandler as IPressHandler).OnPress += call;
                    break;
                }
            }
        }

        public void OnPressRemoveListener(Action<PointerEventData, bool> call)
        {
            foreach (var pointerEventHandler in _pointerEventHandlers)
            {
                if (pointerEventHandler is IPressHandler)
                {
                    (pointerEventHandler as IPressHandler).OnPress -= call;
                    break;
                }
            }
        }

        public void OnHoverAddListener(Action<PointerEventData, bool> call)
        {
            foreach (var pointerEventHandler in _pointerEventHandlers)
            {
                if (pointerEventHandler is IHoverHandler)
                {
                    (pointerEventHandler as IHoverHandler).OnHover += call;
                    break;
                }
            }
        }

        public void OnHoverRemoveListener(Action<PointerEventData, bool> call)
        {
            foreach (var pointerEventHandler in _pointerEventHandlers)
            {
                if (pointerEventHandler is IHoverHandler)
                {
                    (pointerEventHandler as IHoverHandler).OnHover -= call;
                    break;
                }
            }
        }

        public void OnSelectAddListener(Action<BaseEventData, bool> call)
        {
            foreach (var pointerEventHandler in _pointerEventHandlers)
            {
                if (pointerEventHandler is ISelectionHandler)
                {
                    (pointerEventHandler as ISelectionHandler).OnSelect += call;
                    break;
                }
            }
        }

        public void OnSelectRemoveListener(Action<BaseEventData, bool> call)
        {
            foreach (var pointerEventHandler in _pointerEventHandlers)
            {
                if (pointerEventHandler is ISelectionHandler)
                {
                    (pointerEventHandler as ISelectionHandler).OnSelect -= call;
                    break;
                }
            }
        }
    }
}
