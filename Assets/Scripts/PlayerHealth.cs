// PlayerHealth
// The player's hull health, damage handling and death sequence. Damage hits the shield
// first (via PlayerShield), then the hull. Taking damage shakes the ship's visual child,
// and losing all health plays a cutscene: the ship drifts to screen centre, grows, spins,
// explodes, then the game over panel appears.
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth = 100f;
    public float healthPerUpgrade = 10f;
    public Transform shakeTarget;
    public float shakeDuration = 0.2f;
    public float shakeAmount = 0.1f;
    public GameObject explosionEffect;
    public int deathExplosions = 3;
    public float deathDuration = 1.5f;
    public float deathScale = 2f;
    public float deathSpinSpeed = 120f;
    public CanvasGroup[] hudToHide;
    public float hudFadeTime = 0.4f;
    public float deathShakeAmount = 0.25f;
    public float deathExplosionScale = 0.5f;
    public float finalExplosionScale = 1f;
    public float explosionLifetime = 2f;
    public float gameOverDelay = 1f;

    private PlayerShield shield;
    private bool dying;
    private int appliedLevel;
    private Vector3 shakeOrigin;
    private Coroutine shakeRoutine;

    void Start()
    {
        shield = GetComponent<PlayerShield>();
        if (shakeTarget == transform) shakeTarget = null;
        if (shakeTarget != null) shakeOrigin = shakeTarget.localPosition;
        ApplyUpgrades();
        currentHealth = maxHealth;
    }

    // Adds hull upgrade health gained since this object was last refreshed.
    // Called on level start and again when buying upgrades from the shop.
    public void ApplyUpgrades()
    {
        float gain = (SaveSystem.Data.hullLevel - appliedLevel) * healthPerUpgrade;
        appliedLevel = SaveSystem.Data.hullLevel;
        maxHealth += gain;
        currentHealth += gain;
    }

    // Heals the hull, clamped to max. Used by repair crates.
    public void Heal(float amount)
    {
        if (dying) return;
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
    }

    // Applies damage: the shield absorbs what it can, and different hit sounds play
    // depending on whether the shield took it, an asteroid hit the hull, or an enemy
    // laser hit the hull.
    public void TakeDamage(float damage, bool fromLaser = false)
    {
        if (dying) return;
        if (shield != null)
        {
            damage = shield.Absorb(damage);
        }
        if (damage <= 0f) AudioManager.Play(AudioManager.Sfx.ShieldHit);
        else AudioManager.Play(fromLaser ? AudioManager.Sfx.HullHitLaser : AudioManager.Sfx.HullHit);

        currentHealth -= damage;

        if (currentHealth <= 0f)
        {
            currentHealth = 0f;
            StartCoroutine(Die());
            return;
        }
        Shake();
    }

    // Death cutscene: locks controls, fades the HUD, glides the ship to the centre while
    // growing, shaking, spinning and spawning explosions, then shows the game over panel.
    System.Collections.IEnumerator Die()
    {
        dying = true;
        PlayerLaneMovement movement = GetComponent<PlayerLaneMovement>();
        if (movement != null) movement.enabled = false;
        PlayerShooter shooter = GetComponent<PlayerShooter>();
        if (shooter != null) shooter.enabled = false;
        foreach (Collider2D col in GetComponentsInChildren<Collider2D>()) col.enabled = false;
        if (shakeRoutine != null) StopCoroutine(shakeRoutine);

        SpriteRenderer sr = shakeTarget != null ? shakeTarget.GetComponentInChildren<SpriteRenderer>() : GetComponentInChildren<SpriteRenderer>();
        float interval = deathDuration / Mathf.Max(1, GameSettings.ScaleEffectCount(deathExplosions));
        float shake = GameSettings.ScreenShake ? deathShakeAmount : 0f;
        float nextExplosion = interval * 0.5f;

        Vector3 startPos = transform.position;
        Vector3 startScale = transform.localScale;
        Camera cam = Camera.main;
        Vector3 center = cam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, Mathf.Abs(cam.transform.position.z - startPos.z)));
        center.z = startPos.z;
        float grow = 1f;

        for (float t = 0f; t < deathDuration; t += Time.deltaTime)
        {
            float k = Mathf.SmoothStep(0f, 1f, t / deathDuration);
            grow = Mathf.Lerp(1f, deathScale, k);
            transform.position = Vector3.Lerp(startPos, center, k);
            transform.localScale = startScale * grow;
            transform.Rotate(0f, 0f, deathSpinSpeed * (t / deathDuration) * Time.deltaTime);
            SetHudAlpha(1f - Mathf.Clamp01(t / hudFadeTime));

            if (shakeTarget != null) shakeTarget.localPosition = shakeOrigin + (Vector3)(Random.insideUnitCircle * shake);
            if (t >= nextExplosion)
            {
                nextExplosion += interval;
                Bounds b = sr != null ? sr.bounds : new Bounds(transform.position, Vector3.one * 0.5f * grow);
                Vector3 offset = new Vector3(Random.Range(-b.extents.x, b.extents.x), Random.Range(-b.extents.y, b.extents.y), 0f);
                SpawnExplosion(b.center + offset, deathExplosionScale * grow);
            }
            yield return null;
        }

        transform.position = center;
        SetHudAlpha(0f);
        if (shakeTarget != null) shakeTarget.localPosition = shakeOrigin;
        SpawnExplosion(center, finalExplosionScale * grow);
        foreach (Renderer r in GetComponentsInChildren<Renderer>()) r.enabled = false;

        yield return new WaitForSeconds(gameOverDelay);
        if (GameOverMenu.Instance != null) GameOverMenu.Instance.Show();
        gameObject.SetActive(false);
    }

    // Fades the HUD canvas groups out during the death cutscene so it feels like a scene.
    void SetHudAlpha(float alpha)
    {
        if (hudToHide == null) return;
        foreach (CanvasGroup group in hudToHide)
        {
            if (group == null) continue;
            group.alpha = alpha;
            group.interactable = alpha > 0f;
            group.blocksRaycasts = alpha > 0f;
        }
    }

    // Spawns one explosion effect at a position and scale, and plays the ship explosion sound.
    void SpawnExplosion(Vector3 position, float scale)
    {
        AudioManager.Play(AudioManager.Sfx.ShipExplode);
        if (explosionEffect == null) return;
        GameObject effect = Instantiate(explosionEffect, position, Quaternion.identity);
        effect.transform.localScale *= scale;
        Destroy(effect, explosionLifetime);
    }

    void Shake()
    {
        if (shakeTarget == null || !GameSettings.ScreenShake) return;
        if (shakeRoutine != null) StopCoroutine(shakeRoutine);
        shakeRoutine = StartCoroutine(ShakeRoutine());
    }

    System.Collections.IEnumerator ShakeRoutine()
    {
        for (float t = 0f; t < shakeDuration; t += Time.deltaTime)
        {
            float strength = shakeAmount * (1f - t / shakeDuration);
            shakeTarget.localPosition = shakeOrigin + (Vector3)(Random.insideUnitCircle * strength);
            yield return null;
        }
        shakeTarget.localPosition = shakeOrigin;
        shakeRoutine = null;
    }
}
