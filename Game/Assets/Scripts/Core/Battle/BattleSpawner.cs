using System;
using System.Collections.Generic;
using System.Linq;
using Core.Heroes;
using Services.FactoryServices;
using Services.SaveLoadServices;
using Services.StaticDataServices;
using UnityEngine;
using VContainer;
using Random = UnityEngine.Random;

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
        
        [SerializeField] 
        private Transform playerHeroContainer;
        [SerializeField] 
        private Transform aiHeroContainer;

        private IStaticDataService _staticDataService;
        private ISaveLoadService _saveLoadService;
        private IGameFactoryService _gameFactory;
        
        private List<Hero> _playerHeroes = new();
        private List<Hero> _aiHeroes = new();

        public event Action<List<Hero>, List<Hero>> OnHeroesSpawned;

        [Inject]
        public void Construct(IStaticDataService staticDataService, IGameFactoryService iGameFactoryService,
            IFactoryUIService factoryUIService, ISaveLoadService saveLoadService)
        {
            _staticDataService = staticDataService;
            _saveLoadService = saveLoadService;
            _gameFactory = iGameFactoryService;
            battleController.Construct(factoryUIService,this);

            LevelSpawn();
            SpawnHeroes();
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
        
        public Hero GetRandomLivingHero(List<Hero> whoseHeroes)
        {
            List<Hero> livingHeroes =
                whoseHeroes.Where(hero => hero.IsAlive()).ToList();
        
            if (livingHeroes.Count == 0)
            {
                Debug.LogWarning("Нет живых героев в списке!");
                return null;
            }
            
            int randomIndex = Random.Range(0, livingHeroes.Count);
            return livingHeroes[randomIndex];
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
                        hero.transform.SetParent(playerHeroContainer);
                        hero.SetHeroTeam(HeroTeam.Player);

                        if (hero != null)
                            _playerHeroes.Add(hero);
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
                    aiHero.transform.SetParent(aiHeroContainer);
                    aiHero.SetHeroTeam(HeroTeam.AI);
                    
                    if (aiHero != null)
                        _aiHeroes.Add(aiHero);
                }
            }

            OnHeroesSpawned?.Invoke(_playerHeroes, _aiHeroes);
        }
    }
}