using UnityEngine;

public class PlayerShield : MonoBehaviour
{
    public float maxShield = 50f;
    public float currentShield = 50f;
    public SpriteRenderer shieldBubble;
    public float maxBubbleAlpha = 0.6f;

    void Start()
    {
        UpdateBubble();
    }

    public float Absorb(float damage)
    {
        float absorbed = Mathf.Min(currentShield, damage);
        currentShield -= absorbed;
        UpdateBubble();
        return damage - absorbed;
    }

    public void Recharge(float amount)
    {
        currentShield = Mathf.Min(currentShield + amount, maxShield);
        UpdateBubble();
    }

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
