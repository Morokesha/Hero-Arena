using DG.Tweening;
using UI.CardInMenu;
using UI.CardUI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Interaction
{
    public abstract class SelectableCard : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private float moveTime = 0.2f;
        [SerializeField] [Range(0, 2f)] private float scaleAmount = 1.2f; 
        [SerializeField] private Image highlightBorder;

        private Vector3 _startScale;
        private Color _originalBorderColor;
        private bool _isSelected = false;

        protected CardSelectionManager SelectionManager;

        private void Start()
        {
            _startScale = transform.localScale;
            if (highlightBorder != null)
                _originalBorderColor = highlightBorder.color;
        }
        
        public void SetSelectionManager(CardSelectionManager manager) => SelectionManager = manager;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!_isSelected)
                SelectCard();
            else
                DeselectCard();
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!_isSelected) 
                AnimateScale(true, 1.1f);
            if (highlightBorder != null)
                highlightBorder.color = Color.yellow;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!_isSelected) 
                AnimateScale(false, 1.0f);
            if (highlightBorder != null)
                highlightBorder.color = _originalBorderColor; 
        }
        
        public void DeselectCard()
        {
            _isSelected = false;
            AnimateScale(false, 1.0f); 
            if (highlightBorder != null)
                highlightBorder.color = _originalBorderColor; 
            
            OnDeselected();
        }
        
        private void SelectCard()
        {
            _isSelected = true;
            AnimateScale(true, scaleAmount);
            
            if (highlightBorder != null) 
                highlightBorder.color = Color.green;
            
            OnSelected(); 
        }

        private void AnimateScale(bool scaleUp, float targetScale)
        {
            Vector3 endScale = _startScale * targetScale;
            transform.DOScale(endScale, moveTime).SetEase(Ease.OutQuad);
        }
        
        protected abstract void OnSelected();
        protected abstract void OnDeselected(); 
    }
}