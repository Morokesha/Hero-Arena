using System.Collections.Generic;
using Core.Heroes;
using UnityEngine;

namespace Core.Battle
{
    [CreateAssetMenu(fileName = "LevelConfig", menuName = "Level/Level Config", order = 0)]
    public class LevelConfig : ScriptableObject
    {
        [Header("Арена")]
        public GameObject arenaPrefab;

        [Header("ИИ-противники")]
        [Tooltip("Список данных прокачки для ИИ-героев (по порядку позиций, до 4)")]
        public List<HeroUpgradeData> aiHeroes = new(); 
        
        private void OnValidate()
        {
            if (aiHeroes.Count > 4) 
                aiHeroes = aiHeroes.GetRange(0, 4);
        }
    }
}