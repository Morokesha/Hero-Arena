using Core.Battle;
using Core.Heroes;
using UnityEngine;

namespace Services.FactoryServices
{
    public interface IGameFactoryService
    {
        public Hero CreateHero(HeroData heroData, HeroUpgradeData upgradeData, Transform spawnPoint);
        public void SpawnLevel(LevelConfig levelConfig);
    }
}