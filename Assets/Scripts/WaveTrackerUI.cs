using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WaveTrackerUI : MonoBehaviour
{
    public Sprite circleSprite;
    public Sprite bossSprite;
    public Slider progressBar;
    public int visibleBehind = 2;
    public int visibleAhead = 3;
    public float iconSize = 80f;
    public float spacing = 90f;
    public bool fitToPanel = true;
    public float minScale = 0.4f;
    public Color normalColor = Color.white;
    public Color bossColor = new Color(1f, 0.3f, 0.3f);
    public Color textColor = Color.white;
    public float fontSize = 32f;
    public float pulseScale = 1.5f;
    public float pulseRampTime = 0.3f;

    private int displayedWave = -1;
    private float pulseElapsed;
    private float pulseDuration;

    private class Icon
    {
        public RectTransform rect;
        public Image image;
        public TMP_Text label;
        public CanvasGroup group;
    }

    private List<Icon> icons = new List<Icon>();

    void Start()
    {
        if (progressBar != null)
        {
            progressBar.interactable = false;
            progressBar.minValue = 0f;
            progressBar.maxValue = 1f;
        }
        for (int i = 0; i < visibleBehind + visibleAhead + 2; i++)
        {
            icons.Add(CreateIcon(i));
        }
    }

    Icon CreateIcon(int index)
    {
        GameObject go = new GameObject("Wave Icon " + index, typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
        go.transform.SetParent(transform, false);
        Icon icon = new Icon();
        icon.rect = go.GetComponent<RectTransform>();
        icon.rect.sizeDelta = new Vector2(iconSize, iconSize);
        icon.image = go.GetComponent<Image>();
        icon.image.sprite = circleSprite;
        icon.image.raycastTarget = false;
        icon.group = go.GetComponent<CanvasGroup>();

        GameObject textGo = new GameObject("Number", typeof(RectTransform));
        textGo.transform.SetParent(go.transform, false);
        RectTransform textRect = textGo.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        icon.label = textGo.AddComponent<TextMeshProUGUI>();
        icon.label.alignment = TextAlignmentOptions.Center;
        icon.label.fontSize = fontSize;
        icon.label.color = textColor;
        icon.label.fontStyle = FontStyles.Bold;
        icon.label.raycastTarget = false;
        return icon;
    }

    void Update()
    {
        WaveManager wm = WaveManager.Instance;
        if (wm == null) return;

        if (progressBar != null)
        {
            progressBar.value = wm.waveProgress;
        }

        if (displayedWave < 0)
        {
            displayedWave = wm.currentWave;
            pulseDuration = 0f;
        }
        else if (wm.currentWave != displayedWave)
        {
            displayedWave = wm.currentWave;
            pulseElapsed = 0f;
            pulseDuration = Mathf.Max(0.5f, wm.waveBreakDuration);
        }

        float pulse = 1f;
        if (pulseElapsed < pulseDuration)
        {
            pulseElapsed += Time.deltaTime;
            float up = Mathf.Clamp01(pulseElapsed / pulseRampTime);
            float down = Mathf.Clamp01((pulseDuration - pulseElapsed) / pulseRampTime);
            pulse = 1f + (pulseScale - 1f) * Mathf.SmoothStep(0f, 1f, Mathf.Min(up, down));
        }
        for (int i = 0; i < icons.Count; i++)
        {
            int wave = displayedWave - visibleBehind + i;
            float slot = (wave - displayedWave) - wm.waveProgress;
            UpdateIcon(icons[i], wave, slot, wm.IsBossWave(wave), wave == displayedWave ? pulse : 1f);
        }

        icons.Sort((a, b) => a.rect.localScale.x.CompareTo(b.rect.localScale.x));
        for (int i = 0; i < icons.Count; i++)
        {
            icons[i].rect.SetSiblingIndex(i);
        }
    }

    float SlotToX(float slot, float range)
    {
        float a = Mathf.Abs(slot);
        if (range <= 0f) return Mathf.Sign(slot) * a * minScale;
        float x = a <= range
            ? a - (1f - minScale) * a * a / (2f * range)
            : range * (1f + minScale) * 0.5f + (a - range) * minScale;
        return Mathf.Sign(slot) * x;
    }

    float GetSpacing()
    {
        if (!fitToPanel) return spacing;
        float halfWidth = ((RectTransform)transform).rect.width * 0.5f - iconSize * minScale * 0.5f;
        float extent = Mathf.Max(SlotToX(visibleBehind + 0.5f, visibleBehind), SlotToX(visibleAhead + 0.5f, visibleAhead));
        return extent > 0f ? Mathf.Min(spacing, halfWidth / extent) : spacing;
    }

    void UpdateIcon(Icon icon, int wave, float slot, bool isBoss, float pulse)
    {
        bool valid = wave >= 1;
        icon.rect.gameObject.SetActive(valid);
        if (!valid) return;

        float range = slot < 0f ? visibleBehind : visibleAhead;
        icon.rect.anchoredPosition = new Vector2(SlotToX(slot, range) * GetSpacing(), 0f);

        float t = range > 0f ? Mathf.Clamp01(Mathf.Abs(slot) / range) : 1f;
        icon.rect.localScale = Vector3.one * Mathf.Lerp(1f, minScale, t) * pulse;

        icon.group.alpha = Mathf.Clamp01(1f - Mathf.Abs(slot) / (range + 1f));

        icon.image.sprite = isBoss && bossSprite != null ? bossSprite : circleSprite;
        icon.image.color = isBoss ? bossColor : normalColor;
        icon.label.text = wave.ToString();
    }
}
