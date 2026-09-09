using System;
using Movement;
using UnityEngine;

public class CardDrop : MonoBehaviour, IInteractable
{

    public void Interact()
    {
        CardManager.Instance.AddCard();
        gameObject.SetActive(false);
    }
    
}
