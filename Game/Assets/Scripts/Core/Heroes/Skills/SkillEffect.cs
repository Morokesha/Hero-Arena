using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Heroes.Skills
{
    // Обновлённые структуры для SkillEffect (расширены на списки)
    [Serializable]
    public class SkillEffect
    {
        [Tooltip("Список положительных эффектов (баффов)")]
        public List<BuffEffect> buffs;
    
        [Tooltip("Список отрицательных эффектов (дебаффов)")]
        public List<DebuffEffect> debuffs;
    }

    [Serializable]
    public class BuffEffect
    {
        [Tooltip("Тип баффа")]
        public BuffEffectType buffEffectType;
    
        [Tooltip("Значение эффекта (множитель для статов)")]
        [Range(0.1f, 5f)]
        public float value;
    
        [Tooltip("Длительность в ходах (0 — постоянный, >0 — временный)")]
        [Min(0)]
        public int duration;
    }

    [Serializable]
    public class DebuffEffect
    {
        [Tooltip("Тип дебаффа")]
        public DebuffType debuffEffectType;
    
        [Tooltip("Значение эффекта (множитель для статов или абсолютный урон для DoT)")]
        [Range(0.1f, 5f)]
        public float value;
    
        [Tooltip("Длительность в ходах (0 — постоянный, >0 — временный)")]
        [Min(0)]
        public int duration;
    }


    public enum BuffEffectType
    {
        HpBoost,          
        AttackBoost,     
        DefenseBoost,     
        SpeedBoost,      
        Shield,           // Щит (абсолютный или % от HP)
        RankUp,           // Повышение ранга карт (на 1-2 уровня, макс)
        Cleanse           // Очистка дебаффов (удаление по типу)
    }
    
    public enum DebuffType
    {
        Poison,           // Отравление (DoT)
        Bleed,            // Кровотечение (DoT)
        BaseStatsReduction, // Снижение базовых статов
        SubStatsReduction,  // Снижение субстатов
        SilenceSingleTarget, // Запрет навыков (общий)
        SilenceArea,      // Запрет для группы
        DefenseDown,      // Снижение защиты
        AttackDown,       // Снижение атаки
        Stun,             // Стан (пропуск хода)
        Freeze,           // Заморозка (аналог стана + замедление?)
        MeleeSilence,     // Запрет ближнего боя
        RangeSilence      // Запрет дальнего боя
    }
}