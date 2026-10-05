// ShopUI
// The upgrade shop. Four rows (Hull, Shield, Firepower, Stardust), each with left/right
// arrows: left refunds the last pending level, right buys one at an increasing cost.
// Nothing is charged until Apply, which deducts stardust, saves, and applies hull/shield
// upgrades immediately. Icons fill fractionally: 12 icons show 24 levels.
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
    public enum UpgradeType { Hull, Shield, Firepower, Stardust }

    [System.Serializable]
    public class UpgradeRow
    {
        public UpgradeType type;
        public Button left;
        public Button right;
        public Image[] icons;
        public TMP_Text levelText;
        public TMP_Text costText;
        public Color color = Color.green;
        [HideInInspector] public int pending;
        [HideInInspector] public Image[] pendingFills;
        [HideInInspector] public Image[] ownedFills;
    }

    public UpgradeRow[] rows;
    public TMP_Text bankText;
    public TMP_Text totalText;
    public Button applyButton;
    public Button backButton;
    public GameObject returnPanel;
    public int maxLevel = 24;
    public int baseCost = 250;
    public int costIncrease = 50;
    [Range(0f, 1f)] public float pendingAlpha = 0.4f;

    // Builds the fractional fill overlays over each icon (dim for pending, bright for
    // owned) and wires up the arrows and apply button.
    void Awake()
    {
        foreach (UpgradeRow row in rows)
        {
            UpgradeRow r = row;
            if (r.left != null) r.left.onClick.AddListener(() => Change(r, -1));
            if (r.right != null) r.right.onClick.AddListener(() => Change(r, 1));
            r.pendingFills = new Image[r.icons.Length];
            r.ownedFills = new Image[r.icons.Length];
            for (int i = 0; i < r.icons.Length; i++)
            {
                r.pendingFills[i] = CreateFill(r.icons[i], r.color, pendingAlpha);
                r.ownedFills[i] = CreateFill(r.icons[i], r.color, 1f);
            }
        }
        if (applyButton != null) applyButton.onClick.AddListener(Apply);
        if (backButton != null) backButton.onClick.AddListener(Back);
    }

    void Back()
    {
        gameObject.SetActive(false);
        if (returnPanel != null) returnPanel.SetActive(true);
    }

    void OnEnable()
    {
        foreach (UpgradeRow r in rows) r.pending = 0;
        Refresh();
    }

    Image CreateFill(Image icon, Color color, float alpha)
    {
        GameObject go = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        RectTransform rt = (RectTransform)go.transform;
        rt.SetParent(icon.transform, false);
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        Image img = go.GetComponent<Image>();
        img.sprite = icon.sprite;
        img.preserveAspect = icon.preserveAspect;
        img.raycastTarget = false;
        img.type = Image.Type.Filled;
        img.fillMethod = Image.FillMethod.Horizontal;
        img.fillOrigin = (int)Image.OriginHorizontal.Left;
        color.a = alpha;
        img.color = color;
        return img;
    }

    // Cost of the next level for a row, scaling with levels already owned.
    int Cost(int level)
    {
        return baseCost + level * costIncrease;
    }

    int GetLevel(UpgradeType type)
    {
        SaveData d = SaveSystem.Data;
        switch (type)
        {
            case UpgradeType.Hull: return d.hullLevel;
            case UpgradeType.Shield: return d.shieldLevel;
            case UpgradeType.Firepower: return d.firepowerLevel;
            default: return d.stardustLevel;
        }
    }

    void AddLevel(UpgradeType type, int amount)
    {
        SaveData d = SaveSystem.Data;
        switch (type)
        {
            case UpgradeType.Hull: d.hullLevel += amount; break;
            case UpgradeType.Shield: d.shieldLevel += amount; break;
            case UpgradeType.Firepower: d.firepowerLevel += amount; break;
            default: d.stardustLevel += amount; break;
        }
    }

    // Total cost of every pending level across all rows.
    int PendingTotal()
    {
        int total = 0;
        foreach (UpgradeRow r in rows)
        {
            int level = GetLevel(r.type);
            for (int i = 0; i < r.pending; i++) total += Cost(level + i);
        }
        return total;
    }

    bool CanBuy(UpgradeRow r, int total)
    {
        int next = GetLevel(r.type) + r.pending;
        return next < maxLevel && total + Cost(next) <= SaveSystem.Data.totalStardust;
    }

    // Left arrow refunds one pending level; right arrow buys one if max level and
    // the bank allow it. Nothing is spent until Apply.
    void Change(UpgradeRow r, int dir)
    {
        if (dir < 0 && r.pending > 0) r.pending--;
        else if (dir > 0 && CanBuy(r, PendingTotal())) r.pending++;
        Refresh();
    }

    // Charges the bank, saves the new levels, applies hull/shield to the live player,
    // and clears pending. Pending changes are lost when the panel closes unapplied.
    void Apply()
    {
        int total = PendingTotal();
        if (total <= 0) return;
        SaveSystem.Data.totalStardust -= total;
        foreach (UpgradeRow r in rows)
        {
            AddLevel(r.type, r.pending);
            r.pending = 0;
        }
        SaveSystem.Save();

        PlayerHealth health = FindObjectOfType<PlayerHealth>();
        if (health != null) health.ApplyUpgrades();
        PlayerShield shield = FindObjectOfType<PlayerShield>();
        if (shield != null) shield.ApplyUpgrades();
        Refresh();
    }

    // Refreshes icon fills, level/cost labels, arrow availability, bank and totals.
    void Refresh()
    {
        int total = PendingTotal();
        foreach (UpgradeRow r in rows)
        {
            int level = GetLevel(r.type);
            int next = level + r.pending;
            float perIcon = (float)maxLevel / Mathf.Max(1, r.icons.Length);
            for (int i = 0; i < r.icons.Length; i++)
            {
                r.pendingFills[i].fillAmount = Mathf.Clamp01((next - i * perIcon) / perIcon);
                r.ownedFills[i].fillAmount = Mathf.Clamp01((level - i * perIcon) / perIcon);
            }
            if (r.levelText != null) r.levelText.text = next + "/" + maxLevel;
            if (r.costText != null) r.costText.text = next >= maxLevel ? "MAX" : Cost(next).ToString("N0");
            if (r.left != null) r.left.interactable = r.pending > 0;
            if (r.right != null) r.right.interactable = CanBuy(r, total);
        }
        if (bankText != null) bankText.text = (SaveSystem.Data.totalStardust - total).ToString("N0");
        if (totalText != null) totalText.text = total.ToString("N0");
        if (applyButton != null) applyButton.interactable = total > 0;
    }
}
