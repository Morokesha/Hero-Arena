using Core.Heroes;
using Core.Heroes.Skills;
using Services.AssetServices;
using Services.StaticDataServices;
using UI;
using UI.CardInBattle;
using UI.CardInMenu;
using UI.CardUI;
using UnityEngine;

namespace Services.FactoryServices
{
    public class FactoryUIService : IFactoryUIService 
    {
        private readonly IAssetProvider _assetProvider;
        private readonly IStaticDataService _staticDataService;

        public FactoryUIService(IAssetProvider assetProvider, IStaticDataService staticDataService)
        {
            _assetProvider = assetProvider;
            _staticDataService = staticDataService;
        }

        public HeroSelectionWindow CreateHeroSelectionWindow(RectTransform parentContainer,
            CardSelectionManager cardSelectionManager)
        {
            HeroSelectionWindow heroSelectionWindow = _assetProvider.Instantiate<HeroSelectionWindow>
                (AssetPath.HeroSelectionWindow);
            heroSelectionWindow.transform.SetParent(parentContainer.transform, false);
            heroSelectionWindow.Construct(_staticDataService, this, cardSelectionManager);

            return heroSelectionWindow;
        }

        public HeroCard CreateHeroCard(RectTransform heroListContainer, HeroData heroData,
            CardSelectionManager cardSelectionManager)
        {
            HeroCard heroCard = _assetProvider.Instantiate<HeroCard>(AssetPath.HeroCardPath, heroListContainer);
            heroCard.Construct(heroData, cardSelectionManager);

            return heroCard;
        }

        public PurificationCard CreatePurificationCard(RectTransform container)
        {
            PurificationCard purificationCard =
                _assetProvider.Instantiate<PurificationCard>(AssetPath.PurificationCardPath, container);

            return purificationCard;
        }

        public SkillCard CreateSkillCard(RectTransform container)
        {
            SkillCard skillCard = _assetProvider.Instantiate<SkillCard>(AssetPath.SkillCardPath, container);
            return skillCard;
        }
    }
}