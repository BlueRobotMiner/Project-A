using UnityEngine;

public class ResourcePickup : MonoBehaviour
{
    public enum PickupType { Stardust, ShieldBuff, Card }

    public PickupType type = PickupType.Stardust;
    public int stardustAmount = 1;
    public float shieldBuffAmount = 25f;
    public float shieldBuffDuration = 30f;
    public string cardId = "PlaceholderCard";
    public float floatSpeed = 3f;
    public float lifetime = 15f;

    private Transform player;

    void Start()
    {
        PlayerHealth ph = FindObjectOfType<PlayerHealth>();
        if (ph != null) player = ph.transform;
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (player != null && player.gameObject.activeInHierarchy)
        {
            transform.position = Vector3.MoveTowards(transform.position, player.position, floatSpeed * Time.deltaTime);
        }
        else
        {
            transform.position += Vector3.left * floatSpeed * BackGroundScroller.SpeedMultiplier * Time.deltaTime;
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.GetComponent<PlayerHealth>() == null) return;

        switch (type)
        {
            case PickupType.Stardust:
                PlayerStardust stardust = col.GetComponent<PlayerStardust>();
                if (stardust != null) stardust.Add(stardustAmount);
                break;
            case PickupType.ShieldBuff:
                PlayerShield shield = col.GetComponent<PlayerShield>();
                if (shield != null) shield.ApplyBuff(shieldBuffAmount, shieldBuffDuration);
                break;
            case PickupType.Card:
                PlayerRunCards cards = col.GetComponent<PlayerRunCards>();
                if (cards != null) cards.AddCard(cardId);
                break;
        }

        Destroy(gameObject);
    }
}
