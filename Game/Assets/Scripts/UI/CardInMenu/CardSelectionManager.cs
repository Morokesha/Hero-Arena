using System.Collections.Generic;
using System.Linq;
using Core.Heroes;
using Services.SaveLoadServices;
using Services.StaticDataServices;
using UI.CardUI;
using UI.Interaction;
using UnityEngine;

namespace UI.CardInMenu
{
    public class CardSelectionManager : MonoBehaviour
    {
        private ISaveLoadService _saveLoadService;
        private IStaticDataService _staticDataService;
        private HeroSelectionWindow _heroSelectionWindow;
        private HeroInfoPanel _heroInfoPanel;
        private SquadSlotCard _selectedSlot;
        private HeroCardInteract _selectedHero;
        
        private List<SquadSlotCard> _squadSlots;
        
        private readonly Dictionary<SquadSlotCard, string> _assignedHeroes = new();
        private readonly HashSet<string> _assignedHeroIds = new();

        private int _slotIndex;
        
        private string _cardId;

        public void Construct(ISaveLoadService saveLoadService, IStaticDataService staticDataService,
            HeroSelectionWindow heroSelectionWindow, List<SquadSlotCard> cardSlots, HeroInfoPanel heroInfoPanel)
        {
            _saveLoadService = saveLoadService;
            _staticDataService = staticDataService;
            _heroSelectionWindow = heroSelectionWindow;
            _squadSlots = cardSlots;
            _heroInfoPanel = heroInfoPanel;
            
            _heroSelectionWindow.OnPurificationSelected += HandlePurificationSelected;
            
            foreach (var slot in _squadSlots.Where(slot => slot != null))
                slot.SetSlotIndex(_squadSlots.IndexOf(slot));
        }
        
        public void SelectSlot(SquadSlotCard slot)
        {
            if (_selectedSlot != null && _selectedSlot != slot)
                _selectedSlot.DeselectCard();

            _selectedSlot = slot;
            
            bool purification = _assignedHeroes.ContainsKey(slot);
        
            _heroSelectionWindow.UpdateHeroList(purification);
            _heroSelectionWindow.FilterAssignedHeroes(_assignedHeroIds);
        }
        
        public void SelectHero(HeroCardInteract hero, HeroData data)
        {
            if (_selectedHero != null && _selectedHero != hero)
                _selectedHero.DeselectCard();
        
            _selectedHero = hero;
            _heroInfoPanel.ShowHeroInfo(data);

            if (_selectedSlot == null)
            {
                Debug.LogWarning("Сначала выберите слот для назначения героя");
                return;
            }

            if (_assignedHeroIds.Contains(data.heroId))
            {
                Debug.LogWarning("Этот герой уже назначен в отряд");
                return;
            }

            AssignHeroToSlot(_selectedSlot, data);
            
            _selectedSlot.DeselectCard();
            _selectedSlot = null;
            _selectedHero = null;
        }

        public void LoadAssignedSquad()
        {
            var savedSquad = _saveLoadService.GetProgress().GetSavedSquad();
            ApplyAssignedSquad(savedSquad);
        }
        
        private Dictionary<int, string> GetAssignedSquad()
        {
            var squad = new Dictionary<int, string>();
            
            foreach (var kvp in _assignedHeroes)
            {
                int slotIndex = kvp.Key.GetSlotIndex();
                string heroId = kvp.Value;
                squad[slotIndex] = heroId;
            }
        
            return squad;
        }

        private void ApplyAssignedSquad(Dictionary<int, string> savedSquad)
        {
            _assignedHeroes.Clear();
            _assignedHeroIds.Clear();

            foreach (var slot in _squadSlots)
            {
                if (slot == null)
                    continue;

                int slotIndex = slot.GetSlotIndex();
                
                if (savedSquad.TryGetValue(slotIndex, out string cardId))
                {
                    HeroData heroData = _staticDataService.GetHeroById(cardId);

                    if (heroData != null)
                        AssignHeroToSlot(slot, heroData);
                    else
                        slot.ClearSlot();
                }
            }

            foreach (var slot in savedSquad)
                Debug.LogWarning("Номер слота " + slot.Key + " // Ключ слота " + slot.Value);
            
            _heroSelectionWindow.UpdateHeroList(false);
            _heroSelectionWindow.FilterAssignedHeroes(_assignedHeroIds);
        }

        private void HandlePurificationSelected()
        {
            if (_selectedSlot != null)
            {
                _selectedSlot.ClearSlot();
                
                if (_assignedHeroes.TryGetValue(_selectedSlot, out string heroId))
                {
                    _assignedHeroes.Remove(_selectedSlot);
                    _assignedHeroIds.Remove(heroId);
                    
                    _saveLoadService.GetProgress().SetSelectedSquad(GetAssignedSquad());
                    _saveLoadService.Save();
                }
                
                _heroSelectionWindow.UpdateHeroList(false);
                _heroSelectionWindow.FilterAssignedHeroes(_assignedHeroIds);
                
                _selectedSlot = null;
            }
        }
        
        private void AssignHeroToSlot(SquadSlotCard slot, HeroData heroData)
        {
            if (slot == null || heroData == null)
                return;
            
            if (_assignedHeroes.TryGetValue(slot, out string oldHeroId))
                _assignedHeroIds.Remove(oldHeroId);
            
            _assignedHeroes[slot] = heroData.heroId;
            _assignedHeroIds.Add(heroData.heroId);
            
            slot.AssignedHeroInSlot(heroData);
            
            _heroSelectionWindow.UpdateHeroList(false);
            _heroSelectionWindow.FilterAssignedHeroes(_assignedHeroIds);
            
            _saveLoadService.GetProgress().SetSelectedSquad(GetAssignedSquad());
            _saveLoadService.Save();

            foreach (var squad in _saveLoadService.GetProgress().GetSavedSquad())
                Debug.Log("Номер слота " + squad.Key  + "           " + " кто в слоте айди " + squad.Value);
        }

        private void OnDestroy() =>
            _heroSelectionWindow.OnPurificationSelected -= HandlePurificationSelected;
    }
}