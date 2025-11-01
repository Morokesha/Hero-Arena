using Core.Battle;
using Core.Heroes;
using Services.AssetServices;
using Services.StaticDataServices;
using UnityEngine;
using VContainer;

namespace Services.FactoryServices
{
    public class GameFactoryService : IGameFactoryService
    {

        private readonly IAssetProvider _assetProvider;

        [Inject]
        public GameFactoryService(IAssetProvider assetProvider) => _assetProvider = assetProvider;

        public Hero CreateHero(HeroData heroData, HeroUpgradeData upgradeData, Transform spawnPoint)
        {
            Hero hero = _assetProvider.Instantiate<Hero>(heroData.heroPf, spawnPoint.position);
            hero.transform.rotation = spawnPoint.localRotation;
            hero.Initialize(heroData, upgradeData);
        
            return hero;
        }

        public void SpawnLevel(LevelConfig levelConfig) =>
            _assetProvider.Instantiate<GameObject>(levelConfig.arenaPrefab);
    }
}