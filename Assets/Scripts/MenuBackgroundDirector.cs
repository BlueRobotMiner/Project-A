using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuBackgroundDirector : MonoBehaviour
{
    public GameObject playerShipPrefab;
    public GameObject bossShipPrefab;
    public GameObject playerBulletPrefab;
    public GameObject bossBulletPrefab;
    public GameObject explosionPrefab;
    public float playerFacingAngle = 0f;
    public float bossFacingAngle = 90f;
    public float bulletFacingOffset = -90f;
    public Color playerBulletColor = Color.green;
    public Color bossBulletColor = Color.red;
    public float minEventDelay = 5f;
    public float maxEventDelay = 10f;
    [Range(0f, 1f)] public float battleChance = 0.4f;
    public float flybySpeed = 6f;
    public float enterSpeed = 4f;
    public float battleDuration = 6f;
    public float bobAmount = 1f;
    public float playerFireRate = 0.3f;
    public float bossFireRate = 0.6f;
    public float bulletSpeed = 10f;
    public float hitRadius = 0.8f;
    public float explosionLifetime = 2f;

    private class Bullet
    {
        public Transform t;
        public Vector3 velocity;
        public Transform target;
    }

    private readonly List<Bullet> bullets = new List<Bullet>();
    private Camera cam;

    IEnumerator Start()
    {
        cam = Camera.main;
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minEventDelay, maxEventDelay));
            if (bossShipPrefab != null && playerShipPrefab != null && Random.value < battleChance) yield return StartCoroutine(Battle());
            else if (playerShipPrefab != null) yield return StartCoroutine(Flyby());
        }
    }

    Vector3 View(float x, float y)
    {
        Vector3 p = cam.ViewportToWorldPoint(new Vector3(x, y, Mathf.Abs(cam.transform.position.z)));
        p.z = 0f;
        return p;
    }

    Transform Spawn(GameObject prefab, Vector3 pos, float angle)
    {
        return Instantiate(prefab, pos, Quaternion.Euler(0f, 0f, angle)).transform;
    }

    IEnumerator Flyby()
    {
        float y = Random.Range(0.2f, 0.8f);
        Transform ship = Spawn(playerShipPrefab, View(0f, y) + Vector3.left * 2f, playerFacingAngle);
        float endX = View(1f, y).x + 2f;
        float baseY = ship.position.y;
        while (ship.position.x < endX)
        {
            ship.position = new Vector3(ship.position.x + flybySpeed * Time.deltaTime, baseY + Mathf.Sin(Time.time * 2f) * 0.2f, 0f);
            yield return null;
        }
        Destroy(ship.gameObject);
    }

    IEnumerator Battle()
    {
        Transform player = Spawn(playerShipPrefab, View(0f, 0.5f) + Vector3.left * 2f, playerFacingAngle);
        Transform boss = Spawn(bossShipPrefab, View(1f, 0.5f) + Vector3.right * 3f, bossFacingAngle);
        Vector3 playerHold = View(0.2f, 0.5f);
        Vector3 bossHold = View(0.8f, 0.5f);

        while (player.position != playerHold || boss.position != bossHold)
        {
            player.position = Vector3.MoveTowards(player.position, playerHold, enterSpeed * Time.deltaTime);
            boss.position = Vector3.MoveTowards(boss.position, bossHold, enterSpeed * Time.deltaTime);
            yield return null;
        }

        float end = Time.time + battleDuration;
        float nextPlayerShot = Time.time;
        float nextBossShot = Time.time + bossFireRate;
        while (Time.time < end)
        {
            player.position = playerHold + Vector3.up * Mathf.Sin(Time.time * 1.3f) * bobAmount;
            boss.position = bossHold + Vector3.up * Mathf.Sin(Time.time * 0.9f + 1f) * bobAmount;
            if (Time.time >= nextPlayerShot)
            {
                nextPlayerShot = Time.time + playerFireRate;
                Fire(playerBulletPrefab, player, boss, playerBulletColor);
            }
            if (Time.time >= nextBossShot)
            {
                nextBossShot = Time.time + bossFireRate;
                Fire(bossBulletPrefab, boss, player, bossBulletColor);
            }
            yield return null;
        }

        bool playerWins = Random.value < 0.5f;
        Transform winner = playerWins ? player : boss;
        yield return StartCoroutine(Explode(playerWins ? boss : player));

        float exitX = playerWins ? View(1f, 0.5f).x + 3f : View(0f, 0.5f).x - 3f;
        Vector3 exit = new Vector3(exitX, winner.position.y, 0f);
        while (winner.position != exit)
        {
            winner.position = Vector3.MoveTowards(winner.position, exit, flybySpeed * Time.deltaTime);
            yield return null;
        }
        Destroy(winner.gameObject);
    }

    IEnumerator Explode(Transform ship)
    {
        Vector3 basePos = ship.position;
        float shake = GameSettings.ScreenShake ? 0.15f : 0f;
        for (int i = 0; i < 3; i++)
        {
            SpawnExplosion(basePos + (Vector3)(Random.insideUnitCircle * 0.5f), 0.6f);
            for (float t = 0f; t < 0.3f; t += Time.deltaTime)
            {
                ship.position = basePos + (Vector3)(Random.insideUnitCircle * shake);
                ship.Rotate(0f, 0f, 90f * Time.deltaTime);
                yield return null;
            }
        }
        SpawnExplosion(basePos, 1.2f);
        Destroy(ship.gameObject);
    }

    void Fire(GameObject prefab, Transform from, Transform target, Color color)
    {
        if (prefab == null) return;
        Vector3 dir = (target.position - from.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + bulletFacingOffset;
        GameObject b = Instantiate(prefab, from.position, Quaternion.Euler(0f, 0f, angle));
        EffectSpawner.Tint(b, color);
        bullets.Add(new Bullet { t = b.transform, velocity = dir * bulletSpeed, target = target });
    }

    void SpawnExplosion(Vector3 position, float scale)
    {
        if (explosionPrefab == null) return;
        GameObject effect = Instantiate(explosionPrefab, position, Quaternion.identity);
        effect.transform.localScale *= scale;
        Destroy(effect, explosionLifetime);
    }

    void Update()
    {
        for (int i = bullets.Count - 1; i >= 0; i--)
        {
            Bullet b = bullets[i];
            if (b.t == null)
            {
                bullets.RemoveAt(i);
                continue;
            }
            b.t.position += b.velocity * Time.deltaTime;
            bool hit = b.target != null && (b.t.position - b.target.position).sqrMagnitude < hitRadius * hitRadius;
            Vector3 vp = cam.WorldToViewportPoint(b.t.position);
            if (hit) SpawnExplosion(b.t.position, 0.3f);
            if (hit || vp.x < -0.1f || vp.x > 1.1f || vp.y < -0.1f || vp.y > 1.1f)
            {
                Destroy(b.t.gameObject);
                bullets.RemoveAt(i);
            }
        }
    }
}
