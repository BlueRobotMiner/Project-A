// PlayerRunCards
// Placeholder inventory of card IDs collected in a run. Kept for the future card system;
// ResourcePickup's Card type adds to it.
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
