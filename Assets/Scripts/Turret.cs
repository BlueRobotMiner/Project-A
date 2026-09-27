using UnityEngine;

public class Turret : MonoBehaviour
{
    public int health = 2;
    public Transform barrel;
    public Transform firePoint;
    public GameObject enemyProjectilePrefab;
    public float fireRate = 1.5f;
    public float rotateSpeed = 180f;
    public GameObject destroyEffect;

    [HideInInspector] public bool IsOnScreen;

    private Transform player;
    private Camera cam;
    private float nextFireTime;

    void Start()
    {
        cam = Camera.main;
        PlayerHealth ph = FindObjectOfType<PlayerHealth>();
        if (ph != null) player = ph.transform;
        nextFireTime = Time.time + fireRate;
    }

    void Update()
    {
        float dist = Mathf.Abs(cam.transform.position.z - transform.position.z);
        float left = cam.ViewportToWorldPoint(new Vector3(0f, 0f, dist)).x;
        float right = cam.ViewportToWorldPoint(new Vector3(1f, 0f, dist)).x;
        IsOnScreen = transform.position.x < right && transform.position.x > left;

        if (transform.position.x < left - 2f)
        {
            Destroy(gameObject);
            return;
        }

        if (player == null || !player.gameObject.activeInHierarchy) return;

        Transform aim = barrel != null ? barrel : transform;
        Vector2 dir = player.position - aim.position;
        float targetAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        float angle = Mathf.MoveTowardsAngle(aim.eulerAngles.z, targetAngle, rotateSpeed * Time.deltaTime);
        aim.rotation = Quaternion.Euler(0f, 0f, angle);

        if (IsOnScreen && Time.time >= nextFireTime && enemyProjectilePrefab != null)
        {
            nextFireTime = Time.time + fireRate;
            Transform point = firePoint != null ? firePoint : aim;
            GameObject shot = Instantiate(enemyProjectilePrefab, point.position, aim.rotation);
            shot.GetComponent<EnemyProjectile>().owner = gameObject;
        }
    }

    public void TakeHit(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            if (WaveManager.Instance != null) WaveManager.Instance.AddKill();
            if (destroyEffect != null)
            {
                Instantiate(destroyEffect, transform.position, Quaternion.identity);
            }
            Destroy(gameObject);
        }
    }
}
