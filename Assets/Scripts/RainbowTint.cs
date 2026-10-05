// RainbowTint
// Cycles a sprite through the full hue spectrum. Used on Rainbow-rarity asteroids and
// the higher-level crates they drop.
// Source: included with the Rasterloom Free Icon Sampler asset pack (freeicon sampler
// demo scripts); comments added for this project.
using UnityEngine;

public class RainbowTint : MonoBehaviour
{
    public float cycleSpeed = 0.5f;
    public float saturation = 0.8f;
    public float brightness = 1f;

    private SpriteRenderer sr;
    private float hueOffset;

    void Start()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
        hueOffset = Random.value;
    }

    void Update()
    {
        if (sr == null) return;
        float hue = Mathf.Repeat(hueOffset + Time.time * cycleSpeed, 1f);
        Color c = Color.HSVToRGB(hue, saturation, brightness);
        c.a = sr.color.a;
        sr.color = c;
    }
}
