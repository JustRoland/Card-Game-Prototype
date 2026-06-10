using UnityEngine;

public class BodyPart : MonoBehaviour
{
    [SerializeField] private BodyPartData data;
    private CharacterBase _character;
    private Material _material; 
    private Color _originalColor;

    public void Initialize(CharacterBase character)
    {
        _character = character;
        _material = GetComponentInChildren<MeshRenderer>().material;
        _originalColor = _material.color;
    }
    
    public void Damage(int damage, float knockBack, Vector3 origin)
    {
        int damageTaken = (int)(damage * data.multiplier);
        print($"Hit <b>{transform.parent.name}</b> in the <b>{data.name}</b> for <b>{damageTaken}</b> damage.");
        _character?.Damage(this, damageTaken, knockBack, origin);
    }

    public void SetColor(Color color) => _material.color = color;

    public void ResetColor() => _material.color = _originalColor;
}
