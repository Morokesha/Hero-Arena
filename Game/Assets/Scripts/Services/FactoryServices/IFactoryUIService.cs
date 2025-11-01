using Core.Heroes;
using UI;
using UI.CardInMenu;
using UI.CardUI;
using UnityEngine;

namespace Services.FactoryServices
{
    public interface IFactoryUIService
    {
        public HeroSelectionWindow CreateHeroSelectionWindow(RectTransform parentContainer,
            CardSelectionManager cardSelectionManager);

        public HeroCard CreateHeroCard(RectTransform heroListContainer, HeroData heroData,
            CardSelectionManager cardSelectionManager);
        public PurificationCard CreatePurificationCard(RectTransform transform);
    }
}