using System.Collections.Generic;
using Core.Heroes;
using Core.Heroes.Skills;
using Services.FactoryServices;
using Services.SaveLoadServices;
using Services.StaticDataServices;
using UnityEngine;
using VContainer;

namespace Core.Battle
{
    public class BattleController : MonoBehaviour
    {
        [SerializeField]
        private TargetSelector targetSelector;
        [SerializeField] 
        private SkillCardManager skillCardManager;
        [SerializeField]
        private BattleSpawner battleSpawner;
        
        private IFactoryUIService _factoryUI;
        private SkillExecutor _skillExecutor;

        private List<Hero> _playerHeroes;
        private List<Hero> _aiHeroes;
        
        [Inject]
        public void Construct(IStaticDataService staticDataService, IGameFactoryService gameFactoryService,
            IFactoryUIService factoryUIService, ISaveLoadService saveLoadService)
        {
            _playerHeroes = new List<Hero>();
            _aiHeroes = new List<Hero>();
            _skillExecutor = new SkillExecutor();
            
            _factoryUI = factoryUIService;
            
            battleSpawner.Construct(staticDataService, gameFactoryService, saveLoadService);
            skillCardManager.Construct(_factoryUI, battleSpawner, _skillExecutor, targetSelector);
            battleSpawner.OnHeroesSpawned += InitializeHeroes;
            battleSpawner.SpawnHeroes();
        }
        
        private void InitializeHeroes(List<Hero> playerHeroes, List<Hero> aiHeroes)
        {
            _playerHeroes = playerHeroes;
            _aiHeroes = aiHeroes;
            
            Debug.Log($"BattleController InitializeHeroes: Received {_playerHeroes.Count} " +
                      $"player heroes and {_aiHeroes.Count} AI heroes");
            
            foreach (var hero in playerHeroes)
                hero.OnDeath += OnHeroDeath;

            foreach (var hero in aiHeroes)
                hero.OnDeath += OnHeroDeath;
        }

        private void OnHeroDeath(Hero deadHero)
        {
            _playerHeroes.Remove(deadHero);
            _aiHeroes.Remove(deadHero);

            if (_playerHeroes.Count == 0)
                Debug.Log("Проигрыш");
            
            else if (_aiHeroes.Count == 0) 
                Debug.Log("Победа");
        }

        private void OnDestroy() => battleSpawner.OnHeroesSpawned -= InitializeHeroes;
    }
}