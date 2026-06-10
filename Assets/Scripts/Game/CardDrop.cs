using System;
using Movement;
using UnityEngine;

public class CardDrop : MonoBehaviour
{
    private void OnCollisionEnter(Collision other)
    {
        print(CardManager.Instance);
        if (!other.transform.GetComponent<PlayerController>()) return;
        CardManager.Instance.AddCard();
        gameObject.SetActive(false);
    }
    
}
