using System.Collections.Generic;
using Core.Heroes;

namespace Services.StaticDataServices
{
    public interface IStaticDataService
    {
        public IReadOnlyCollection<HeroData> GetAllHeroes();
        public HeroData GetHeroById(string cardId);
        public List<HeroData> GetAllHeroesSortedByCardName();
    }
}