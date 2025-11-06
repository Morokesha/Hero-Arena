using System;
using UnityEngine;
using System.Collections.Generic;
using Core.Heroes;
using Core.Heroes.Skills;
using DG.Tweening;
using Services.FactoryServices;
using UI.CardInBattle;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

namespace Core.Battle
{
    public class SkillCardManager : MonoBehaviour
    {
        [SerializeField] private SkillCard skillCardPf;
        [SerializeField] private RectTransform handContainer; // Контейнер 750x200, якоря в правом углу, RectMask2D
        [SerializeField] private float spawnOffsetX = -480f; // Спавн слева за контейнером
        [SerializeField] private float spawnDuration = 0.25f;
        [SerializeField] private float spreadDistance = 100f; // Расстояние между картами (шаг -100f от правого края)
        [SerializeField] private float cardScaleOnSelect = 1.2f; // Масштаб при выборе
        [SerializeField] private float dragThreshold = 10f; // Порог для начала драга
        

        private IFactoryUIService _factoryUI;
        private BattleSpawner _battleSpawner;
        private SkillExecutor _skillExecutor;
        private TargetSelector _targetSelector;

        private List<SkillCard> _hand = new List<SkillCard>();
        private List<Hero> _allPlayerHero;
        private List<Hero> _allAiHero;

        private int _maxCards = 8; // Зависит от героев

        public void Construct(IFactoryUIService factoryUIService, BattleSpawner battleSpawner,
            SkillExecutor skillExecutor, TargetSelector targetSelector)
        {
            _factoryUI = factoryUIService;
            _battleSpawner = battleSpawner;
            _skillExecutor = skillExecutor;
            _targetSelector = targetSelector;

            _battleSpawner.OnHeroesSpawned += OnHeroesSpawnedHandler;
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

        private void OnHeroesSpawnedHandler(List<Hero> playerHeroes, List<Hero> aiHeroes)
        {
            Debug.Log($"SkillCardManager OnHeroesSpawnedHandler: Received {playerHeroes?.Count ?? 0} player heroes");
            InitializeCards(playerHeroes, aiHeroes);
        }

        private void InitializeCards(List<Hero> playerHeroes, List<Hero> aiHeroes)
        {
            Debug.Log($"InitializeCards called with playerHeroes.Count = {playerHeroes?.Count ?? 0}");

            if (playerHeroes == null || playerHeroes.Count == 0)
            {
                Debug.LogError("InitializeCards: playerHeroes is null or empty! Cards won't spawn.");
                return;
            }

            _allPlayerHero = new List<Hero>(playerHeroes);
            _allAiHero = new List<Hero>(aiHeroes);

            Debug.Log($"InitializeCards: _allPlayerHero now has {_allPlayerHero.Count} heroes. " +
                      $"Calling SpawnInitialCards.");

            SpawnInitialCards();
        }

        private void SpawnInitialCards()
        {
            _hand.Clear();

            foreach (var hero in _allPlayerHero)
            {
                Debug.Log("Спавн карты навыка, имя героя: - " + hero.name);

                var skillData = hero.GetHeroSkillData(); // Получаем данные навыков героя
                // Спавним по одной Bronze-карте на каждый активный навык (2 шт)
                SpawnCard(hero, skillData.activeSkill1, SkillRarity.Bronze);
                SpawnCard(hero, skillData.activeSkill2, SkillRarity.Bronze);
            }

            ArrangeCards();
        }

        // Спавн одной карты с анимацией (спавн слева за контейнером, летит к позиции)
        private void SpawnCard(Hero hero, Skill skill, SkillRarity rarity)
        {
            int level = (int)rarity + 1; // Bronze=1, Silver=2, Gold=3;
            SkillCard card = _factoryUI.CreateSkillCard(handContainer);
            card.GetRectTransform().anchoredPosition = new Vector2(spawnOffsetX, 0f); // Спавн на (-480f, 0f)
            card.Construct(this, hero, skill, level);

            // Передать параметры drag&drop из менеджера
            card.CardScaleOnSelect = cardScaleOnSelect;
            card.DragThreshold = dragThreshold;

            _hand.Add(card);

            // Анимация: от спавн-позиции к финальной позиции в руке
            int index = _hand.Count - 1;
            card.GetRectTransform().DOAnchorPos(CalculateCardPosition(index), spawnDuration)
                .SetEase(Ease.OutBack);
        }

        // Расположить карты в руке (с фиксированным шагом от правого края)
        private void ArrangeCards()
        {
            for (int i = 0; i < _hand.Count; i++)
            {
                _hand[i].GetRectTransform().DOAnchorPos(CalculateCardPosition(i), 0.3f);
            }
        }

        // Позиция карты: первая 350f (от правого края?), вторая 250f, etc. (шаг -100f)
        // Предполагаем, что якорь контейнера в правом углу, так что 350f - это близко к правому краю
        private Vector2 CalculateCardPosition(int index)
        {
            float startX = 350f; // Позиция первой карты
            return new Vector2(startX - index * spreadDistance, 0f);
        }

        // Мерж: удалить 2, добавить 1 upgraded
        public void MergeCards(SkillCard card1, SkillCard card2)
        {
            if (card1.GetRarity() == SkillRarity.Gold || card2.GetRarity() == SkillRarity.Gold)
                return; // Gold не мержатся

            if (card1.GetOwner() != card2.GetOwner() || card1.GetSkill() != card2.GetSkill() ||
                card1.GetRarity() != card2.GetRarity())
                return; // Только одинаковые герой, навык, ранг

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
            ArrangeCards(); // Перерасположить после мержа
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

        private void OnDestroy()
        {
            if (_battleSpawner != null)
                _battleSpawner.OnHeroesSpawned -= OnHeroesSpawnedHandler;
        }
    }
}