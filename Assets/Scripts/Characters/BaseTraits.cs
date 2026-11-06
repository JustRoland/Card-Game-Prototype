using System;
using UnityEngine;

namespace Characters
{
    public enum TraitType
    {
        None,
        Dash,
        DoubleJump,
        Weaken,
        ExplosiveShot,
        Ricochet,
        Pierce
    }

    [CreateAssetMenu(fileName = "New Traits", menuName = "Traits")]
    public class BaseTraits : ScriptableObject
    {
        public Trait[] traits = new Trait[Enum.GetValues(typeof(TraitType)).Length - 1];

        public BaseTraits()
        {
            for (int i = 0; i < traits.Length; i++)
            {
                traits[i] = new Trait((TraitType)i + 1, false);
            }
        }
    }

    [Serializable]
    public class Trait
    {
        public TraitType type;
        public bool unlocked;

        public Trait(TraitType type, bool unlocked)
        {
            this.type = type;
            this.unlocked = unlocked;
        }
    }
}