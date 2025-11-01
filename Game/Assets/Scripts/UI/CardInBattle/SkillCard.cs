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
    public Hero owner; // Владелец героя
    public Skill skill; // Навык
    public SkillRarity rarity; // Bronze, Silver, Gold
    public Image icon; // Иконка навыка
    public Text rarityText; // Текст рарности (B/S/G)
    public int level = 1; // Уровень для мержа (1 для Bronze, 2 для Silver, 3 для Gold)

    public RectTransform rectTransform;
    private Canvas _canvas;
    private Vector2 _originalPos;
    private SkillCardManager _skillCardManager;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        _canvas = GetComponentInParent<Canvas>();
        _skillCardManager = FindObjectOfType<SkillCardManager>(); // Или инжектируй через DI
        UpdateDisplay();
    }

    public void UpdateDisplay()
    {
        icon.sprite = skill.skillIcon; 
        rarityText.text = rarity.ToString()[0].ToString(); // Bronse/Silver/Gold
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
        return owner == other.owner && skill == other.skill && rarity == other.rarity && rarity != SkillRarity.Gold;
    }

    // Клик для применения (если не drag)
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!EventSystem.current.IsPointerOverGameObject()) return; // Игнорируем если drag
        _skillCardManager.ApplySkill(this);
    }
}
}