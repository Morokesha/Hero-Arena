using System;
using System.Collections.Generic;
using Core.Heroes.Skills;
using UnityEngine;

namespace Core.Heroes
{
    [Serializable]
    public class BattleHeroStats
    {
        // Финальные статы (пересчитываются в RecalculateFinalStats)
        public int finalHP;
        public int finalAttack;
        public int finalDefense;
        [Range(0f, 1f)]
        public float finalCritChance;
        [Range(2f, 4.5f)]
        public float finalCritDamageMultiplier;
        public float finalCritChanceResistance;
        [Range(1f, 3.5f)] 
        public float finalCritDamageResistance;
        public float finalPenetration;
        public float finalDamageAbsorption;
        public float finalSpeed;

        public int currentHP;

        // Базовые статы (только для чтения, рассчитываются в начале боя и не меняются)
        private int _baseHp;
        private int _baseAttack;
        private int _baseDefense;
        private float _baseCritChance;
        private float _baseCritDamageMultiplier;
        private float _baseCritChanceResistance;
        private float _baseCritDamageResistance;
        private float _basePenetration;
        private float _baseDamageAbsorption;
        private float _baseSpeed;

        [Header("Поля для ослаблений")] private int _poisonDamagePerTurn = 0;
        private int _bleedDamagePerTurn = 0;
        private bool _isSilenced = false;
        private bool _isStunned = false;

        [Header("Множители для статов (пересчитываются в ApplyAllEffects)")]
        private float _hpMultiplier = 1f;

        private float _attackMultiplier = 1f;
        private float _defenseMultiplier = 1f;
        private float _critChanceMultiplier = 1f;
        private float _critDamageMultiplier = 1f;
        public float critChanceResistanceMultiplier = 1f;
        public float critDamageResistanceMultiplier = 1f;
        public float penetrationMultiplier = 1f;
        private float _damageAbsorptionMultiplier = 1f;
        private float _speedMultiplier = 1f;

        [Header("Дополнительные состояния")] public float shieldAmount = 0f; // Щит (поглощает урон)
        public bool isStunned = false; // Стан (пропуск хода)
        public bool isFrozen = false; // Заморозка (стан + -speed)
        public bool meleeSilenced = false; // Запрет ближнего
        public bool rangeSilenced = false; // Запрет дальнего
        public int silencedTurns = 0; // Общие ходы silence (для обновления)

        private float _shieldMultiplier = 1f; // Множитель для щита (% от HP или абсолют)

        // Расширенная структура для активных эффектов: теперь поддерживает несколько баффов и дебаффов с их длительностями
        [Serializable]
        public class ActiveSkillEffect
        {
            public List<BuffEffect> buffs = new List<BuffEffect>();
            public List<DebuffEffect> debuffs = new List<DebuffEffect>();

            public List<int>
                buffDurations = new List<int>(); // Длительности для каждого баффа (0 — истёк, -1 — постоянный)

            public List<int>
                debuffDurations = new List<int>(); // Длительности для каждого дебаффа (0 — истёк, -1 — постоянный)

            public ActiveSkillEffect(SkillEffect skillEffect)
            {
                if (skillEffect.buffs != null)
                {
                    buffs.AddRange(skillEffect.buffs);
                    foreach (var buff in skillEffect.buffs)
                    {
                        buffDurations.Add(buff.duration > 0 ? buff.duration : (buff.duration == 0 ? -1 : 0));
                    }
                }

                if (skillEffect.debuffs != null)
                {
                    debuffs.AddRange(skillEffect.debuffs);
                    foreach (var debuff in skillEffect.debuffs)
                    {
                        debuffDurations.Add(debuff.duration > 0 ? debuff.duration : (debuff.duration == 0 ? -1 : 0));
                    }
                }
            }

            // Проверка, истёк ли весь эффект (все durations <= 0 и не постоянные)
            public bool IsExpired()
            {
                bool buffsExpired = true;
                foreach (var dur in buffDurations)
                {
                    if (dur > 0 || dur == -1) buffsExpired = false;
                }

                bool debuffsExpired = true;
                foreach (var dur in debuffDurations)
                {
                    if (dur > 0 || dur == -1) debuffsExpired = false;
                }

                return buffsExpired && debuffsExpired;
            }

            // Обновление длительностей: уменьшает временные (>0), возвращает индексы истёкших для удаления
            public (List<int> expiredBuffIndices, List<int> expiredDebuffIndices) UpdateDurations()
            {
                var expiredBuffs = new List<int>();
                for (int i = 0; i < buffDurations.Count; i++)
                {
                    if (buffDurations[i] > 0) buffDurations[i]--;
                    if (buffDurations[i] <= 0 && buffDurations[i] != -1) expiredBuffs.Add(i);
                }

                var expiredDebuffs = new List<int>();
                for (int i = 0; i < debuffDurations.Count; i++)
                {
                    if (debuffDurations[i] > 0) debuffDurations[i]--;
                    if (debuffDurations[i] <= 0 && debuffDurations[i] != -1) expiredDebuffs.Add(i);
                }

                return (expiredBuffs, expiredDebuffs);
            }
        }

        [SerializeField] private List<ActiveSkillEffect> activeEffects = new List<ActiveSkillEffect>();

        public BattleHeroStats(HeroBaseStats baseStats, HeroUpgradeData upgradeData)
        {
            var upgradedStats = upgradeData.CalculateFinalStats(baseStats);

            // Сохраняем базовые статы (только для чтения)
            _baseHp = upgradedStats.baseHP;
            _baseAttack = upgradedStats.baseAttack;
            _baseDefense = upgradedStats.baseDefense;
            _baseCritChance = upgradedStats.critChance;
            _baseCritDamageMultiplier = upgradedStats.critDamageMultiplier;
            _baseCritChanceResistance = upgradedStats.critChanceResistance;
            _baseCritDamageResistance = upgradedStats.critDamageResistance;
            _basePenetration = upgradedStats.penetration;
            _baseDamageAbsorption = upgradedStats.damageAbsorption;
            _baseSpeed = upgradedStats.baseSpeed;

            RecalculateFinalStats();

            currentHP = finalHP;
        }

        public float CalculateMitigatedDamage(float rawDamage, float attackerPenetration)
        {
            // Mitigation: defense снижает урон (например, 1 defense = 0.01% снижения, макс 4000 defense = 40% mitigation)
            float mitigation = finalDefense * 0.01f; // 100 defense = 1% mitigation, 4000 = 40%
            float penetration = attackerPenetration - finalDamageAbsorption; // Эффективное пробитие после поглощения

            if (shieldAmount > 0)
            {
                float absorbed = Mathf.Min(rawDamage, shieldAmount);
                shieldAmount -= absorbed;
                rawDamage -= absorbed; // Щит поглощает урон
            }

            // Если пробитие превышает поглощение и mitigation, наносим бонусный урон на оставшийся %
            if (penetration > mitigation)
                return rawDamage * (1f + (penetration - mitigation));

            return rawDamage * (1f - Mathf.Clamp(mitigation - penetration, 0f, 0.9f));
        }

        public void AddEffectFromSkill(SkillEffect skillEffect)
        {
            // Аналог AddEffect, но только если не expired
            ActiveSkillEffect active = new ActiveSkillEffect(skillEffect);
            if (!active.IsExpired()) activeEffects.Add(active);
            ApplyAllEffects();
        }

        public void AddEffect(SkillEffect skillEffect)
        {
            activeEffects.Add(new ActiveSkillEffect(skillEffect));
            ApplyAllEffects();
        }

        // Обновлённый метод: удаляет бафф по типу (если есть в активных эффектах)
        public void RemoveBuff(BuffEffectType buffType)
        {
            foreach (var effect in activeEffects)
            {
                for (int i = effect.buffs.Count - 1; i >= 0; i--)
                {
                    if (effect.buffs[i].buffEffectType == buffType)
                    {
                        effect.buffDurations[i] = 0; // Истекаем бафф немедленно
                    }
                }
            }

            ApplyAllEffects();
        }

        // Аналогичный метод для дебаффа
        public void RemoveDebuff(DebuffType debuffType)
        {
            foreach (var effect in activeEffects)
            {
                for (int i = effect.debuffs.Count - 1; i >= 0; i--)
                {
                    if (effect.debuffs[i].debuffEffectType == debuffType)
                    {
                        effect.debuffDurations[i] = 0; // Истекаем дебафф немедленно
                    }
                }
            }

            ApplyAllEffects();
        }

        private void ResetModifiers()
        {
            _hpMultiplier = 1f;
            _attackMultiplier = 1f;
            _defenseMultiplier = 1f;
            _critChanceMultiplier = 1f;
            _critDamageMultiplier = 1f;
            critChanceResistanceMultiplier = 1f;
            critDamageResistanceMultiplier = 1f;
            penetrationMultiplier = 1f;
            _damageAbsorptionMultiplier = 1f;
            _speedMultiplier = 1f;
            _shieldMultiplier = 1f;
            shieldAmount = 0f;
            isStunned = false;
            isFrozen = false;
            meleeSilenced = false;
            rangeSilenced = false;
            silencedTurns = 0;

            // Сброс субстатов к базовым
            finalCritChance = _baseCritChance;
            finalCritDamageMultiplier = _baseCritDamageMultiplier;
            finalCritChanceResistance = _baseCritChanceResistance;
            finalCritDamageResistance = _baseCritDamageResistance;
            finalPenetration = _basePenetration;
            finalDamageAbsorption = _baseDamageAbsorption;
            finalSpeed = _baseSpeed;

            // Сброс переменных ослаблений
            _poisonDamagePerTurn = 0;
            _bleedDamagePerTurn = 0;
            _isSilenced = false;
            _isStunned = false;
        }

        // Применить один активный эффект (применяет все баффы и дебаффы, если их durations > 0 или -1)
        private void ApplyBuffs(ActiveSkillEffect effect)
        {
            for (int i = 0; i < effect.buffs.Count; i++)
            {
                if (effect.buffDurations[i] > 0 || effect.buffDurations[i] == -1)
                {
                    var buff = effect.buffs[i];
                    switch (buff.buffEffectType)
                    {
                        case BuffEffectType.HpBoost:
                            _hpMultiplier *= buff.value;
                            break;
                        case BuffEffectType.AttackBoost:
                            _attackMultiplier *= buff.value;
                            break;
                        case BuffEffectType.DefenseBoost:
                            _defenseMultiplier *= buff.value;
                            break;
                        case BuffEffectType.SpeedBoost:
                            _speedMultiplier *= buff.value;
                            break;
                        // Shield теперь в DamageCalculator, но если нужно fallback — оставь
                        case BuffEffectType.RankUp:
                            // Событие для ранга
                            break;
                        case BuffEffectType.Cleanse:
                            // Очистка дебаффов
                            break;
                    }
                }
            }
        }

        private void ApplyDebuffs(ActiveSkillEffect effect)
        {
            for (int i = 0; i < effect.debuffs.Count; i++)
            {
                if (effect.debuffDurations[i] > 0 || effect.debuffDurations[i] == -1)
                {
                    var debuff = effect.debuffs[i];
                    switch (debuff.debuffEffectType)
                    {
                        case DebuffType.Poison:
                            _poisonDamagePerTurn += Mathf.RoundToInt(debuff.value);
                            break;
                        case DebuffType.Bleed:
                            _bleedDamagePerTurn += Mathf.RoundToInt(debuff.value);
                            break;
                        case DebuffType.BaseStatsReduction:
                            _hpMultiplier *= debuff.value;
                            _attackMultiplier *= debuff.value;
                            break;
                        case DebuffType.SubStatsReduction:
                            _critChanceMultiplier *= debuff.value;
                            _critDamageMultiplier *= debuff.value;
                            critChanceResistanceMultiplier *= debuff.value;
                            critDamageResistanceMultiplier *= debuff.value;
                            penetrationMultiplier *= debuff.value;
                            _damageAbsorptionMultiplier *= debuff.value;
                            _speedMultiplier *= debuff.value;
                            break;
                        case DebuffType.SilenceSingleTarget:
                        case DebuffType.SilenceArea:
                            _isSilenced = true;
                            break;
                        case DebuffType.DefenseDown:
                            _defenseMultiplier *= debuff.value;
                            break;
                        case DebuffType.AttackDown:
                            _attackMultiplier *= debuff.value;
                            break;
                        case DebuffType.Stun:
                            isStunned = true;
                            silencedTurns += debuff.duration;
                            break;
                        case DebuffType.Freeze:
                            isFrozen = true;
                            _speedMultiplier *= 0.5f;
                            silencedTurns += debuff.duration;
                            break;
                        case DebuffType.MeleeSilence:
                            meleeSilenced = true;
                            silencedTurns += debuff.duration;
                            break;
                        case DebuffType.RangeSilence:
                            rangeSilenced = true;
                            silencedTurns += debuff.duration;
                            break;
                    }
                }
            }
        }

        private void ApplyAllEffects()
        {
            ResetModifiers();
            foreach (var effect in activeEffects)
            {
                ApplyBuffs(effect);
                ApplyDebuffs(effect);
            }

            RecalculateFinalStats();
        }

        public void UpdateEffects()
        {
            UpdateSilenceState();
            for (int i = activeEffects.Count - 1; i >= 0; i--)
            {
                var effect = activeEffects[i];
                var (expiredBuffs, expiredDebuffs) = effect.UpdateDurations();

                // Удаляем истёкшие (сортировка для безопасного удаления)
                expiredBuffs.Sort((a, b) => b.CompareTo(a));
                foreach (var idx in expiredBuffs)
                {
                    effect.buffs.RemoveAt(idx);
                    effect.buffDurations.RemoveAt(idx);
                }

                expiredDebuffs.Sort((a, b) => b.CompareTo(a));
                foreach (var idx in expiredDebuffs)
                {
                    effect.debuffs.RemoveAt(idx);
                    effect.debuffDurations.RemoveAt(idx);
                }

                if (effect.IsExpired()) activeEffects.RemoveAt(i);
            }

            ApplyAllEffects();
        }

        // Пересчитать финальные статы на основе базовых + множителей + ограничений
        private void RecalculateFinalStats()
        {
            // Пересчет с множителями
            finalHP = Mathf.RoundToInt(_baseHp * _hpMultiplier);
            finalAttack = Mathf.RoundToInt(_baseAttack * _attackMultiplier);
            finalDefense = Mathf.RoundToInt(_baseDefense * _defenseMultiplier);
            finalCritChance = _baseCritChance * _critChanceMultiplier;
            finalCritDamageMultiplier = _baseCritDamageMultiplier * _critDamageMultiplier;
            finalCritChanceResistance = _baseCritChanceResistance * critChanceResistanceMultiplier;
            finalCritDamageResistance = _baseCritDamageResistance * critDamageResistanceMultiplier;
            finalPenetration = _basePenetration * penetrationMultiplier;
            finalDamageAbsorption = _baseDamageAbsorption * _damageAbsorptionMultiplier;
            finalSpeed = _baseSpeed * _speedMultiplier;

            // Применение ограничений (caps)
            // HP не ниже 0
            finalHP = Mathf.Max(finalHP, 0);
            // Attack не ниже 0
            finalAttack = Mathf.Max(finalAttack, 0);
            // Defense: 0 - 4000 (макс 40% mitigation)
            finalDefense = Mathf.Clamp(finalDefense, 0, 4000);
            // CritChance: 0% - 50%, сопротивление снижает напрямую, макс снижение 40% от baseCritChance (но clamped)
            finalCritChance = Mathf.Clamp(finalCritChance - finalCritChanceResistance, 0f, 0.5f);
            // CritDamage: 100% - 350%, сопротивление снижает напрямую
            finalCritDamageMultiplier = Mathf.Clamp(finalCritDamageMultiplier - finalCritDamageResistance,
                1.0f, 3.5f);
            // CritChanceResistance: 0% - 40% (макс снижение critChance на 40%)
            finalCritChanceResistance = Mathf.Clamp(finalCritChanceResistance, 0f, 0.4f);
            // CritDamageResistance: 0% - 250%
            finalCritDamageResistance = Mathf.Clamp(finalCritDamageResistance, 0f, 2.5f);
            // Penetration: 0% - 90%
            finalPenetration = Mathf.Clamp(finalPenetration, 0f, 0.9f);
            // DamageAbsorption: 0% - 40%
            finalDamageAbsorption = Mathf.Clamp(finalDamageAbsorption, 0f, 0.4f);
            // Speed от 50 до 150
            finalSpeed = Mathf.Clamp(finalSpeed, 50f, 150f);
        }

        private void UpdateSilenceState()
        {
            if (silencedTurns > 0)
                silencedTurns--;

            if (silencedTurns <= 0)
            {
                isStunned = false;
                isFrozen = false;
                meleeSilenced = false;
                rangeSilenced = false;
            }
        }

        // Метод: применить урон от DoT
        public void ApplyDotDamage()
        {
            int totalDot = _poisonDamagePerTurn + _bleedDamagePerTurn;
            if (totalDot > 0)
            {
                currentHP -= totalDot;
                // Можно добавить событие OnDamageTaken или VFX
            }
        }
    }
}