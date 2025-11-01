using System.Collections.Generic;
using Core.Heroes.Skills;
using UnityEngine;
using UnityEngine.Serialization;

namespace Core.Heroes
{
    public class HeroAnimator : MonoBehaviour
    {
         [SerializeField]
         private Animator animator;
        
         private HeroSkillsData _heroSkillData;
    }
}