using UnityEngine;

public class PlayerShooter : MonoBehaviour
{
    public GameObject weapon_prefab;
    public Transform[] firePoints = new Transform[5];
    public float fireRate = 0.25f;
    public Color projectileColor = Color.green;
    public GameObject torpedoPrefab;
    public Transform torpedoPoint;
    public float torpedoCooldown = 1.5f;
    public KeyCode torpedoKey = KeyCode.E;

    private float nextFireTime;
    private float nextTorpedoTime;
    private int fireIndex;

    void Update()
    {
        if (PauseMenu.IsPaused || PlayerLaneMovement.IntroPlaying) return;
        if ((Input.GetKeyDown(torpedoKey) || Input.GetMouseButtonDown(1)) && Time.time >= nextTorpedoTime)
        {
            FireTorpedo();
        }

        if ((GameSettings.AutoFire || Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0)) && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate;
            Transform point = GetNextFirePoint();
            GameObject bullet = Instantiate(weapon_prefab, point.position, Quaternion.Euler(0f, 0f, -90f));
            bullet.GetComponent<Projectile>().owner = gameObject;
            EffectSpawner.Tint(bullet, projectileColor);
            AudioManager.Play(AudioManager.Sfx.PlayerLaser);
        }
    }

    void FireTorpedo()
    {
        if (torpedoPrefab == null) return;

        nextTorpedoTime = Time.time + torpedoCooldown;
        Transform point = torpedoPoint != null ? torpedoPoint : transform;
        Instantiate(torpedoPrefab, point.position, Quaternion.Euler(0f, 0f, -90f));
        AudioManager.Play(AudioManager.Sfx.Torpedo);
    }

    Transform GetNextFirePoint()
    {
        int count = Mathf.Min(firePoints.Length, 5);
        for (int i = 0; i < count; i++)
        {
            Transform point = firePoints[fireIndex % count];
            fireIndex = (fireIndex + 1) % count;
            if (point != null)
            {
                return point;
            }
        }
        return transform;
    }
}
