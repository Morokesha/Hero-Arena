using System;
using System.Collections.Generic;
using System.Linq;
using Core.Heroes;
using Services.AssetServices;
using Services.SaveLoadServices;
using VContainer;

namespace Services.StaticDataServices
{
    public class StaticDataService : IStaticDataService
    {
        private readonly IAssetProvider _assetProvider;
        private readonly ISaveLoadService _saveLoadService;
        
        private Dictionary<string, HeroData> _heroes;
        private HeroData[] _allHeroes;

        [Inject]
        public StaticDataService(IAssetProvider assetProvider, ISaveLoadService saveLoadService)
        {
            _assetProvider = assetProvider;
            _saveLoadService = saveLoadService;
            
            LoadData();
        }

        private void LoadData()
        {
            _saveLoadService.Load();
            
            _allHeroes = _assetProvider.LoadAll<HeroData>(AssetPath.HeroesDataPath);
            _heroes = new Dictionary<string, HeroData>();

            foreach (var hero in _allHeroes)
                _heroes[hero.heroId] = hero;
        }

        public IReadOnlyCollection<HeroData> GetAllHeroes() => _heroes.Values;
        public HeroData GetHeroById(string cardId) => _allHeroes.FirstOrDefault(heroData => heroData.heroId == cardId);

        public List<HeroData> GetAllHeroesSortedByCardName()
        {
            return _heroes.Values
                .OrderBy(hero => hero.heroCardData.heroName, StringComparer.OrdinalIgnoreCase).ToList();
        }
    }
}