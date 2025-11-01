using UnityEngine;
using UnityEngine.Serialization;

namespace Core.Heroes.Skills
{ 
    [CreateAssetMenu(fileName = "SkillIconConfig", menuName = "Skills/SkillIconConfig", order = 1)]
    public class SkillEffectIconConfig : ScriptableObject
    {
        [Header("Effect Category Icons")]
        [Tooltip("Иконка для DamageSingleTarget")]
        public Sprite meleeAttackDamageIcon;
    
        [Tooltip("Иконка для RangeDamage")]
        public Sprite rangeDamageIcon;
    
        [Tooltip("Иконка для Debuff")]
        public Sprite debuffIcon;
    
        [Tooltip("Иконка для Buff")]
        public Sprite buffIcon;
    
        [Tooltip("Иконка для HealCleanse")]
        public Sprite healCleanseIcon;
        
        [Tooltip("Иконка для Ultimate")]
        public Sprite ultimateIcon;
    }
}