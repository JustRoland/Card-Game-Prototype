using System;
using Movement;
using UnityEngine;

public class StatModifierPickup : MonoBehaviour
{
    [SerializeField] private StatType statType;
    [SerializeField] private int value;
    [SerializeField] private float duration;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent(out PlayerController controller)) return;
        var modifier = new BasicModifier(statType, duration, v => v + value);
        controller.Stats.Mediator.AddModifiers(modifier);
    }
}