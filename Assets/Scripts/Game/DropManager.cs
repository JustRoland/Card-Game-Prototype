#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using Utility;
using Random = System.Random;

namespace Game
{
    public class DropManager : MonoBehaviour
    {
        public static DropManager Instance;

        [ShowInInspector] private int TotalWeight => availableDrops.Sum(drop => drop.weight);
        [SerializeField] private List<Drop> availableDrops = new();

        private readonly List<GenericFactory<GameObject>> _factories = new();


        private void Awake()
        {
            if (!Instance) Instance = this;
            else Destroy(gameObject);

            Debug.Assert(availableDrops.Count > 0, "No drops found");

            availableDrops.ForEach(d =>
                _factories.Add(new GenericFactory<GameObject>(d.prefab, cardDrop => !cardDrop.activeSelf, 0,
                    d.maxActive)));

        }
        
        public List<GameObject>? GetDrop(int value, Vector3 position)
        {
            if (_factories.Count == 0) return null;

            
            var results  = new List<GameObject>();
            var generatedDrops = GenerateDrops(value, availableDrops, 1);
            if (generatedDrops != null)
                foreach (var drop in generatedDrops.Select(t => availableDrops.FindIndex(d => d.Equals(t)))
                             .Select(index => _factories[index].GetItem()))
                {
                    if (drop) drop.transform.position = position;
                    if (drop) results.Add(drop);
                }

            return results;
        }

        /// <summary>
        /// Returns a random list of items (repeats allowed) whose values sum exactly to <paramref name="target"/>
        /// and whose length is within [minCount, maxCount]. Returns null if no such combination exists.
        /// Items with higher Weight are picked proportionally more often.
        /// </summary>
        private List<Drop>? GenerateDrops(
            int target,
            IReadOnlyList<Drop> items,
            int minCount = 0,
            int? maxCount = null,
            Random? rng = null)
        {
            if (target < 0) throw new ArgumentOutOfRangeException(nameof(target));
            if (minCount < 0) throw new ArgumentOutOfRangeException(nameof(minCount));
            if (items.Any(i => i.value <= 0)) throw new ArgumentException("Item values must be positive.");
            if (items.Any(i => i.weight < 0 || double.IsNaN(i.weight)))
                throw new ArgumentException("Weights must be >= 0.");

            rng ??= new Random();

            // Items with weight 0 can never be chosen, so leave them out entirely.
            var pool = items.Where(i => i.weight > 0).ToList();
            if (pool.Count == 0) return target == 0 && minCount == 0 ? new List<Drop>() : null;

            // No combination can have more than target / smallestValue items.
            int hardMax = target / pool.Min(i => i.value);
            int max = Math.Min(maxCount ?? hardMax, hardMax);
            if (max < minCount) return null;

            // reach[k][v] = true if value v can be made from exactly k items.
            var reach = new bool[max + 1][];
            for (int k = 0; k <= max; k++) reach[k] = new bool[target + 1];
            reach[0][0] = true;
            for (int k = 1; k <= max; k++)
            for (int v = 1; v <= target; v++)
                foreach (var item in pool)
                    if (item.value <= v && reach[k - 1][v - item.value])
                    {
                        reach[k][v] = true;
                        break;
                    }

            // Can we finish value r, having already used 'used' items, with a total count in [minCount, max]?
            bool CanFinish(int r, int used)
            {
                for (int j = Math.Max(0, minCount - used); j <= max - used; j++)
                    if (reach[j][r])
                        return true;
                return false;
            }

            if (!CanFinish(target, 0)) return null;

            // Weighted random walk: only choose items that keep a valid completion possible,
            // so it never dead-ends and always ends exactly on the target within the count limits.
            var result = new List<Drop>();
            var candidates = new List<Drop>(pool.Count);
            int remaining = target;

            while (remaining > 0)
            {
                candidates.Clear();
                double totalWeight = 0;
                foreach (var item in pool)
                    if (item.value <= remaining && CanFinish(remaining - item.value, result.Count + 1))
                    {
                        candidates.Add(item);
                        totalWeight += item.weight;
                    }

                double roll = rng.NextDouble() * totalWeight;
                Drop pick = candidates[^1]; // fallback guards against floating-point rounding
                foreach (var c in candidates)
                {
                    roll -= c.weight;
                    if (roll < 0)
                    {
                        pick = c;
                        break;
                    }
                }

                result.Add(pick);
                remaining -= pick.value;
            }

            return result;
        }


    }

    [Serializable]
    public class Drop : IEquatable<Drop>
    {
    [Required] public GameObject prefab;
    public int weight;
    public int value = 1;
    // [HideInInspector] public int lowerLimit;
    // [HideInInspector] public int upperLimit;
    public int maxActive;
    

    public bool Equals(Drop? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Equals(prefab, other.prefab);
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((Drop)obj);
    }

    public override int GetHashCode()
    {
        return (prefab != null ? prefab.GetHashCode() : 0);
    }
    }
}