using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Core.Heroes
{
    [Serializable]
    public class HeroBaseStats
    {
        [Range(9500, 200000)] 
        public int baseHP;
    
        [Range(830, 20000)] 
        public int baseAttack;
    
        [Range(550, 4000)] // Defense: 0-4000 (макс 40% mitigation)
        public int baseDefense;

        [Range(50, 150)] // Speed: 50-150
        public int baseSpeed;

        [Space(5)]
        [Range(0f, 0.5f)] // CritChance: 0%-50%
        public float critChance;
    
        [Range(1f, 3.5f)] // CritDamageMultiplier: 100%-350%
        public float critDamageMultiplier;
    
        [Range(0f, 0.4f)] // CritChanceResistance: 0%-40%
        public float critChanceResistance;
    
        [Range(0f, 2.5f)] // CritDamageResistance: 0%-250%
        public float critDamageResistance;

        [Space(5)]
        [Range(0f, 0.9f)] // Penetration: 0%-90%
        public float penetration;
    
        [Range(0f, 0.4f)] // DamageAbsorption: 0%-40%
        public float damageAbsorption;
    }
}