using System.Linq;
using Core.Heroes.Skills;
using UnityEngine;

namespace Core.Heroes
{
    public class DamageCalculator
    {
        private Hero _owner;
        private BattleHeroStats stats; // Ссылка на статы героя

        public DamageCalculator(Hero hero)
        {
            _owner = hero;
            stats = hero.battleStats;
        }

        // Обновлённая сигнатура: теперь передаём Skill для доступа к effectCategory
        public void ApplySkillDamage(Hero target, Skill skill, Skill.SkillLevelData skillLevelData)
        {
            if (skill == null || skillLevelData == null) return;

            // Шаг 1: Урон, если категория подразумевает (Melee/Range/Ultimate) — используем skill.effectCategory
            if (skill.effectCategory == EffectCategory.MeleeAttackDamage ||
                skill.effectCategory == EffectCategory.RangeDamage ||
                skill.effectCategory == EffectCategory.Ultimate)
            {
                // Условия: если цель silenced/stunned/isFrozen
                if (target.battleStats.isStunned || target.battleStats.isFrozen || (skill.effectCategory ==
                        EffectCategory.MeleeAttackDamage && target.battleStats.meleeSilenced) ||
                    (skill.effectCategory == EffectCategory.RangeDamage && target.battleStats.rangeSilenced))
                    return;

                // Используем damageModifier вместо старого damageMultiplier
                float rawDamage = CalculateRawDamage(skillLevelData.damageModifier);

                // Доп. пробитие от навыка
                float totalPen = stats.finalPenetration + skillLevelData.extraPenetration;

                // Крит?
                bool isCrit = false;
                if (skillLevelData.isCritical)
                {
                    isCrit = true; // Игнор resistance
                }
                else
                {
                    float critRoll = Random.Range(0f, 1f);
                    isCrit = critRoll < stats.finalCritChance;
                }

                float finalDamage = target.battleStats.CalculateMitigatedDamage(rawDamage, totalPen);
                if (isCrit) finalDamage *= stats.finalCritDamageMultiplier;

                target.TakeDamage((int)finalDamage);
            }
            else if (skill.effectCategory == EffectCategory.HealCleanse)
                ApplyHeal(target, skillLevelData);
            else if (skill.effectCategory == EffectCategory.Shield) 
                ApplyShield(target, skillLevelData);
            
            foreach (var effect in skillLevelData.effects.Where(effect => effect != null))
                target.battleStats.AddEffectFromSkill(effect);
        }

        // Новый метод: применение лечения
        private void ApplyHeal(Hero target, Skill.SkillLevelData skillLevelData)
        {
            if (skillLevelData.damageModifier > 0)
            {
                float healAmount = stats.finalAttack * skillLevelData.damageModifier;
                target.battleStats.currentHP = Mathf.Min(target.battleStats.currentHP + (int)healAmount,
                    target.battleStats.finalHP);
                // Можно добавить VFX или событие OnHeal
            }
        }

        // Новый метод: наложение щита
        private void ApplyShield(Hero target, Skill.SkillLevelData skillLevelData)
        {
            if (skillLevelData.damageModifier > 0)
            {
                // Предполагаем, что damageModifier для щита — это % от maxHP или абсолют (как в BuffEffect)
                if (skillLevelData.damageModifier > 1f)
                    target.battleStats.shieldAmount +=
                        target.battleStats.finalHP * (skillLevelData.damageModifier - 1f);
                else
                    target.battleStats.shieldAmount += skillLevelData.damageModifier * 100; // Абсолютный
                // Можно добавить VFX или событие OnShieldApplied
            }
        }

        protected virtual int CalculateRawDamage(float multiplier) =>
            (int)(stats.finalAttack * multiplier);
    }
}