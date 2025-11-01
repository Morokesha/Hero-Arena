using System.Collections.Generic;
using System.Linq;
using Core.Extension;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Serialization;

namespace Core.Heroes.Skills
{
    // Новый enum для рарности навыков
    public enum SkillRarity
    {
        Bronze,
        Silver,
        Gold
    }

    public enum SkillType
    {
        Active,
        Ultimate,
        Passive
    }

    public enum EffectCategory
    {
        MeleeAttackDamage,
        RangeDamage,
        Debuff,
        Buff,
        HealCleanse,
        Ultimate,
        Shield
    }

    [CreateAssetMenu(fileName = "Skills", menuName = "Skills/NewSkill", order = 0)]
    public class Skill : ScriptableObject
    {
        public SkillType skillType;
        [Space(5)] public Sprite skillIcon;

        [Space(5)] [Tooltip("Категория эффекта (влияет на иконку навыка)")]
        public EffectCategory effectCategory;

        [ReadOnly, Tooltip("Иконка обновляется автоматически на основе EffectCategory первого уровня")]
        public Sprite skillTypeIcon;

        public string skillName;

        [Tooltip("Префаб VFX")] public GameObject visualEffectPrefab;
        [Space(2)] [Tooltip("Анимации атак")] public AnimationClip attackClip;

        [Header("Effect Category Icons"), Tooltip("Иконка для SkillType")]
        public SkillEffectIconConfig iconConfig;

        [Tooltip("Уровни навыка: всегда 3 уровня (Bronze, Silver, Gold). Элементы можно перетаскивать для reorder.")]
        public List<SkillLevelData> skillLevels = new();
        
        [TextArea,
         Tooltip("Общее описание навыка. Используй {damage} как плейсхолдер для урона (он заменится на % из уровня)")]
        public string description;

        [Tooltip("Цель: все или один? (Общее для всех уровней)")]
        public bool targetAll = false;


        public SkillLevelData GetLevelData(SkillRarity rarity) =>
            skillLevels?.FirstOrDefault(l => l.rarity == rarity);
        
        public string GetFormattedDescription(SkillRarity rarity)
        {
            var levelData = GetLevelData(rarity);
            if (levelData == null)
                return description; 
            
            string damagePercent = (levelData.damageModifier * 100).ToString("0") + "%";
            return description.Replace("{damage}", damagePercent);
        }

        private void UpdateSkillTypeIcon()
        {
            if (skillLevels == null || skillLevels.Count == 0 || iconConfig == null) return;

            skillTypeIcon = effectCategory switch
            {
                EffectCategory.MeleeAttackDamage => iconConfig.meleeAttackDamageIcon,
                EffectCategory.RangeDamage => iconConfig.rangeDamageIcon,
                EffectCategory.Debuff => iconConfig.debuffIcon,
                EffectCategory.Buff => iconConfig.buffIcon,
                EffectCategory.HealCleanse => iconConfig.healCleanseIcon,
                EffectCategory.Ultimate => iconConfig.ultimateIcon,
                _ => null
            };
        }

        private void OnValidate()
        {
            skillLevels ??= new List<SkillLevelData>();

            // Если меньше 3, добавляем пустые элементы
            while (skillLevels.Count < 3)
                skillLevels.Add(new SkillLevelData());

            if (skillLevels.Count > 3)
                skillLevels = skillLevels.Take(3).ToList();

            for (int i = 0; i < skillLevels.Count; i++)
            {
                skillLevels[i] ??= new SkillLevelData();
                skillLevels[i].rarity = (SkillRarity)i;
            }

            UpdateSkillTypeIcon();
        }

        public void Activate(Hero caster, List<Hero> targets, SkillRarity rarity)
        {
            var levelData = GetLevelData(rarity);
            if (levelData == null) return;

            // Анимация/VFX (твой код)
            if (attackClip != null) caster.PlayAnimation(attackClip); // Добавь метод в Hero

            // Используем общее targetAll из Skill (не из levelData)
            List<Hero> actualTargets = targetAll ? targets : new List<Hero> { targets[0] };

            // Применить к caster, если бафф (используем effectCategory из Skill)
            if (effectCategory == EffectCategory.Buff || effectCategory == EffectCategory.HealCleanse)
            {
                actualTargets = new List<Hero> { caster };
            }

            foreach (var target in actualTargets) 
                caster.GetComponent<DamageCalculator>().ApplySkillDamage(target, this, levelData);
        }

        [System.Serializable]
        public class SkillLevelData
        {
            [Tooltip("Рарность уровня навыка (автоматически: 0-Bronze, 1-Silver, 2-Gold)")]
            public SkillRarity rarity;

            [Tooltip("Множитель урона (от атаки героя, заменяет старый damageMultiplier если нужно)")] [Range(0.1f, 5f)]
            public float damageModifier = 1f;

            [Tooltip("Пробитие брони (доп. % игнора)")] [Range(0f, 1f)]
            public float extraPenetration = 0f;

            [Tooltip("Крит? (игнорирует resistance цели)")]
            public bool isCritical = false; // Для ultimate

            [Header("Эффекты (баффы/дебаффы)")]
            public List<SkillEffect> effects;
        }
    }
}