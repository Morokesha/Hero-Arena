using Core.Battle;
using Core.Heroes;
using Core.Heroes.Skills;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI.CardInBattle
{
    public class SkillCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
    {
        [SerializeField] private Image icon; // Иконка навыка
        [FormerlySerializedAs("rarityColor")] [SerializeField] private Image background; // Цвет редкости карты (B/S/G)
        [SerializeField] private RectTransform rectTransform;

        private Hero _owner;
        private Skill _skill;
        private SkillRarity _rarity; // Bronze, Silver, Gold

        private int _level = 1; // Уровень для мержа (1 для Bronze, 2 для Silver, 3 для Gold)

        private Canvas _canvas;
        private Vector2 _originalPos;
        private SkillCardManager _skillCardManager;

        void Start()
        {
            rectTransform = GetComponent<RectTransform>();
            _canvas = GetComponentInParent<Canvas>();

            UpdateDisplay();
        }

        public void Construct(SkillCardManager skillCardManager, Hero owner, Skill skill, int level)
        {
            _skillCardManager = skillCardManager;
            _owner = owner;
            _skill = skill;
            _level = level;
        }

        public void SetOwner(Hero hero) => _owner = hero;

        public void UpdateDisplay()
        {
            icon.sprite = _skill.skillIcon;


            switch (_rarity)
            {
                case SkillRarity.Bronze:
                        background.color = Color.saddleBrown;
                    break;
                case SkillRarity.Silver:
                    background.color = Color.silver;
                    break; 
                case SkillRarity.Gold:
                    background.color = Color.gold;
                    break; 
                default: background.color = Color.white; 
                    break;
            }
        }

        // Drag для swap/merge
        public void OnBeginDrag(PointerEventData eventData)
        {
            _originalPos = rectTransform.anchoredPosition;
            transform.SetAsLastSibling(); // Чтобы карта была сверху
        }

        public void OnDrag(PointerEventData eventData)
        {
            rectTransform.anchoredPosition += eventData.delta / _canvas.scaleFactor;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            // Проверяем, над какой картой отпустили
            var hit = eventData.pointerCurrentRaycast.gameObject;
            if (hit != null && hit.TryGetComponent<SkillCard>(out var targetCard))
            {
                if (CanMergeWith(targetCard))
                {
                    _skillCardManager.MergeCards(this, targetCard);
                }
                else
                {
                    _skillCardManager.SwapCards(this, targetCard);
                }
            }
            else
            {
                // Вернуть на место
                rectTransform.DOAnchorPos(_originalPos, 0.2f);
            }
        }

        private bool CanMergeWith(SkillCard other)
        {
            return _owner == other._owner && _skill == other._skill && _rarity == other._rarity &&
                   _rarity != SkillRarity.Gold;
        }

        // Клик для применения (если не drag)
        public void OnPointerClick(PointerEventData eventData)
        {
            if (!EventSystem.current.IsPointerOverGameObject()) return; // Игнорируем если drag
            _skillCardManager.ApplySkill(this);
        }
        
        public RectTransform GetRectTransform() => rectTransform;

        public SkillRarity GetRarity() => _rarity;

        public Hero GetOwner() => _owner;

        public Skill GetSkill() => _skill;
    }
}