using System.Collections.Generic;
using UnityEngine;

public class PlayerRunCards : MonoBehaviour
{
    public List<string> cards = new List<string>();

    public void AddCard(string cardId)
    {
        cards.Add(cardId);
    }
}
