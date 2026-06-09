using UnityEngine;

public class BodyPart : MonoBehaviour
{
    [SerializeField] private BodyPartData data;
    
    private CharacterBase _character;

    public void Initialize(CharacterBase character)
    {
        _character = character;
    }
    
    public void Damage(int damage, float knockBack, Vector3 origin)
    {
        print($"Hit <b>{name}</b> in the <b>{data.name}</b> for <b>{damage}</b> damage.");
        _character?.Damage((int)(damage * data.multiplier), knockBack, origin);
    }
}
