using System.Collections.Generic;
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
        [SerializeField] private Image background; // Цвет редкости карты (B/S/G)
        [SerializeField] private RectTransform rectTransform;

        private Hero _owner;
        private Skill _skill;
        private SkillRarity _rarity; // Bronze, Silver, Gold
        private int _level = 1; // Уровень для мержа (1 для Bronze, 2 для Silver, 3 для Gold)

        private Canvas _canvas;
        private Vector2 _originalPos;
        private SkillCardManager _skillCardManager;

        // Параметры для drag&drop (передаются из менеджера)
        public float CardScaleOnSelect { get; set; } = 1.2f;
        public float DragThreshold { get; set; } = 10f;

        private bool _isDragging;
        private Vector2 _dragStartPos;
        private Vector3 _cardStartAnchoredPos;

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
            _rarity = (SkillRarity)(level - 1); // 1=Bronze, 2=Silver, 3=Gold
            UpdateDisplay();
        }

        public void SetOwner(Hero hero) => _owner = hero;

        public void UpdateDisplay()
        {
            icon.sprite = _skill.skillIcon;

            switch (_rarity)
            {
                case SkillRarity.Bronze:
                    background.color = ColorUtility.TryParseHtmlString("#8B4513", out Color bronze)
                        ? bronze
                        : Color.saddleBrown;
                    break;
                case SkillRarity.Silver:
                    background.color = ColorUtility.TryParseHtmlString("#C0C0C0", out Color silver)
                        ? silver
                        : Color.gray;
                    break;
                case SkillRarity.Gold:
                    background.color = ColorUtility.TryParseHtmlString("#FFD700", out Color gold) ? gold : Color.yellow;
                    break;
                default:
                    background.color = Color.white;
                    break;
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _originalPos = rectTransform.anchoredPosition;
            _cardStartAnchoredPos = rectTransform.anchoredPosition;
            _dragStartPos = eventData.position;
            _isDragging = false;

            // Увеличить масштаб
            rectTransform.DOScale(CardScaleOnSelect, 0.1f);

            // Поднять выше (чтобы была поверх других)
            transform.SetAsLastSibling();
        }

        public void OnDrag(PointerEventData eventData)
        {
            // Проверить порог для начала драга
            if (!_isDragging && Vector2.Distance(eventData.position, _dragStartPos) > DragThreshold)
            {
                _isDragging = true;
            }

            if (_isDragging)
            {
                // Только горизонтальный drag: вычислить delta X и обновить позицию
                float deltaX = eventData.delta.x / _canvas.scaleFactor;
                float newX = _cardStartAnchoredPos.x + deltaX;

                rectTransform.anchoredPosition = new Vector2(newX, _cardStartAnchoredPos.y); // Y не меняется
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            // Вернуть масштаб
            rectTransform.DOScale(1f, 0.1f);

            if (_isDragging)
            {
                // Проверить, над какой картой отпустили
                SkillCard targetCard = GetCardUnderPointer(eventData);
                if (targetCard != null && targetCard != this)
                {
                    // Попытка мерж или swap
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

            _isDragging = false;
        }

        // Получить карту под указателем
        private SkillCard GetCardUnderPointer(PointerEventData eventData)
        {
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            foreach (var result in results)
            {
                SkillCard card = result.gameObject.GetComponent<SkillCard>();
                if (card != null && card != this)
                    return card;
            }

            return null;
        }

        private bool CanMergeWith(SkillCard other)
        {
            return _owner == other._owner && _skill == other._skill && _rarity == other._rarity &&
                   _rarity != SkillRarity.Gold;
        }

        // Клик для применения (если не drag)
        public void OnPointerClick(PointerEventData eventData)
        {
            if (_isDragging) return; // Если был drag, не применять
            _skillCardManager.ApplySkill(this);
        }

        public RectTransform GetRectTransform() => rectTransform;

        public SkillRarity GetRarity() => _rarity;

        public Hero GetOwner() => _owner;

        public Skill GetSkill() => _skill;
    }
}