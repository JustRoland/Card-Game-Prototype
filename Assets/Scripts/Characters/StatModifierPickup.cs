using Characters.Player;
using UnityEngine;

namespace Characters
{
    public class StatModifierPickup : MonoBehaviour
    {
        [SerializeField] private StatType statType;
        [SerializeField] private int value;
        [SerializeField] private float duration;

        private void OnTriggerEnter(Collider other)
        {
            if (!other.TryGetComponent(out PlayerCharacter character)) return;
            var modifier = new BasicModifier(statType, duration, v => v + value);
            character.Stats.Mediator.AddModifiers(modifier);
        }
    }
}