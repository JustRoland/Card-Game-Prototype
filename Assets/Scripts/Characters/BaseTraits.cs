using System;
using UnityEngine;

namespace Characters
{
    public enum TraitType
    {
        None,
        Dash,
        DoubleJump,
    }

    [CreateAssetMenu(fileName = "New Traits", menuName = "Traits")]
    public class BaseTraits : ScriptableObject
    {
        public Trait[] traits = new Trait[Enum.GetValues(typeof(TraitType)).Length];
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
