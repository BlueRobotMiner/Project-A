using UnityEngine;

public class BreakableDropper : MonoBehaviour
{
    public enum Rarity { Regular, Shiny, Rainbow }

    [HideInInspector] public Rarity rarity = Rarity.Regular;
    public int hitsToBreak = 3;
    public float healthPerWave = 0.25f;
    public float shinyHealthMultiplier = 1.5f;
    public float rainbowHealthMultiplier = 2.5f;
    public float contactDamage = 20f;
    public float lifetime = 20f;
    public GameObject dropPrefab;
    public GameObject breakEffect;
    public float breakEffectLifetime = 2f;
    public GameObject stardustPrefab;
    public int minStardust = 3;
    public int maxStardust = 6;
    public GameObject crateDropPrefab;
    [Range(0f, 1f)] public float crateDropChance = 0.1f;
    public float shinyCrateMultiplier = 2f;
    public float rainbowCrateMultiplier = 3f;
    public bool randomizeScale = true;
    public float minScale = 1f;
    public float maxScale = 2f;

    public float hitShakeDuration = 0.15f;
    public float hitShakeAmount = 0.08f;

    private float health;
    private float shakeTimer;
    private Vector3 shakeOffset;

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
        int wave = WaveManager.Instance != null ? WaveManager.Instance.currentWave : 1;
        float rarityMultiplier = rarity == Rarity.Rainbow ? rainbowHealthMultiplier : rarity == Rarity.Shiny ? shinyHealthMultiplier : 1f;
        health = hitsToBreak * (1f + (wave - 1) * healthPerWave) * rarityMultiplier;
    }

    void LateUpdate()
    {
        transform.position -= shakeOffset;
        shakeOffset = Vector3.zero;
        if (shakeTimer <= 0f) return;
        shakeTimer -= Time.deltaTime;
        if (GameSettings.ScreenShake && hitShakeDuration > 0f)
        {
            shakeOffset = (Vector3)(Random.insideUnitCircle * hitShakeAmount * Mathf.Clamp01(shakeTimer / hitShakeDuration));
        }
        transform.position += shakeOffset;
    }

    public void TakeHit(float damage)
    {
        shakeTimer = hitShakeDuration;
        health -= damage;
        if (health <= 0f)
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
        AudioManager.Play(AudioManager.Sfx.AsteroidExplode);

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
        if (crateDropPrefab != null && Random.value < crateDropChance)
        {
            GameObject crate = Instantiate(crateDropPrefab, transform.position, Quaternion.identity);
            ResourcePickup pickup = crate.GetComponent<ResourcePickup>();
            if (rarity == Rarity.Rainbow)
            {
                if (pickup != null) pickup.amountMultiplier = rainbowCrateMultiplier;
                crate.AddComponent<RainbowTint>();
            }
            else if (rarity == Rarity.Shiny)
            {
                if (pickup != null) pickup.amountMultiplier = shinyCrateMultiplier;
                SpriteRenderer ownSr = GetComponentInChildren<SpriteRenderer>();
                if (ownSr != null) EffectSpawner.Tint(crate, ownSr.color);
            }
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
