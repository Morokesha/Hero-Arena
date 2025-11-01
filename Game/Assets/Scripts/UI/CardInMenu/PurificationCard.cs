using System;

namespace UI.CardUI
{
    public class PurificationCard : Card
    {
        public event Action OnCardClicked;

        private void OnEnable() => cardButton.onClick.AddListener(OnCardClick);
        private void OnDisable() => cardButton.onClick.RemoveListener(OnCardClick);

        public override void OnCardClick() =>
            OnCardClicked?.Invoke();
    }
}