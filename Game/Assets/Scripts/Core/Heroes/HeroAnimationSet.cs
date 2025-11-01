using System;
using UnityEngine;

namespace Core.Heroes
{
    [Serializable]
    public class HeroAnimationSet
    {
        public AnimationClip Idle;
        public AnimationClip Death;
        public AnimationClip Hit;
        public AnimationClip Victory;
    }
}