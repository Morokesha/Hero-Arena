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
        [Header("Конфиг арены")] [SerializeField]
        private LevelConfig levelConfig;

        [Header("Точки спавна (в префабе арены)")] 
        [SerializeField]
        private List<Transform> playerSpawnPoints;

        [SerializeField] private List<Transform> aiSpawnPoints;

        [Header("Компоненты")] [SerializeField]
        private BattleController battleController;

        [SerializeField] private Transform playerHeroContainer;
        [SerializeField] private Transform aiHeroContainer;

        private IStaticDataService _staticDataService;
        private ISaveLoadService _saveLoadService;
        private IGameFactoryService _gameFactory;

        private List<Hero> _playerHeroes = new();
        private List<Hero> _aiHeroes = new();

        public event Action<List<Hero>, List<Hero>> OnHeroesSpawned;

        public void Construct(IStaticDataService staticDataService, IGameFactoryService iGameFactoryService,
            ISaveLoadService saveLoadService)
        {
            _staticDataService = staticDataService;
            _saveLoadService = saveLoadService;
            _gameFactory = iGameFactoryService;

            LevelSpawn();
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
                Debug.LogError("ArenaConfig.arenaPrefab не задан!");
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

        public void SpawnHeroes()
        {
            var gameProgress = _saveLoadService.GetProgress();
            var assignedSquad = gameProgress.GetSavedSquad();
            Debug.Log($"BattleSpawner SpawnHeroes: assignedSquad has {assignedSquad.Count} heroes");

            // Спавн героев игрока
            for (int i = 0; i < Mathf.Min(assignedSquad.Count, playerSpawnPoints.Count); i++)
            {
                if (assignedSquad.TryGetValue(i, out string heroId))
                {
                    Debug.Log($"Spawning player hero with ID: {heroId}");
                    
                    HeroData heroData = _staticDataService.GetHeroById(heroId);
                    HeroUpgradeData upgradeData = gameProgress.GetHeroUpgrade(heroId);

                    if (heroData != null)
                    {
                        upgradeData = CheckUpgradeDataToNull(upgradeData, heroId, gameProgress);
                        
                        Hero hero = SpawnHero(heroData, upgradeData, i);

                        _playerHeroes.Add(hero);
                            Debug.Log($"Added player hero: {hero.name}");
                    }
                    else
                        Debug.LogError($"Failed to create hero with ID: {heroId}");
                }
                else
                    Debug.LogError($"HeroData for ID {heroId} not found!");
            }

            // Спавн героев AI
            Debug.Log($"BattleSpawner SpawnHeroes: levelConfig.aiHeroes has {levelConfig.aiHeroes.Count} heroes");
            
            for (int i = 0; i < Mathf.Min(levelConfig.aiHeroes.Count, aiSpawnPoints.Count); i++)
            {
                HeroUpgradeData aiUpgradeData = levelConfig.aiHeroes[i];
                HeroData aiHeroData = _staticDataService.GetHeroById(aiUpgradeData.heroId);
                Debug.Log($"Spawning AI hero with ID: {aiUpgradeData.heroId}");

                if (aiHeroData != null)
                {
                    Hero aiHero = SpawnHero(aiHeroData, aiUpgradeData, i);
                    if (aiHero != null)
                    {
                        _aiHeroes.Add(aiHero);
                        Debug.Log($"Added AI hero: {aiHero.name}");
                    }
                    else
                        Debug.LogError($"Failed to create AI hero with ID: {aiUpgradeData.heroId}");
                }
                else
                    Debug.LogError($"AI HeroData for ID {aiUpgradeData.heroId} not found!");
            }

            Debug.Log($"BattleSpawner: Финальное количество героев - Player: {_playerHeroes.Count}," +
                      $" AI: {_aiHeroes.Count}");
            OnHeroesSpawned?.Invoke(_playerHeroes, _aiHeroes);
        }

        private HeroUpgradeData CheckUpgradeDataToNull(HeroUpgradeData upgradeData, string heroId,
            GameProgress gameProgress)
        {
            if (upgradeData == null)
            {
                upgradeData = new HeroUpgradeData
                {
                    heroId = heroId,
                    currentLevel = 1,
                    stars = 0,
                    redStars = 0
                };
                gameProgress.SaveHeroUpgrade(upgradeData);
            }
            
            return upgradeData;
        }


        private Hero SpawnHero(HeroData heroData, HeroUpgradeData upgradeData, int i)
        {
            Hero hero = _gameFactory.CreateHero(heroData, upgradeData, playerSpawnPoints[i]);
            if (hero != null)
            {
                hero.transform.SetParent(playerHeroContainer);
                hero.SetHeroTeam(HeroTeam.Player);
            }

            return hero;
        }
    }
}