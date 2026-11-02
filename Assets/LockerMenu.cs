using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Inventory UI only. WeaponCatalog remains the authority for ownership and equipment.
public sealed class LockerMenu : MonoBehaviour
{
    public WeaponCatalog catalog;
    public ShopManager shop;
    public GameObject panel;
    public LockerPresentation presentation;
    public WeaponPreview preview;
    public LockerWeaponCard cardPrefab;
    public RectTransform cardContent;
    public ScrollRect scroll;
    public Button equipButton, closeButton;
    public TextMeshProUGUI coinsText, gemsText, titleText, categoryText, stateText;
    public TextMeshProUGUI damageText, rateText, dpsText, actionText, equippedText, countText, emptyText;
    public WeaponDefinition Selected { get; private set; }
    public IReadOnlyList<LockerWeaponCard> Cards => cards;

    readonly List<LockerWeaponCard> cards = new List<LockerWeaponCard>();
    readonly List<WeaponDefinition> owned = new List<WeaponDefinition>();
    readonly HashSet<string> ids = new HashSet<string>();
    Vector2 lastScreen, lastSafeSize;
    Rect lastSafe;
    float nextRefresh;

    public void Open()
    {
        // Both menus reuse WeaponPreview's isolated layer. Keep only one stage active.
        shop.CloseShop();
        panel.SetActive(true);
        RefreshInventory();
        Layout();
        nextRefresh = Time.unscaledTime + .25f;
    }

    public void Close()
    {
        scroll.StopMovement();
        panel.SetActive(false); // WeaponPreview.OnDisable also hides its camera and model.
    }

    void Update()
    {
        if (!panel.activeInHierarchy) return;
        // Covers external navigation as well as the normal Main Menu buttons.
        if (shop.shopPanel.activeInHierarchy) { Close(); return; }
        if (Input.GetKeyDown(KeyCode.Escape)) { Close(); return; }
        if (lastScreen != new Vector2(Screen.width, Screen.height) || lastSafe != Screen.safeArea || lastSafeSize != presentation.safeArea.rect.size)
            Layout();
        // The existing catalog has no change event. Read its small list while visible;
        // do not add a second save system or alter the Shop purchase flow.
        if (Time.unscaledTime >= nextRefresh)
        {
            nextRefresh = Time.unscaledTime + .25f;
            RefreshInventory();
        }
    }

    void OnApplicationFocus(bool focused)
    {
        if (focused && panel != null && panel.activeInHierarchy) RefreshInventory();
    }

    bool CanSelect(WeaponDefinition weapon) => catalog != null && weapon != null &&
        catalog.Find(weapon.id) == weapon && catalog.IsOwned(weapon);

    public void RefreshInventory()
    {
        owned.Clear(); ids.Clear();
        if (catalog != null)
            foreach (var weapon in catalog.weapons)
                if (CanSelect(weapon) && ids.Add(weapon.id)) owned.Add(weapon);

        bool changed = owned.Count != cards.Count;
        if (!changed)
            for (int i = 0; i < owned.Count; i++)
                if (cards[i].Weapon != owned[i]) { changed = true; break; }
        if (changed)
        {
            foreach (var card in cards) { card.gameObject.SetActive(false); Destroy(card.gameObject); }
            cards.Clear();
            foreach (var weapon in owned)
            {
                var card = Instantiate(cardPrefab, cardContent);
                card.name = "Owned_" + weapon.id;
                card.Bind(this, weapon);
                card.gameObject.SetActive(true);
                cards.Add(card);
            }
        }

        var selection = Selected;
        if (!owned.Contains(selection))
            selection = catalog != null && owned.Contains(catalog.Equipped) ? catalog.Equipped : owned.Count > 0 ? owned[0] : null;
        ShowSelection(selection);
        RefreshState();
        if (changed && panel.activeInHierarchy) Layout();
    }

    public void SelectWeapon(WeaponDefinition weapon)
    {
        if (!CanSelect(weapon)) return;
        ShowSelection(weapon);
        RefreshState();
        scroll.StopMovement();
        scroll.verticalNormalizedPosition = 1;
    }

    void ShowSelection(WeaponDefinition weapon)
    {
        bool changed = Selected != weapon;
        Selected = weapon;
        preview.gameObject.SetActive(weapon != null);
        if (weapon != null && (changed || preview.display.texture == null)) preview.Show(weapon);
        titleText.text = weapon != null ? weapon.displayName : "NO WEAPONS";
        categoryText.text = weapon != null ? weapon.category.ToString().ToUpperInvariant() + "  /  " + (weapon.Automatic ? "AUTOMATIC" : "SEMI-AUTO") : "INVENTORY";
        damageText.text = weapon != null ? weapon.damage.ToString() : "—";
        rateText.text = weapon != null ? weapon.roundsPerMinute.ToString() : "—";
        dpsText.text = weapon != null ? Mathf.RoundToInt(weapon.damage / weapon.FireInterval).ToString() : "—";
    }

    public void EquipSelected()
    {
        if (!CanSelect(Selected)) { RefreshInventory(); return; }
        if (catalog.Equipped != Selected) catalog.Equip(Selected);
        RefreshState();
    }

    void RefreshState()
    {
        var equipped = catalog != null ? catalog.Equipped : null;
        bool valid = CanSelect(Selected), active = valid && Selected == equipped;
        equipButton.interactable = valid && !active;
        actionText.text = !valid ? "NO WEAPON AVAILABLE" : active ? "EQUIPPED" : "EQUIP " + Selected.displayName;
        stateText.text = !valid ? "" : active ? "EQUIPPED" : "OWNED";
        equippedText.text = equipped != null ? "EQUIPPED: " + equipped.displayName : "NO WEAPON AVAILABLE";
        countText.text = cards.Count.ToString("00");
        emptyText.gameObject.SetActive(cards.Count == 0);
        coinsText.text = PlayerPrefs.GetInt(WeaponCatalog.CoinsKey, WeaponCatalog.StartingCoins).ToString("N0");
        gemsText.text = PlayerPrefs.GetInt(WeaponCatalog.GemsKey, 50).ToString("N0");
        foreach (var card in cards) card.Refresh(Selected, equipped);
    }

    public void Layout()
    {
        lastScreen = new Vector2(Screen.width, Screen.height); lastSafe = Screen.safeArea;
        presentation.safeArea.anchorMin = new Vector2(lastSafe.xMin / Mathf.Max(1, Screen.width), lastSafe.yMin / Mathf.Max(1, Screen.height));
        presentation.safeArea.anchorMax = new Vector2(lastSafe.xMax / Mathf.Max(1, Screen.width), lastSafe.yMax / Mathf.Max(1, Screen.height));
        presentation.safeArea.offsetMin = presentation.safeArea.offsetMax = Vector2.zero;
        presentation.Layout();
        lastSafeSize = presentation.safeArea.rect.size;
    }
}
