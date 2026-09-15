using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class Enemy : CharacterBase, IEntity
{
    private EntitySpawner _spawner;
    
    public void SetSpawner(EntitySpawner spawner) => _spawner = spawner;

    public override void Damage(BodyPart bodyPart, int damage, float knockBack, Vector3 origin)
    {
        base.Damage(bodyPart, damage, knockBack, origin);

        if (Stats.Health > 0) return;
        
        _spawner.UnloadEntity(gameObject);
        CardManager.Instance.GetCardDrop(transform.position);
    }
}