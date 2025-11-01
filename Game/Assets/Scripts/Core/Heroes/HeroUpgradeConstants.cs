using System;

namespace Core.Heroes
{
    
    public static class HeroUpgradeConstants
    {
        public const float LevelCoeff = 0.1f; // +10% за уровень (для HP, Attack, Defense)
        public const float StarBonus = 5f; // +5 единиц за звезду (к HP, Attack, Defense)
        public const float RedStarBonus = 10f; // +10 единиц за красную звезду
    }
}