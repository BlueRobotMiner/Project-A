// PlayerShield
// The player's regenerating-style shield pool. Absorb() soaks up damage before the hull,
// Recharge() refills it (shield pickups and crates), and ApplyBuff() raises max shield
// temporarily. The bubble sprite's transparency shows how much shield is left.
using UnityEngine;

public class PlayerShield : MonoBehaviour
{
    public float maxShield = 50f;
    public float currentShield = 50f;
    public SpriteRenderer shieldBubble;
    public float maxBubbleAlpha = 0.6f;
    public float shieldPerUpgrade = 5f;

    private int appliedLevel;

    void Start()
    {
        ApplyUpgrades();
    }

    public void ApplyUpgrades()
    {
        float gain = (SaveSystem.Data.shieldLevel - appliedLevel) * shieldPerUpgrade;
        appliedLevel = SaveSystem.Data.shieldLevel;
        maxShield += gain;
        currentShield += gain;
        UpdateBubble();
    }

    // soaks up as much damage as the shield can cover and returns the rest for the hull.
    public float Absorb(float damage)
    {
        float absorbed = Mathf.Min(currentShield, damage);
        currentShield -= absorbed;
        UpdateBubble();
        return damage - absorbed;
    }

    // Refills shield up to max, without going over.
    public void Recharge(float amount)
    {
        currentShield = Mathf.Min(currentShield + amount, maxShield);
        UpdateBubble();
    }

    // Temporarily raises max shield (shield-buff pickups), then removes the bonus later.
    public void ApplyBuff(float amount, float duration)
    {
        maxShield += amount;
        currentShield += amount;
        UpdateBubble();
        StartCoroutine(RemoveBuff(amount, duration));
    }

    System.Collections.IEnumerator RemoveBuff(float amount, float duration)
    {
        yield return new WaitForSeconds(duration);
        maxShield -= amount;
        currentShield = Mathf.Min(currentShield, maxShield);
        UpdateBubble();
    }

    void UpdateBubble()
    {
        if (shieldBubble == null) return;
        shieldBubble.enabled = currentShield > 0f;
        Color c = shieldBubble.color;
        c.a = maxBubbleAlpha * (currentShield / maxShield);
        shieldBubble.color = c;
    }
}
