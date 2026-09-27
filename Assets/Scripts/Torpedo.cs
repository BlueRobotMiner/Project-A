using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Torpedo : MonoBehaviour
{
    public float speed = 7f;
    public float turnSpeed = 200f;
    public int damage = 1;
    [Range(0f, 1f)] public float missChance = 0.25f;
    public float missOffset = 1.5f;
    public float lifetime = 6f;
    public GameObject hitEffect;

    [HideInInspector] public Turret target;

    private Rigidbody2D rb;
    private Camera cam;
    private Vector3 aimOffset;
    private bool homing = true;

    void Start()
    {
        cam = Camera.main;
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        if (Random.value < missChance)
        {
            aimOffset = Random.insideUnitCircle.normalized * missOffset;
        }
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (homing && target != null)
        {
            Vector3 aimPoint = target.transform.position + aimOffset;
            Vector2 dir = aimPoint - transform.position;
            if (aimOffset != Vector3.zero && dir.magnitude < 0.3f)
            {
                homing = false;
            }
            float targetAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
            float angle = Mathf.MoveTowardsAngle(transform.eulerAngles.z, targetAngle, turnSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        rb.velocity = transform.up * speed;

        Vector3 vp = cam.WorldToViewportPoint(transform.position);
        if (vp.x < -0.1f || vp.x > 1.1f || vp.y < -0.1f || vp.y > 1.1f)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        Turret turret = col.GetComponent<Turret>();
        if (turret != null)
        {
            turret.TakeHit(damage);
            if (hitEffect != null)
            {
                Instantiate(hitEffect, transform.position, Quaternion.identity);
            }
            Destroy(gameObject);
        }
    }
}
