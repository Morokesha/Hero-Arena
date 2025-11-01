using System;
using UnityEngine;

namespace Core.Heroes
{
     [Serializable]
     public class HeroUpgradeData
     {
         public string heroId;
         [Range(1,100)]
         public int currentLevel = 1;
         [Range(0,6)]
         public int stars = 0;
         [Range(0,6)]
         public int redStars = 0;
         
         public HeroBaseStats CalculateFinalStats(HeroBaseStats baseStats) 
         { 
              var finalStats = new HeroBaseStats
             {
                 baseHP = Mathf.RoundToInt(baseStats.baseHP * (1 + currentLevel * HeroUpgradeConstants.LevelCoeff) +
                                           stars * HeroUpgradeConstants.StarBonus + redStars * 
                                           HeroUpgradeConstants.RedStarBonus),
                 baseAttack = Mathf.RoundToInt(baseStats.baseAttack * (1 + currentLevel *  HeroUpgradeConstants.
                     LevelCoeff) + HeroUpgradeConstants.StarBonus + redStars * HeroUpgradeConstants.RedStarBonus),
                 baseDefense = Mathf.RoundToInt(baseStats.baseDefense * (1 + currentLevel *  HeroUpgradeConstants.
                     LevelCoeff) + stars * HeroUpgradeConstants.StarBonus + redStars * 
                     HeroUpgradeConstants.RedStarBonus),
                 
                 critChance = baseStats.critChance,
                 critDamageMultiplier = baseStats.critDamageMultiplier,
                 critChanceResistance = baseStats.critChanceResistance,
                 critDamageResistance = baseStats.critDamageResistance,
                 penetration = baseStats.penetration,
                 damageAbsorption = baseStats.damageAbsorption
             };
        
             return finalStats; 
         }
     }
}