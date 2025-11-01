using System;
using System.Collections.Generic;
using UI.CardInMenu;
using UI.CardUI;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class SquadSelectionMenu : MonoBehaviour
    {
        [SerializeField]
        private List<SquadSlotCard> slotList;
        
        private CardSelectionManager _cardSelectionManager;

        public void Construct(CardSelectionManager cardSelectionManager) 
        { 
            _cardSelectionManager = cardSelectionManager;
            
            for (int i = 0; i < slotList.Count; i++)
            {
                slotList[i].SetSelectionManager(_cardSelectionManager);
                slotList[i].SetSlotIndex(i);
            }
        }
        
        public List<SquadSlotCard> GetCardSlotsList() => slotList;
    }
}