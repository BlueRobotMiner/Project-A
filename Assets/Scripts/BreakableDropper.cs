using UnityEngine;

public class BreakableDropper : MonoBehaviour
{
    public int hitsToBreak = 3;
    public float contactDamage = 20f;
    public float lifetime = 20f;
    public GameObject dropPrefab;
    public GameObject breakEffect;
    public float breakEffectLifetime = 2f;
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
