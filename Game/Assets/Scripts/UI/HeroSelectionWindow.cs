using System;
using System.Collections.Generic;
using Core.Heroes;
using Services.FactoryServices;
using Services.StaticDataServices;
using UI.CardInMenu;
using UI.CardUI;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class HeroSelectionWindow : MonoBehaviour
    {
        [SerializeField] private RectTransform heroCardContainer;
        [SerializeField] private HeroInfoPanel heroInfoPanel;
        [SerializeField] private Button playBtn;

        public event Action OnPlayBtnActivated;
         public event Action OnPurificationSelected;
         
         private IStaticDataService _staticDataService; 
         private IFactoryUIService _uiFactoryService; 
         private CardSelectionManager _cardSelectionManager;
         
         private List<HeroData> _allHeroes; 
         
         private List<Card> _allCards = new(); 
         private PurificationCard _purificationCard;

         public void Construct(IStaticDataService staticDataService, IFactoryUIService uiFactoryService,
             CardSelectionManager cardSelectionManager) 
         { 
             _staticDataService = staticDataService; 
             _uiFactoryService = uiFactoryService; 
             _cardSelectionManager = cardSelectionManager;
             
             _allHeroes = _staticDataService.GetAllHeroesSortedByCardName();

             CreateAllCards(); 
             UpdateHeroList(false);
             
             if (_allHeroes.Count > 0)
             {
                 SetDefaultHeroInfo(_allHeroes[0]);
             }
         }

         private void Start()
         {
             playBtn.onClick.AddListener(LoadGameScene);
         }

         private void LoadGameScene()
         {
             OnPlayBtnActivated?.Invoke();
         }
         
         public void UpdateHeroList(bool showPurification)
         {
             for (int i = 0; i < _allCards.Count; i++)
             {
                 bool isPurification = (i == 0);
                 _allCards[i].gameObject.SetActive(!isPurification || showPurification);
             }
         }
     
         public void FilterAssignedHeroes(HashSet<string> assignedHeroIds)
         {
             assignedHeroIds ??= new HashSet<string>();

             foreach (var card in _allCards)
             {
                 if (card is HeroCard heroCard)
                 {
                     bool isAssigned = assignedHeroIds.Contains(heroCard.GetHeroData().heroId);
                     heroCard.gameObject.SetActive(!isAssigned);
                 }
             }
         }

         private void SetDefaultHeroInfo(HeroData heroData) =>
             heroInfoPanel.ShowHeroInfo(heroData);
         
         private void CreateAllCards() 
         { 
             foreach (Transform child in heroCardContainer) 
                 Destroy(child.gameObject);
             _allCards.Clear();
             
             _purificationCard = _uiFactoryService.CreatePurificationCard(heroCardContainer);
             _purificationCard.OnCardClicked += OnPurificationClicked;
             _allCards.Add(_purificationCard);
             _purificationCard.gameObject.SetActive(false); 
             
             foreach (var heroData in _allHeroes)
             {
                 HeroCard heroCard = _uiFactoryService.CreateHeroCard(heroCardContainer, heroData, _cardSelectionManager);
                 _allCards.Add(heroCard);
                 heroCard.gameObject.SetActive(false);
             }
         }
         
         private void OnPurificationClicked() =>
             OnPurificationSelected?.Invoke();

         private void OnDestroy()
         {
             if (_purificationCard != null)
                 _purificationCard.OnCardClicked -= OnPurificationClicked;
             
             playBtn.onClick.RemoveListener(LoadGameScene);
         }
    }
}