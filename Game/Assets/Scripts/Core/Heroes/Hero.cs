using System;
using System.Collections.Generic;
using Core.Battle;
using Core.Heroes.Skills;
using UnityEngine;

namespace Core.Heroes
{
    public class Hero : MonoBehaviour
    {
        public event Action<int> OnRankUp; 
         public event Action<Hero> OnDeath; 
         public event Action<float> OnDamageTaken; // Новое: для эффектов после урона (например, бафф атакующего)
         
         private HeroTeam _heroTeam; 
         private HeroData _heroData; 
         private DamageCalculator _damageCalculator;
         private HeroUpgradeData _upgradeData;
         public BattleHeroStats battleStats;
         
         private bool _isAlive;
         public int ultimatePoints;


         public void Initialize(HeroData heroData, HeroUpgradeData upgradeData)
         {
             _heroData = heroData;
             _upgradeData = upgradeData;
             _damageCalculator = new DamageCalculator(this);
             battleStats = new BattleHeroStats(_heroData.heroBaseStats, _upgradeData); 
         }
         
         public void SetHeroTeam(HeroTeam heroTeam) => _heroTeam = heroTeam;
  
         public HeroTeam GetHeroTeam() => _heroTeam;
         public bool IsAlive() => _isAlive;

         public HeroSkillsData GetHeroSkillData() => _heroData.heroSkillsData;
         
        public void TakeDamage(int damageAmount) 
        { 
            battleStats.currentHP -= damageAmount; // battleStats имеет currentHP
            battleStats.currentHP = Mathf.Max(0, battleStats.currentHP);

        //  Вызвать событие для эффектов после урон
            OnDamageTaken?.Invoke(damageAmount);
            
            if (battleStats.currentHP <= 0) 
            {
                OnDeath?.Invoke(this); // Интегрируй с TurnManager для удаления
            } 
        }

    // Метод: применить дебафф (добавляет в battleStats)
        public void ApplyEffects(List<SkillEffect> skillEffects) 
        {
            foreach (var effect in skillEffects)
                battleStats.AddEffect(effect);
        }

        public DamageCalculator GetDamageCalculator() => _damageCalculator;

        public void PlayAnimation(AnimationClip attackClip)
        {
            
        }
    }

    public enum HeroTeam
    {
        Player,
        AI
    }
}