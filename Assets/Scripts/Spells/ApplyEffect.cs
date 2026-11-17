using Characters;
using Game;
using UnityEngine;

namespace Spells
{
    public static class ApplyEffect
    {
        private static readonly Collider[] Results = new Collider[100];
        
        public static void ToAllCharacters(StatusEffect effect)
        {
            foreach (var enemy in EnemyManager.Instance.SpawnedEnemies)
            {
                enemy.Stats.Mediator.AddEffect(effect);
            }
        }

        public static void ToAllCharacters(BasicModifier modifier)
        {
            foreach (var enemy in EnemyManager.Instance.SpawnedEnemies)
            {
                enemy.Stats.Mediator.AddModifiers(modifier);
            }
        }

        public static void ToAllCharacters(int healthChange)
        {
            foreach (var enemy in EnemyManager.Instance.SpawnedEnemies)
            {
                enemy.Stats.Health += healthChange;
            }
        }

        public static void ToArea(Vector3 center, float radius, StatusEffect effect, LayerMask layerMask = default)
        {
            var size = Physics.OverlapSphereNonAlloc(center, radius, Results, layerMask);
            for (int i = 0; i < size; i++)
            {
                Results[i].GetComponent<CharacterBase>()?.Stats.Mediator.AddEffect(effect);
            }
        }
    }
}
