using System;
using System.Collections.Generic;
using Core.Heroes;
using Services.FactoryServices;
using Services.SaveLoadServices;
using Services.StaticDataServices;
using UnityEngine;
using UnityEngine.Serialization;
using VContainer;

namespace Core.Battle
{
    public class BattleSpawner : MonoBehaviour
    {
        [Header("Конфиг арены")] 
        [SerializeField]
        private LevelConfig levelConfig;

        [Header("Точки спавна (в префабе арены)")] 
        [SerializeField]
        private List<Transform> playerSpawnPoints;
        [SerializeField]
        private List<Transform> aiSpawnPoints;

        [Header("Компоненты")] 
        [SerializeField]
        private BattleController battleController;

        private IStaticDataService _staticDataService;
        private ISaveLoadService _saveLoadService;
        private IGameFactoryService _gameFactory;
        
        private List<Hero> playerHeroes = new List<Hero>();
        private List<Hero> aiHeroes = new List<Hero>();
        
        public event Action<List<Hero>, List<Hero>> OnHeroesSpawned;

        [Inject]
        public void Construct(IStaticDataService staticDataService, IGameFactoryService iGameFactoryService,
            ISaveLoadService saveLoadService)
        {
            _staticDataService = staticDataService;
            _saveLoadService = saveLoadService;
            _gameFactory = iGameFactoryService;

            LevelSpawn();
            SpawnHeroes();
            
            battleController.Construct(this);
        }

        private void LevelSpawn()
        {
            if (levelConfig.arenaPrefab != null)
            {
                _gameFactory.SpawnLevel(levelConfig);
                if (playerSpawnPoints.Count == 0)
                    Debug.LogError("Точки спавна героев игрока не заданы!");
                if (aiSpawnPoints.Count == 0)
                    Debug.LogError("Точки спавна героев противника не заданы!");
            }
            else
            {
                Debug.LogError("ArenaConfig.arenaPrefab не задан!");
                return;
            }
        }

        private void SpawnHeroes()
        { 
            var gameProgress = _saveLoadService.GetProgress();
            var assignedSquad = gameProgress.GetSavedSquad();

            for (int i = 0; i < Mathf.Min(assignedSquad.Count, playerSpawnPoints.Count); i++)
            {
                if (assignedSquad.TryGetValue(i, out string heroId))
                {
                    HeroData heroData = _staticDataService.GetHeroById(heroId);
                    HeroUpgradeData upgradeData = gameProgress.GetHeroUpgrade(heroId);

                    if (heroData != null)
                    {
                        if (upgradeData == null)
                        {
                            upgradeData = new HeroUpgradeData
                            {
                                heroId = heroId,
                                currentLevel = 1, // По умолчанию уже 1, но явно указываем для ясности
                                stars = 0,
                                redStars = 0
                            };
                            
                            gameProgress.SaveHeroUpgrade(upgradeData);
                        }
                        
                        Hero hero = _gameFactory.CreateHero(heroData, upgradeData, playerSpawnPoints[i]);
                        hero.SetHeroTeam(HeroTeam.Player);

                        if (hero != null)
                            playerHeroes.Add(hero);
                    }
                }
            }

            // 3. Спавн героев ИИ
            for (int i = 0; i < Mathf.Min(levelConfig.aiHeroes.Count, aiSpawnPoints.Count); i++)
            {
                HeroUpgradeData aiUpgrade = levelConfig.aiHeroes[i];
                HeroData aiHeroData = _staticDataService.GetHeroById(aiUpgrade.heroId);

                if (aiHeroData != null)
                {
                    Hero aiHero = _gameFactory.CreateHero(aiHeroData, aiUpgrade, aiSpawnPoints[i]);
                    aiHero.SetHeroTeam(HeroTeam.AI);
                    
                    if (aiHero != null)
                        aiHeroes.Add(aiHero);
                }
            }

            OnHeroesSpawned?.Invoke(playerHeroes, aiHeroes);
        }
    }
}