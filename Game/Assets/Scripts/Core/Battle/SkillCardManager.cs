using System;
using UnityEngine;
using System.Collections.Generic;
using Core.Heroes;
using Core.Heroes.Skills;
using DG.Tweening;
using Services.FactoryServices;
using UI.CardInBattle;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Core.Battle
{

public class SkillCardManager : MonoBehaviour
{
    [SerializeField]
    private SkillCard skillCardPf;
    [SerializeField] 
    private RectTransform handContainer;
    [SerializeField]
    private Vector2 spawnStartPos = new Vector2(0, -500);
    [SerializeField] 
    private float spawnDuration = 0.25f;
    [SerializeField]
    private float spreadDistance = 100f; // Расстояние между картами
    
    private IFactoryUIService _factoryUI;
    private BattleSpawner _battleSpawner;
    private SkillExecutor _skillExecutor;
    private TargetSelector _targetSelector;

    private List<SkillCard> _hand = new List<SkillCard>();
    private List<Hero> _allPlayerHero;
    private List<Hero> _allAiHero;
    
    private int _maxCards = 8; // Зависит от героев

    private void Awake()
    {
        _allPlayerHero = new List<Hero>();
        _allAiHero = new List<Hero>();
    }

    public void Construct(IFactoryUIService factoryUIService,BattleSpawner battleSpawner, SkillExecutor skillExecutor,
        TargetSelector targetSelector)
    {
        _factoryUI = factoryUIService;
        _battleSpawner = battleSpawner;
        _skillExecutor = skillExecutor;
        _targetSelector = targetSelector;

        _battleSpawner.OnHeroesSpawned += HeroChanged;
        
        SpawnInitialCards();
    }

    private void HeroChanged(List<Hero> playerHeroes, List<Hero> aiHeroes)
    {
        _allPlayerHero = playerHeroes;
        _allAiHero = aiHeroes;
    }

    private void SpawnInitialCards()
    {
        _hand.Clear();
        
        foreach (var hero in _allPlayerHero)
        {
            Debug.Log(hero.name);
            
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
        int level = (int)rarity + 1; // Bronze=1, Silver=2, Gold=3;
        SkillCard card = _factoryUI.CreateSkillCard(handContainer);
        card.Construct(this,hero, skill, level);
        
        _hand.Add(card);

        // Анимация: от центра-низа к позиции в руке
        card.GetRectTransform().anchoredPosition = spawnStartPos;
        card.GetRectTransform().DOAnchorPos(CalculateCardPosition(_hand.Count - 1), spawnDuration).
            SetEase(Ease.OutBack);
    }

    // Расположить карты в руке (с spread)
    private void ArrangeCards()
    {
        for (int i = 0; i < _hand.Count; i++)
        {
            _hand[i].GetRectTransform().DOAnchorPos(CalculateCardPosition(i), 0.3f);
        }
    }

    private Vector2 CalculateCardPosition(int index)
    {
        float totalWidth = (_hand.Count - 1) * spreadDistance;
        float startX = -totalWidth / 2;
        return new Vector2(startX + index * spreadDistance, 0);
    }

    // Мерж: удалить 2, добавить 1 upgraded
    public void MergeCards(SkillCard card1, SkillCard card2)
    {
        if (card1.GetRarity() == SkillRarity.Gold)
            return; // Gold не ранк апаются

        // +1 ОД герою
        card1.GetOwner().ultimatePoints = Mathf.Min(card1.GetOwner().ultimatePoints + 1, 5);

        // Удалить карты
        _hand.Remove(card1);
        _hand.Remove(card2);
        
        Destroy(card1.gameObject);
        Destroy(card2.gameObject);

        // Добавить upgraded
        SkillRarity newRarity = card1.GetRarity() == SkillRarity.Bronze ? SkillRarity.Silver : SkillRarity.Gold;
        SpawnCard(card1.GetOwner(), card1.GetSkill(), newRarity);

        // Докинуть рандомные, если нужно
        FillHand();
    }

    // Swap: поменять местами и +1 ОД
    public void SwapCards(SkillCard card1, SkillCard card2)
    {
        card1.GetOwner().ultimatePoints = Mathf.Min(card1.GetOwner().ultimatePoints + 1, 5);

        // Поменять позиции в списке и на экране
        int index1 = _hand.IndexOf(card1);
        int index2 = _hand.IndexOf(card2);
        _hand[index1] = card2;
        _hand[index2] = card1;

        // Анимировать swap
        card1.GetRectTransform().DOAnchorPos(CalculateCardPosition(index2), 0.2f);
        card2.GetRectTransform().DOAnchorPos(CalculateCardPosition(index1), 0.2f);
    }

    // Заполнить руку рандомными картами до макс
    private void FillHand()
    {
        while (_hand.Count < _maxCards)
        {
            var randomHero = _battleSpawner.GetRandomLivingHero(_allPlayerHero);
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
        _maxCards = livingHeroesCount switch
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
            
            _hand.Remove(card);
            Destroy(card.gameObject);
            FillHand();
    }

    private void OnDestroy()
    {
        _battleSpawner.OnHeroesSpawned -= HeroChanged;
    }
}
}