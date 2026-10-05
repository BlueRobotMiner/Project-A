// Projectile
// The player's laser. Flies straight, despawns off screen, and damages asteroids,
// crates, turrets and the boss on contact. Damage scales with the Firepower upgrade.
// The shooter tints it (green by default) when firing.
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour
{
    public float speed = 12f;
    public int damage = 1;
    public float damagePerUpgrade = 0.25f;
    public float lifetime = 3f;
    public GameObject hitEffect;

    [HideInInspector] public GameObject owner;

    private Camera cam;
    private SpriteRenderer sr;

    void Start()
    {
        cam = Camera.main;
        sr = GetComponent<SpriteRenderer>();
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.velocity = Vector2.right * speed;
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        float dist = Mathf.Abs(cam.transform.position.z - transform.position.z);
        Vector3 min = cam.ViewportToWorldPoint(new Vector3(0f, 0f, dist));
        Vector3 max = cam.ViewportToWorldPoint(new Vector3(1f, 1f, dist));
        Bounds b = sr != null ? sr.bounds : new Bounds(transform.position, Vector3.zero);

        if (b.min.x > max.x || b.max.x < min.x || b.min.y > max.y || b.max.y < min.y)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject == owner) return;

        float hitDamage = damage * (1f + SaveSystem.Data.firepowerLevel * damagePerUpgrade);
        Boss boss = col.GetComponent<Boss>();
        if (boss != null)
        {
            boss.TakeHit(hitDamage);
            AudioManager.Play(AudioManager.Sfx.ProjectileHit);
            EffectSpawner.SpawnHit(hitEffect, col, transform.position, Vector2.right);
            Destroy(gameObject);
            return;
        }

        BreakableDropper target = col.GetComponent<BreakableDropper>();
        if (target != null)
        {
            target.TakeHit(hitDamage);
            AudioManager.Play(AudioManager.Sfx.ProjectileHit);
            EffectSpawner.SpawnHit(hitEffect, col, transform.position, Vector2.right);
            Destroy(gameObject);
        }
    }
}
