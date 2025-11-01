using System.Collections.Generic;
using System.Linq;
using Core.Heroes;
using Core.Heroes.Skills;
using Services.FactoryServices;
using UI.CardInBattle;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Core.Battle
{
    public class BattleController : MonoBehaviour
    {
        [SerializeField]
        private TargetSelector targetSelector;
        [SerializeField] 
        private SkillCardManager skillCardManager;
        
        private IFactoryUIService _factoryUI;
        private BattleSpawner _battleSpawner;
        private SkillExecutor _skillExecutor;
        
        private List<Hero> _playerHeroes;
        private List<Hero> _aiHeroes;

        private void Awake()
        {
            _playerHeroes = new List<Hero>();
            _aiHeroes = new List<Hero>();
            
            _skillExecutor = new SkillExecutor();
            _battleSpawner.OnHeroesSpawned += InitializeHeroes;
        }
        public void Construct(IFactoryUIService factoryUIService, BattleSpawner battleSpawner)
        {
            _factoryUI = factoryUIService;
            _battleSpawner = battleSpawner;
            
            skillCardManager.Construct(_factoryUI,_battleSpawner, _skillExecutor, targetSelector);
        }

        private void InitializeHeroes(List<Hero> playerHeroes, List<Hero> aiHeroes)
        {
            _playerHeroes = playerHeroes;
            _aiHeroes = aiHeroes;
            
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
            {
                Debug.Log("Проигрыш");
                // Логика проигрыша
            }
            else if (_aiHeroes.Count == 0)
            {
                Debug.Log("Победа");
                // Логика победы
            }
        }

        private void OnDestroy() =>
            _battleSpawner.OnHeroesSpawned -= InitializeHeroes;
    }
}