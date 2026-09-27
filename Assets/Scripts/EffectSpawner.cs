using UnityEngine;

public static class EffectSpawner
{
    public static void SpawnHit(GameObject prefab, Collider2D hitCollider, Vector3 hitterPosition, Vector2 travelDirection)
    {
        if (prefab == null || hitCollider == null) return;

        Vector2 contact = hitCollider.ClosestPoint(hitterPosition);
        Vector2 normal = contact - (Vector2)hitCollider.bounds.center;
        if (normal.sqrMagnitude < 0.0001f) normal = -travelDirection;
        normal.Normalize();

        float angle = Mathf.Atan2(normal.y, normal.x) * Mathf.Rad2Deg - 90f;
        Spawn(prefab, contact, hitCollider.gameObject, 1f, Quaternion.Euler(0f, 0f, angle));
    }

    public static void Spawn(GameObject prefab, Vector3 position, GameObject hitObject, float minLifetime = 1f)
    {
        Spawn(prefab, position, hitObject, minLifetime, Quaternion.identity);
    }

    public static void Spawn(GameObject prefab, Vector3 position, GameObject hitObject, float minLifetime, Quaternion rotation)
    {
        if (prefab == null) return;

        GameObject effect = Object.Instantiate(prefab, position, rotation);

        ObjectScroller scroller = hitObject != null ? hitObject.GetComponent<ObjectScroller>() : null;
        if (scroller != null)
        {
            effect.AddComponent<ObjectScroller>().scrollSpeed = scroller.scrollSpeed;
        }

        float lifetime = minLifetime;
        foreach (ParticleSystem ps in effect.GetComponentsInChildren<ParticleSystem>())
        {
            ParticleSystem.MainModule main = ps.main;
            lifetime = Mathf.Max(lifetime, main.startDelay.constantMax + main.duration + main.startLifetime.constantMax);
        }
        Object.Destroy(effect, lifetime);
    }
}
