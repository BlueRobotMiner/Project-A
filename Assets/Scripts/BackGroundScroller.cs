using System.Collections;
using System.Collections.Generic;
// BackGroundScroller
// The repeating background tiles. Scrolls left and wraps to the right edge once fully
// off screen. Can also cross-fade between galaxy sprites on a timer so the level
// appears to travel into a different galaxy.
using UnityEngine;

public class BackGroundScroller : MonoBehaviour
{
    public BoxCollider2D Collider;
    public Rigidbody2D rb;
    public float scrollSpeed = -2f;
    public static float SpeedMultiplier = 1f;
    public Sprite[] galaxySprites;
    public float galaxyChangeInterval = 0f;
    public float galaxyFadeDuration = 2f;
    private float width;
    private Camera cam;
    private SpriteRenderer sr;
    private SpriteRenderer fadeRenderer;
    private int galaxyIndex;
    private float fadeT;

    void Start()
    {
        Collider = GetComponent<BoxCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;

        sr = GetComponent<SpriteRenderer>();
        if (galaxySprites != null && galaxySprites.Length > 0 && galaxySprites[0] != null) sr.sprite = galaxySprites[0];
        width = sr.bounds.size.x;
        Collider.enabled = false;
    }

    // Cross-fades to the next galaxy sprite: a fading copy is drawn on top, and when
    // it finishes the base sprite is swapped and the copy is removed.
    void UpdateGalaxy()
    {
        if (fadeRenderer != null)
        {
            fadeT += Time.deltaTime / Mathf.Max(0.01f, galaxyFadeDuration);
            Color c = fadeRenderer.color;
            c.a = Mathf.Clamp01(fadeT);
            fadeRenderer.color = c;
            if (fadeT >= 1f)
            {
                sr.sprite = fadeRenderer.sprite;
                Destroy(fadeRenderer.gameObject);
                fadeRenderer = null;
            }
            return;
        }

        if (galaxySprites == null || galaxySprites.Length < 2 || galaxyChangeInterval <= 0f) return;
        int index = Mathf.FloorToInt(Time.timeSinceLevelLoad / galaxyChangeInterval) % galaxySprites.Length;
        if (index == galaxyIndex || galaxySprites[index] == null) return;
        galaxyIndex = index;

        GameObject go = new GameObject("Galaxy Fade");
        go.transform.SetParent(transform, false);
        fadeRenderer = go.AddComponent<SpriteRenderer>();
        fadeRenderer.sprite = galaxySprites[index];
        fadeRenderer.sortingLayerID = sr.sortingLayerID;
        fadeRenderer.sortingOrder = sr.sortingOrder + 1;
        fadeRenderer.drawMode = sr.drawMode;
        if (sr.drawMode != SpriteDrawMode.Simple) fadeRenderer.size = sr.size;
        fadeRenderer.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0f);
        fadeT = 0f;
    }

    void Update()
    {
        UpdateGalaxy();

        // re-applied every frame so Inspector edits take effect live
        rb.velocity = new Vector2(scrollSpeed * SpeedMultiplier, 0);

        float dist = Mathf.Abs(cam.transform.position.z - transform.position.z);
        float leftEdge = cam.ViewportToWorldPoint(new Vector3(0f, 0f, dist)).x;

        // only reset once the sprite is fully off screen on the left
        if (transform.position.x + width / 2f < leftEdge)
        {
            transform.position += Vector3.right * (width * 2f);
        }
    }
}
