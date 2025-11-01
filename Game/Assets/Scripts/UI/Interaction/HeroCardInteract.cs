using Core.Heroes;
using UnityEngine;

namespace UI.Interaction
{
    public class HeroCardInteract : SelectableCard
    {
        private HeroData _heroData;
        private HeroCardData _heroCardData;

        public void Construct(HeroData heroData)
        {
            _heroData = heroData;
            _heroCardData = heroData.heroCardData;
        }

        protected override void OnSelected()
        {
            if (_heroCardData != null)
                Debug.Log(_heroCardData + " Hero Card Data");
            else
                Debug.Log("Нет данных карточки");

            SelectionManager.SelectHero(this, _heroData);
        }

        protected override void OnDeselected()
        {
        }
        
        public HeroData GetHeroData() => _heroData;
    }
}