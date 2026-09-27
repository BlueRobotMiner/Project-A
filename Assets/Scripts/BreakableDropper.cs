using UnityEngine;

public class BreakableDropper : MonoBehaviour
{
    public enum Rarity { Regular, Shiny, Rainbow }

    [HideInInspector] public Rarity rarity = Rarity.Regular;
    public int hitsToBreak = 3;
    public float contactDamage = 20f;
    public float lifetime = 20f;
    public GameObject dropPrefab;
    public GameObject breakEffect;
    public float breakEffectLifetime = 2f;
    public GameObject stardustPrefab;
    public int minStardust = 3;
    public int maxStardust = 6;
    public bool randomizeScale = true;
    public float minScale = 1f;
    public float maxScale = 2f;

    void Awake()
    {
        if (randomizeScale)
        {
            transform.localScale = Vector3.one * Random.Range(minScale, maxScale);
        }
    }

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    public void TakeHit(int damage)
    {
        hitsToBreak -= damage;
        if (hitsToBreak <= 0)
        {
            Break();
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        PlayerHealth player = col.GetComponent<PlayerHealth>();
        if (player != null)
        {
            player.TakeDamage(contactDamage);
            Destroy(gameObject);
        }
    }

    void Break()
    {
        if (WaveManager.Instance != null) WaveManager.Instance.AddKill();

        if (stardustPrefab != null)
        {
            ObjectScroller ownScroller = GetComponent<ObjectScroller>();
            int count = Random.Range(minStardust, maxStardust + 1);
            for (int i = 0; i < count; i++)
            {
                Vector3 pos = transform.position + (Vector3)(Random.insideUnitCircle * 0.2f);
                GameObject dust = Instantiate(stardustPrefab, pos, Quaternion.identity);
                StardustPickup pickup = dust.GetComponent<StardustPickup>();
                if (pickup != null && ownScroller != null) pickup.scrollSpeed = ownScroller.scrollSpeed;
            }
        }
        if (dropPrefab != null)
        {
            Instantiate(dropPrefab, transform.position, Quaternion.identity);
        }
        if (breakEffect != null)
        {
            GameObject effect = Instantiate(breakEffect, transform.position, Quaternion.identity);
            effect.transform.localScale = transform.localScale;
            ObjectScroller scroller = GetComponent<ObjectScroller>();
            if (scroller != null)
            {
                effect.AddComponent<ObjectScroller>().scrollSpeed = scroller.scrollSpeed;
            }
            float effectTime = breakEffectLifetime;
            foreach (ParticleSystem ps in effect.GetComponentsInChildren<ParticleSystem>())
            {
                ParticleSystem.MainModule main = ps.main;
                effectTime = Mathf.Max(effectTime, main.startDelay.constantMax + main.duration + main.startLifetime.constantMax);
            }
            Destroy(effect, effectTime);
        }
        Destroy(gameObject);
    }
}
