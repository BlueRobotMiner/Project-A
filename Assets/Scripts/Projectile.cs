using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour
{
    public float speed = 12f;
    public int damage = 1;
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

        BreakableDropper target = col.GetComponent<BreakableDropper>();
        if (target != null)
        {
            target.TakeHit(damage);
            if (hitEffect != null)
            {
                Instantiate(hitEffect, transform.position, Quaternion.identity);
            }
            Destroy(gameObject);
        }
    }
}
