using Core.Heroes;
using TMPro;
using UnityEngine;

namespace UI
{
    public class HeroInfoPanel : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI heroName;
        [SerializeField] private TextMeshProUGUI heroBattlePower;
        [SerializeField] private TextMeshProUGUI heroHealth;
        [SerializeField] private TextMeshProUGUI heroAttack;
        [SerializeField] private TextMeshProUGUI heroDefence;

        private int _battlePower;
        
        public void ShowHeroInfo(HeroData heroData)
        {
            _battlePower = heroData.heroBaseStats.baseHP / 2 + heroData.heroBaseStats.baseAttack + 
                           heroData.heroBaseStats.baseDefense;
            
            heroName.SetText(heroData.heroCardData.heroName);
            heroBattlePower.SetText(_battlePower.ToString());
            heroHealth.SetText(heroData.heroBaseStats.baseHP.ToString());
            heroAttack.SetText(heroData.heroBaseStats.baseAttack.ToString());
            heroDefence.SetText(heroData.heroBaseStats.baseDefense.ToString());
        }
    }
}