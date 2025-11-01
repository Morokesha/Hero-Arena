using System.Collections.Generic;
using System.Linq;
using Core.Heroes;
using Core.Heroes.Skills;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Core.Battle
{
    public class BattleController : MonoBehaviour
    {
        [SerializeField]
        private TargetSelector targetSelector;
        
        private BattleSpawner _battleSpawner;
        private SkillExecutor _skillExecutor;
        
        private List<Hero> _playerHeroes;
        private List<Hero> _aiHeroes;

        private void Awake()
        {
            _skillExecutor = new SkillExecutor();
            _battleSpawner.OnHeroesSpawned += InitializeHeroes;
        }
        public void Construct(BattleSpawner battleSpawner) =>
            _battleSpawner = battleSpawner;

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

        private void OnDestroy() => _battleSpawner.OnHeroesSpawned -= InitializeHeroes;

        public List<Hero> GetAllHeroesPlayer() => _playerHeroes;

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
    }
}