using UnityEngine;

namespace Characters
{
    public class BodyPart : MonoBehaviour
    {
        [SerializeField] private BodyPartData data;
        public string Name => name + " " + transform.parent?.name;
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
            if (!_character) Debug.LogWarning($"Body part {name} took doesn't have an associated character.", this);
            _character?.Damage(this, damageTaken, knockBack, origin);
        }

        public void SetColor(Color color) => _material.color = color;

        public void ResetColor() => _material.color = _originalColor;
    }
}
