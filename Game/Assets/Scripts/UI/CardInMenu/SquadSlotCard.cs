using System;
using Core.Heroes;
using TMPro;
using UI.Interaction;
using UnityEngine;
using UnityEngine.UI;

namespace UI.CardUI
{
    public class SquadSlotCard : SelectableCard
    {
        public event Action<SquadSlotCard> OnSlotSelected;

        [SerializeField] private Button button;
        [SerializeField] private Image heroAvatar;
        [SerializeField] private TextMeshProUGUI selectTxt;
        
        private HeroCardData _heroCardData;
        
        private int _slotIndex;

        private void Awake()
        {
            button.onClick.AddListener(OnClickSlot);
            heroAvatar.gameObject.SetActive(false);
        }

        public void AssignedHeroInSlot(HeroData heroData)
        {
            _heroCardData = heroData.heroCardData;
            
            selectTxt.gameObject.SetActive(false);
            heroAvatar.gameObject.SetActive(true);
            heroAvatar.sprite = _heroCardData.heroAvatar;
        }

        public void SetSlotIndex(int index) => _slotIndex = index;
        public int GetSlotIndex() => _slotIndex;
        public void ClearSlot()
        {
            _heroCardData = null;
            
            heroAvatar.gameObject.SetActive(false);
            selectTxt.gameObject.SetActive(true);
            
            DeselectCard();
        }
        
        private void OnClickSlot() =>
            OnSlotSelected?.Invoke(this);

        protected override void OnSelected()
        {
            SelectionManager.SelectSlot(this);
        }

        protected override void OnDeselected()
        {
        }

        private void OnDestroy() => button.onClick.RemoveListener(OnClickSlot);
    }
}