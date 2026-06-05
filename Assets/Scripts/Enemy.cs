using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private int health = 100;
    
    
    public void Damage(int damage)
    {
        print($"{gameObject.name} took {damage} damage.");
        
        health -= damage;
        
        if (health <= 0) Destroy(gameObject);
    }
}
