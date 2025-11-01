using System;
using Services.FactoryServices;
using Services.SaveLoadServices;
using Services.StaticDataServices;
using UI.CardInMenu;
using UI.CardUI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI
{
    public class MenuWindowController : MonoBehaviour
    {
        [SerializeField] 
        private RectTransform uiCanvas;
        [SerializeField]
        private CardSelectionManager cardSelectionManager;

        private IFactoryUIService _factoryUIService;
        private ISaveLoadService _saveLoadService;
        private IStaticDataService _staticDataService;
        
        private HeroSelectionWindow _heroSelectionWindow;
        private SquadSelectionMenu _squadSelectionMenu;
        private HeroInfoPanel _heroInfoPanel;


        public void Construct(IFactoryUIService factoryUIService, ISaveLoadService saveLoadService, IStaticDataService 
            staticDataService)
        {
            _factoryUIService = factoryUIService;
            _saveLoadService = saveLoadService;
            _staticDataService = staticDataService;
        }

        public void ShowHeroSelectionWindow()
        {
            _heroSelectionWindow = _factoryUIService.CreateHeroSelectionWindow(uiCanvas, cardSelectionManager);
            _heroInfoPanel = _heroSelectionWindow.GetComponentInChildren<HeroInfoPanel>();
            _squadSelectionMenu = _heroSelectionWindow.GetComponentInChildren<SquadSelectionMenu>();
            _squadSelectionMenu.Construct(cardSelectionManager);
            
            cardSelectionManager.Construct(_saveLoadService, _staticDataService, _heroSelectionWindow, 
                _squadSelectionMenu.GetCardSlotsList(), _heroInfoPanel);
            cardSelectionManager.LoadAssignedSquad();
            
            _heroSelectionWindow.OnPlayBtnActivated += LoadGameScene;
        }

        private void LoadGameScene() => SceneManager.LoadSceneAsync(1);

        private void OnDestroy() => _heroSelectionWindow.OnPlayBtnActivated -= LoadGameScene;
    }
}