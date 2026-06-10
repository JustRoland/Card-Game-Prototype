using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class Enemy : CharacterBase, IEntity
{
    private EntitySpawner _spawner;
    
    public void SetSpawner(EntitySpawner spawner) => _spawner = spawner;

    public override void Damage(BodyPart bodyPart, int damage, float knockBack, Vector3 origin)
    {
        Stats.Mediator.AddModifiers(new BasicModifier(StatType.Health, 0, v => v - damage));

        if (Stats.Health <= 0)
        {
            _spawner.UnloadEntity(gameObject);
            CardManager.Instance.GetCardDrop(transform.position);
            return;
        }

        OnDamageEffect(bodyPart, damageEffectDuration, knockBack, origin).Forget();
    }

    protected override async UniTask OnDamageEffect(BodyPart bodyPart, float duration, float knockBack, Vector3 origin)
    {
        var knockBackDirection = (RigidBody.position - origin).normalized;
        RigidBody.AddForce(knockBackDirection * knockBack, ForceMode.Impulse);
        bodyPart.SetColor(damageEffectColor);

        await UniTask.WaitForSeconds(duration);

        bodyPart.ResetColor();
        
        await UniTask.Yield();
    }
}