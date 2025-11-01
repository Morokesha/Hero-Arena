using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Core.Heroes.Skills
{
    [Serializable]
    public class HeroSkillsData
    {
        [Space(5)]
        [Header("Skills")]
        [Tooltip("Пассивный навык (1 шт)")]
        public Skill passiveSkillData;
        
        [Tooltip("Активные навыки (2 шт)")]
        public Skill activeSkill1; 
        public Skill activeSkill2; 
        
        [FormerlySerializedAs("ultimateSkillData")]
        [Space(5)]
        [Tooltip("Ультимативный навык (1 шт)")]
        public Skill ultimateSkill; 
    }
}