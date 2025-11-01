using System.Collections.Generic;
using Core.Heroes;

namespace Services.StaticDataServices
{
    public class SquadData
    { 
        private const int MaxSquadSize = 4;
        
        private readonly List<HeroData> _selectedHeroes = new List<HeroData>();
        
        public IReadOnlyList<HeroData> SelectedHeroes => _selectedHeroes;
        
        public bool AddHero(HeroData heroData) 
        { 
            if (_selectedHeroes.Count >= MaxSquadSize || _selectedHeroes.Contains(heroData))
                return false;

            _selectedHeroes.Add(heroData);
            return true; 
        }
        
        public bool RemoveHero(HeroData heroData) 
        { 
            return _selectedHeroes.Remove(heroData); 
        }
    }
}