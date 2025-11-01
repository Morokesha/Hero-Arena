using System.Collections.Generic;

namespace Core.Heroes.Skills
{
    public class SkillExecutor
    {
        // Метод: выполнить навык (вызывается из UI или BattleManager)
        public void ExecuteSkill(Hero caster, Skill skill, SkillRarity skillRarity, List<Hero> targets)
        {
            if (caster == null || skill == null || targets == null || targets.Count == 0)
                return;

            var damageCalculator = caster.GetComponent<DamageCalculator>();
            if (damageCalculator == null) return;

            var skillLevelData = skill.GetLevelData(skillRarity);
            if (skillLevelData == null) return;
            
            if (skill.targetAll)
            {
                foreach (var target in targets)
                    damageCalculator.ApplySkillDamage(target, skill, skillLevelData);
            }
            else
                damageCalculator.ApplySkillDamage(targets[0], skill, skillLevelData);

        }

        // Перегрузка: для одной цели
        public void ExecuteSkill(Hero caster, Skill skill, SkillRarity skillRarity, Hero target) =>
            ExecuteSkill(caster, skill, skillRarity, new List<Hero> { target });
    }
}