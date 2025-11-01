using System;
using Unity.Collections;
using UnityEngine;

namespace Core.Heroes
{
    [Serializable]
    public class HeroCardData
    {
        public Sprite heroAvatar;
        public Sprite heroElement;

        public HeroElement element;

        [SerializeField, ReadOnly] public Color colorElement; 

        public string heroName;

        public void ChoiceColor()
        {
            switch (element)
            {
                case HeroElement.Fire:
                    colorElement = Color.red;
                    break;
                case HeroElement.Ice:
                    colorElement = Color.cyan;
                    break;
                case HeroElement.Nature:
                    colorElement = Color.green;
                    break;
                default:
                    colorElement = Color.white;
                    break;
            }
        }
    }

    public enum HeroElement
    {
        Fire,
        Ice,
        Nature
    }
}