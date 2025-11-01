using System;
using System.Collections.Generic;
using System.Linq;
using Core.Heroes;
using UnityEngine;

namespace Services.SaveLoadServices
{
    [Serializable]
    public class GameProgress
    {
        [SerializeField]
        private List<SquadEntry> assignedSquad = new();
        [SerializeField] 
        private Dictionary<string, HeroUpgradeData> HeroUpgrades = new();

        public void SetSelectedSquad(Dictionary<int, string> squad) =>
            assignedSquad = squad.Select(kvp => new SquadEntry { slotIndex = kvp.Key, heroId = kvp.Value }).
                ToList();

        public Dictionary<int, string> GetSavedSquad() => 
            assignedSquad.ToDictionary(e => e.slotIndex, e => e.heroId);
        
        public HeroUpgradeData GetHeroUpgrade(string heroId)
        {
            if (HeroUpgrades.TryGetValue(heroId, out var upgrade))
                return upgrade;
            
            var defaultUpgrade = new HeroUpgradeData { heroId = heroId };
            HeroUpgrades[heroId] = defaultUpgrade;
            return defaultUpgrade;
        }
        
        public void SaveHeroUpgrade(HeroUpgradeData upgrade) =>
            HeroUpgrades[upgrade.heroId] = upgrade;
    }
    
    [Serializable]
    public class SquadEntry
    {
        public int slotIndex;
        public string heroId;
    }
}