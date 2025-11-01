using Core.Heroes;
using UI.CardInMenu;
using UI.Interaction;
using UnityEngine;

namespace UI.CardUI
{
    public class HeroCard : Card
    {
        [SerializeField]
        private HeroCardInteract heroCardInteract;
        
        private HeroCardData _heroCardData; 
        private HeroData _heroData;
        
        private bool _isInSquad = false;

        public void Construct(HeroData heroData, CardSelectionManager cardSelectionManager)
        {
            _heroData = heroData;
            _heroCardData = heroData.heroCardData;
            base.Initialize(_heroCardData.heroAvatar, _heroCardData.heroElement, _heroCardData.colorElement);
            heroCardInteract.SetSelectionManager(cardSelectionManager);
            heroCardInteract.Construct(_heroData);
        }

        public override void OnCardClick()
        {
        }
        
        public HeroData GetHeroData() => _heroData;
    }
}