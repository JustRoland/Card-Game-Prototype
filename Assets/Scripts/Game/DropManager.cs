using System;
using System.Collections.Generic;
using System.Linq;
using Cards;
using Sirenix.OdinInspector;
using UnityEngine;
using Utility;
using Random = UnityEngine.Random;

namespace Game
{
    public class DropManager : MonoBehaviour
    {
        public static DropManager Instance;

        [ShowInInspector] private int TotalWeight => drops.Sum(drop => drop.weight);
        [SerializeField] private List<Drop> drops = new();

        private readonly List<GenericFactory<GameObject>> _factories = new ();


        private void Awake()
        {
            if (!Instance) Instance = this;
            else Destroy(gameObject);
            
            Debug.Assert(drops.Count > 0, "No drops found");
            
            drops.ForEach(d => _factories.Add(new GenericFactory<GameObject>(d.prefab, cardDrop => !cardDrop.activeSelf,0 , d.maxActive)));
            
            SetLimitValues();
        }

        public GameObject GetDrop()
        {
            if (_factories.Count == 0) return null;
            
            var roll = Random.Range(0, TotalWeight);

            var index = drops.FindIndex(d => roll >= d.lowerLimit && roll <= d.upperLimit);

            return _factories[index].GetItem();

        }
        

        private void SetLimitValues()
        {
            int limit = 0;
            foreach (var drop in drops)
            {
                drop.lowerLimit = limit;
                limit += drop.weight;
                drop.upperLimit = limit;
            }
        }
        
    }

    [Serializable]
    public class Drop
    {
        public GameObject prefab;
        public int weight;
        [HideInInspector] public int lowerLimit;
        [HideInInspector] public int upperLimit;
        public int maxActive;
    }
}