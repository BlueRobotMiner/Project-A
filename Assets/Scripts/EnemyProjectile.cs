// EnemyProjectile
// A projectile fired by turrets and bosses. Travels in the direction it was aimed and
// damages the player on contact. The shooter tints it (bosses use red by default).
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyProjectile : MonoBehaviour
{
    public float speed = 6f;
    public float damage = 10f;
    public float lifetime = 5f;

    [HideInInspector] public GameObject owner;

    private Camera cam;

    void Start()
    {
        cam = Camera.main;
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.velocity = transform.up * speed;
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        Vector3 vp = cam.WorldToViewportPoint(transform.position);
        if (vp.x < -0.1f || vp.x > 1.1f || vp.y < -0.1f || vp.y > 1.1f)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject == owner) return;

        PlayerHealth player = col.GetComponent<PlayerHealth>();
        if (player != null)
        {
            player.TakeDamage(damage, true);
            Destroy(gameObject);
        }
    }
}
