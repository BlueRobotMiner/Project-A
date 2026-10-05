// PlayerShooter
// Fires the player's lasers from up to five fire points in sequence, and torpedoes on a
// cooldown. Laser damage scales with the Firepower upgrade. Shooting is blocked while
// paused, during the level intro, and during the boss entrance cutscene.
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
        if (PauseMenu.IsPaused || PlayerLaneMovement.IntroPlaying || BossIntroPlaying()) return;
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

    // True while the boss wave has started but the boss hasn't reached its position yet,
    // which locks player shooting so the entrance plays like a short cutscene.
    bool BossIntroPlaying()
    {
        WaveManager wm = WaveManager.Instance;
        if (wm == null || !wm.IsBossWave(wm.currentWave)) return false;
        return Boss.Active == null || !Boss.Active.IsEngaged;
    }

    // Fires a torpedo from the torpedo point and ties its launch sound to the projectile,
    // so the sound fades out if the torpedo is destroyed early.
    void FireTorpedo()
    {
        if (torpedoPrefab == null) return;

        nextTorpedoTime = Time.time + torpedoCooldown;
        Transform point = torpedoPoint != null ? torpedoPoint : transform;
        GameObject torpedo = Instantiate(torpedoPrefab, point.position, Quaternion.Euler(0f, 0f, -90f));
        Torpedo t = torpedo.GetComponent<Torpedo>();
        if (t != null) t.launchSound = AudioManager.Play(AudioManager.Sfx.Torpedo);
    }

    // Cycles through the fire points, skipping any empty slots.
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
