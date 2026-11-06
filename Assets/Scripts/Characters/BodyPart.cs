using UnityEngine;

namespace Characters
{
    public class BodyPart : MonoBehaviour
    {
        [SerializeField] private BodyPartData data;
        private CharacterBase _character;
        private Material _material; 
        private Color _originalColor;
        
        public CharacterBase Character => _character;
        public string Name => name + " " + transform.parent?.name;

        public void Initialize(CharacterBase character)
        {
            _character = character;
            _material = GetComponentInChildren<MeshRenderer>().material;
            _originalColor = _material.color;
        }
    
        public void Damage(int damage, float knockBack, Vector3 origin, StatusEffect[] effects = null)
        {
            int damageTaken = (int)(damage * data.multiplier);
            if (!_character) Debug.LogWarning($"Body part {name} took doesn't have an associated character.", this);
            _character?.Damage(this, damageTaken, knockBack, origin, effects);
        }

        public void SetColor(Color color) => _material.color = color;

        public void ResetColor() => _material.color = _originalColor;
    }
}
