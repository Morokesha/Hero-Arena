using Core.Heroes.Skills;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Serialization;

namespace Core.Heroes
{
    [CreateAssetMenu(fileName = "HeroData", menuName = "Hero Data/Hero Data", order = 0)]
    public class HeroData : ScriptableObject
    {
        [Header("HERO ID")]
        public string heroId;

        [Space(5)]
        public Hero heroPf;

        [Space(2)]
        [Tooltip("Общие анимации")] public HeroAnimationSet heroAnimationSet;

        [Space(5)]
        [Header("Skill Data")] public HeroSkillsData heroSkillsData;

        public HeroBaseStats heroBaseStats;

        [Space(5)] 
        [Header("UI Data")] public HeroCardData heroCardData;

        public HeroBaseStats GetFinalStats(HeroUpgradeData upgradeData) =>
            upgradeData.CalculateFinalStats(heroBaseStats);
        
        private void OnValidate()
        {
            if (heroCardData != null)
            {
                heroCardData.ChoiceColor();

                if (string.IsNullOrEmpty(heroId) && Application.isEditor && !Application.isPlaying) 
                    heroId = System.Guid.NewGuid().ToString();
            }
        }
    }
}