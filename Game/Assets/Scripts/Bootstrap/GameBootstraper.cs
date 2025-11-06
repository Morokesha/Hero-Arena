using Services.FactoryServices;
using Services.SaveLoadServices;
using Services.StaticDataServices;
using UI;
using UnityEngine;
using VContainer;

namespace Bootstrap
{
    public class GameBootstrapper : MonoBehaviour
    {
        private IStaticDataService _staticDataService;
        private IFactoryUIService _factoryUIService;
        private ISaveLoadService _saveLoadService;
        
        private MenuWindowController _menuWindowController;

        [Inject]
        public void Construct(IStaticDataService staticDataService, IFactoryUIService factoryUIService, ISaveLoadService 
            saveLoadService, IObjectResolver resolver)
        {
            _staticDataService = staticDataService;
            _factoryUIService = factoryUIService;
            _saveLoadService = saveLoadService;
            
            resolver.TryResolve(out _menuWindowController);
        }

        private void Start()
        {
            if (_menuWindowController != null)
            {
                _menuWindowController.Construct(_factoryUIService, _saveLoadService, _staticDataService);
                _menuWindowController.ShowHeroSelectionWindow();
            }
        }
    }
}
