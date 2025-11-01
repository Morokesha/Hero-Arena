using UnityEngine;
using System.Collections.Generic;
using Core.Heroes;
using Core.Heroes.Skills;
using DG.Tweening;
using UI.CardInBattle;

namespace Core.Battle
{

public class SkillCardManager : MonoBehaviour
{
    public GameObject cardPrefab; // Prefab SkillCard
    public Transform handContainer; // UI-контейнер для карт (HorizontalLayoutGroup)
    public Vector2 spawnStartPos = new Vector2(0, -500); // Центр-низ
    public Vector2 spawnEndPos = new Vector2(400, -400); // Правый-низний угол
    public float spawnDuration = 0.5f;
    public float spreadDistance = 100f; // Расстояние между картами

    private List<SkillCard> hand = new List<SkillCard>();
    private int maxCards = 8; // Зависит от героев
    private BattleController _battleController; // Ссылка для отслеживания героев
    private SkillExecutor _skillExecutor;
    private TargetSelector _targetSelector;

    public void Construct(BattleController battleController, SkillExecutor skillExecutor, TargetSelector targetSelector)
    {
        _battleController = battleController;
        _skillExecutor = skillExecutor;
        _targetSelector = targetSelector;
        
        SpawnInitialCards();
    }
    
    public void SpawnInitialCards()
    {
        hand.Clear();
    
        var allHeroes = _battleController.GetAllHeroesPlayer(); // Предполагаю, это твой метод
    
        foreach (var hero in allHeroes)
        {
            var skillData = hero.GetHeroSkillData(); // Получаем данные навыков героя
            // Спавним по одной Bronze-карте на каждый активный навык (2 шт)
            SpawnCard(hero, skillData.activeSkill1, SkillRarity.Bronze);
            SpawnCard(hero, skillData.activeSkill2, SkillRarity.Bronze);
        }
        ArrangeCards();
    }

    // Спавн одной карты с анимацией
    private void SpawnCard(Hero hero, Skill skill, SkillRarity rarity)
    {
        var cardObj = Instantiate(cardPrefab, handContainer);
        var card = cardObj.GetComponent<SkillCard>();
        card.owner = hero;
        card.skill = skill;
        card.rarity = rarity;
        card.level = (int)rarity + 1; // Bronze=1, Silver=2, Gold=3
        hand.Add(card);

        // Анимация: от центра-низа к позиции в руке
        card.rectTransform.anchoredPosition = spawnStartPos;
        card.rectTransform.DOAnchorPos(CalculateCardPosition(hand.Count - 1), spawnDuration).SetEase(Ease.OutBack);
    }

    // Расположить карты в руке (с spread)
    private void ArrangeCards()
    {
        for (int i = 0; i < hand.Count; i++)
        {
            hand[i].rectTransform.DOAnchorPos(CalculateCardPosition(i), 0.3f);
        }
    }

    private Vector2 CalculateCardPosition(int index)
    {
        float totalWidth = (hand.Count - 1) * spreadDistance;
        float startX = -totalWidth / 2;
        return new Vector2(startX + index * spreadDistance, 0);
    }

    // Мерж: удалить 2, добавить 1 upgraded
    public void MergeCards(SkillCard card1, SkillCard card2)
    {
        if (card1.rarity == SkillRarity.Gold) return; // Gold не мержатся

        // +1 ОД герою
        card1.owner.ultimatePoints = Mathf.Min(card1.owner.ultimatePoints + 1, 5);

        // Удалить карты
        hand.Remove(card1);
        hand.Remove(card2);
        Destroy(card1.gameObject);
        Destroy(card2.gameObject);

        // Добавить upgraded
        SkillRarity newRarity = card1.rarity == SkillRarity.Bronze ? SkillRarity.Silver : SkillRarity.Gold;
        SpawnCard(card1.owner, card1.skill, newRarity);

        // Докинуть рандомные, если нужно
        FillHand();
    }

    // Swap: поменять местами и +1 ОД
    public void SwapCards(SkillCard card1, SkillCard card2)
    {
        card1.owner.ultimatePoints = Mathf.Min(card1.owner.ultimatePoints + 1, 5);

        // Поменять позиции в списке и на экране
        int index1 = hand.IndexOf(card1);
        int index2 = hand.IndexOf(card2);
        hand[index1] = card2;
        hand[index2] = card1;

        // Анимировать swap
        card1.rectTransform.DOAnchorPos(CalculateCardPosition(index2), 0.2f);
        card2.rectTransform.DOAnchorPos(CalculateCardPosition(index1), 0.2f);
    }

    // Заполнить руку рандомными картами до макс
    private void FillHand()
    {
        while (hand.Count < maxCards)
        {
            var randomHero = _battleController.GetRandomLivingHero(_battleController.GetAllHeroesPlayer());
            var skillData = randomHero.GetHeroSkillData();
            // Выбираем случайно между activeSkill1 и activeSkill2
            int randomIndex = Random.Range(0, 2); // 0 или 1
            Skill randomSkill = randomIndex == 0 ? skillData.activeSkill1 : skillData.activeSkill2;
        
            SpawnCard(randomHero, randomSkill, SkillRarity.Bronze);
        }
        ArrangeCards();
    }

    // Обновить макс карт при смерти героя (вызывай из BattleController)
    public void UpdateMaxCards(int livingHeroesCount)
    {
        maxCards = livingHeroesCount switch
        {
            4 => 8,
            3 => 7,
            2 => 5,
            1 => 4,
            _ => 4
        };
        // Если карт больше макс — не удаляем, просто не добавляем новые
        FillHand();
    }

    // Применить навык (после клика по карте)
    public void ApplySkill(SkillCard card)
    {
        // Перейти в режим выбора целей
        //_targetSelector.StartTargeting(card.skill, card.owner, (targets) => {
      //      _skillExecutor.ExecuteSkill(card.owner, card.skill, card.rarity, targets);
            
            hand.Remove(card);
            Destroy(card.gameObject);
            FillHand();
    }
}
}