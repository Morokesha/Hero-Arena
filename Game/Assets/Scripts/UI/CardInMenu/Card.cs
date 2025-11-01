using UnityEngine;
using UnityEngine.UI;

namespace UI.CardUI
{
    public abstract class Card : MonoBehaviour
    {
        [SerializeField]
        protected Button cardButton; 
        [SerializeField]
        protected Image cardImage;
        [SerializeField] 
        protected Image elementSprite;
        [SerializeField] 
        protected Image elementBackground;


        public virtual void Initialize(Sprite heroAvatar,Sprite element, Color color)
        {
            if (cardImage != null)
                cardImage.sprite = heroAvatar;
            if (element != null)
                elementSprite.sprite = element;
            if (elementBackground != null)
                elementBackground.color = color;
            
            cardButton.onClick.AddListener(OnCardClick);
        }
        
        public abstract void OnCardClick();
    }
}