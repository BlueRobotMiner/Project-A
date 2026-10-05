// ResourcePickup
// A collectible that flies to the player and applies an effect on touch. Used for the
// supply crates: type SupplyCrate opens into a hull repair or a shield recharge (never
// wasted on the full stat). Rarity asteroids drop stronger crates via amountMultiplier.
using UnityEngine;

public class ResourcePickup : MonoBehaviour
{
    public enum PickupType { Stardust, ShieldBuff, Card, SupplyCrate }

    public PickupType type = PickupType.Stardust;
    public int stardustAmount = 1;
    public float shieldBuffAmount = 25f;
    public float shieldBuffDuration = 30f;
    public string cardId = "PlaceholderCard";
    public float repairAmount = 25f;
    public float shieldAmount = 25f;
    [HideInInspector] public float amountMultiplier = 1f;
    public float floatSpeed = 3f;
    public float lifetime = 15f;

    private Transform player;

    void Start()
    {
        PlayerHealth ph = FindObjectOfType<PlayerHealth>();
        if (ph != null) player = ph.transform;
        Destroy(gameObject, lifetime);
    }

    // Magnet flight to the player, or drift with the world when the player is gone.
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

    // Applies the pickup effect based on type, then removes the crate.
    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.GetComponent<PlayerHealth>() == null) return;

        switch (type)
        {
            case PickupType.Stardust:
                PlayerStardust stardust = col.GetComponent<PlayerStardust>();
                if (stardust != null) stardust.Add(stardustAmount);
                AudioManager.Play(AudioManager.Sfx.StardustPickup);
                break;
            case PickupType.ShieldBuff:
                PlayerShield shield = col.GetComponent<PlayerShield>();
                if (shield != null) shield.ApplyBuff(shieldBuffAmount, shieldBuffDuration);
                AudioManager.Play(AudioManager.Sfx.ShieldPickup);
                break;
            case PickupType.Card:
                PlayerRunCards cards = col.GetComponent<PlayerRunCards>();
                if (cards != null) cards.AddCard(cardId);
                break;
            case PickupType.SupplyCrate:
                OpenCrate(col.GetComponent<PlayerHealth>(), col.GetComponent<PlayerShield>());
                break;
        }

        Destroy(gameObject);
    }

    // Chooses repair or shield: if exactly one of the two is full, it picks the other,
    // otherwise a 50/50 roll. The crate's rarity multiplier scales the amount.
    void OpenCrate(PlayerHealth health, PlayerShield shield)
    {
        bool healthFull = health == null || health.currentHealth >= health.maxHealth;
        bool shieldFull = shield == null || shield.currentShield >= shield.maxShield;
        bool repair = healthFull != shieldFull ? !healthFull : Random.value < 0.5f;

        AudioManager.Play(AudioManager.Sfx.CratePickup);
        if (repair && health != null)
        {
            health.Heal(repairAmount * amountMultiplier);
            AudioManager.Play(AudioManager.Sfx.RepairPickup);
        }
        else if (shield != null)
        {
            shield.Recharge(shieldAmount * amountMultiplier);
            AudioManager.Play(AudioManager.Sfx.ShieldPickup);
        }
    }
}
