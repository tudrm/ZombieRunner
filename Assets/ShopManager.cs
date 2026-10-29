using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    public static ShopManager instance;
    public int monede, gemuri;
    public TextMeshProUGUI textMonede, textGemuri;
    public WeaponCatalog catalog;
    public GameObject shopPanel;
    public RectTransform safeArea, header, categories, collection, details, actionBar;
    public RectTransform cardContent;
    public GameObject cardTemplate;
    public ScrollRect weaponScroll;
    public GridLayoutGroup cardGrid;
    public Button[] categoryButtons;
    public Button purchaseButton;
    public TextMeshProUGUI titleText, tierText, descriptionText, damageText, rateText, dpsText;
    public TextMeshProUGUI damageCompare, rateCompare, dpsCompare;
    public Image damageFill, rateFill, dpsFill;
    public TextMeshProUGUI actionText, priceText, statusText, collectionText, equippedText;
    public WeaponPreview preview;
    public WeaponDefinition Selected { get; private set; }
    public WeaponCategory Category { get; private set; }
    readonly List<Card> cards = new List<Card>();
    bool wasOpen;
    Vector2 lastSize;
    Rect lastSafe;
    static readonly Color Red = new Color(.76f, .18f, .16f);
    static readonly Color Green = new Color(.45f, .83f, .62f);
    static readonly Color Muted = new Color(.58f, .63f, .64f);

    class Card
    {
        public WeaponDefinition weapon;
        public Image background;
        public TextMeshProUGUI ownership;
    }

    void Awake() { instance = this; }
    void Start()
    {
        if (catalog == null) return;
        for (int i = 0; i < categoryButtons.Length; i++)
        {
            int index = i;
            categoryButtons[i].onClick.AddListener(() => SelectCategory(index));
        }
        purchaseButton.onClick.AddListener(PurchaseOrEquip);
        ActualizeazaInterfata();
        if (shopPanel.activeSelf) OpenShop();
    }

    void Update()
    {
        if (shopPanel == null || catalog == null) return;
        bool open = shopPanel.activeInHierarchy;
        if (open && !wasOpen)
        {
            ActualizeazaInterfata();
            SelectCategory((int)(catalog.Equipped?.category ?? WeaponCategory.Pistol));
        }
        wasOpen = open;
        if (!open) return;
        if (lastSize != new Vector2(Screen.width, Screen.height) || lastSafe != Screen.safeArea) Layout();
        if (Input.GetKeyDown(KeyCode.Escape)) CloseShop();
    }

    public void OpenShop()
    {
        if (shopPanel == null || catalog == null) return;
        shopPanel.SetActive(true);
        ActualizeazaInterfata();
        SelectCategory((int)(catalog.Equipped?.category ?? WeaponCategory.Pistol));
        wasOpen = true;
        Layout();
    }

    public void CloseShop() { shopPanel.SetActive(false); wasOpen = false; }

    public void SelectCategory(int index)
    {
        if (catalog == null) return;
        Category = (WeaponCategory)Mathf.Clamp(index, 0, 2);
        foreach (var card in cards) { card.background.gameObject.SetActive(false); Destroy(card.background.gameObject); }
        cards.Clear();
        WeaponDefinition first = null;
        int owned = 0;
        foreach (var weapon in catalog.weapons)
        {
            if (weapon.category != Category) continue;
            if (first == null) first = weapon;
            if (catalog.IsOwned(weapon)) owned++;
            var go = Instantiate(cardTemplate, cardContent);
            go.name = "Weapon_" + weapon.id;
            go.SetActive(true);
            go.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = weapon.displayName;
            go.transform.Find("Stats").GetComponent<TextMeshProUGUI>().text = weapon.damage + " DMG  /  " + weapon.roundsPerMinute + " RPM";
            var thumb = go.transform.Find("Thumbnail").GetComponent<Image>();
            thumb.sprite = weapon.thumbnail;
            thumb.preserveAspect = true;
            var card = new Card { weapon = weapon, background = go.GetComponent<Image>(), ownership = go.transform.Find("Ownership").GetComponent<TextMeshProUGUI>() };
            cards.Add(card);
            go.GetComponent<Button>().onClick.AddListener(() => SelectWeapon(weapon));
        }
        for (int i = 0; i < categoryButtons.Length; i++)
            categoryButtons[i].GetComponent<Image>().color = i == index ? Red : new Color(.1f, .13f, .14f, .97f);
        collectionText.text = "YOUR ARSENAL  /  " + owned + " OF " + cards.Count + " OWNED";
        var equipped = catalog.Equipped;
        SelectWeapon(equipped != null && equipped.category == Category ? equipped : first);
        Canvas.ForceUpdateCanvases();
        weaponScroll.verticalNormalizedPosition = 1;
        Layout();
    }

    public void SelectWeapon(WeaponDefinition weapon)
    {
        if (weapon == null) return;
        Selected = weapon;
        titleText.text = weapon.displayName;
        tierText.text = weapon.category.ToString().ToUpperInvariant() + "  /  TIER " + weapon.tier.ToString("00") + "  /  " + (weapon.Automatic ? "AUTOMATIC" : "SEMI-AUTO");
        descriptionText.text = weapon.description;
        damageText.text = weapon.damage.ToString();
        rateText.text = weapon.roundsPerMinute.ToString();
        float dps = weapon.damage / weapon.FireInterval;
        dpsText.text = Mathf.RoundToInt(dps).ToString();
        damageFill.fillAmount = weapon.damage / 100f;
        rateFill.fillAmount = weapon.roundsPerMinute / 1000f;
        dpsFill.fillAmount = Mathf.Clamp01(dps / 1200f);
        var equipped = catalog.Equipped;
        Compare(damageCompare, weapon.damage - (equipped?.damage ?? 0));
        Compare(rateCompare, weapon.roundsPerMinute - (equipped?.roundsPerMinute ?? 0));
        Compare(dpsCompare, Mathf.RoundToInt(dps - (equipped == null ? 0 : equipped.damage / equipped.FireInterval)));
        statusText.text = "Drag to inspect. Stats compared with your equipped weapon.";
        statusText.color = Muted;
        preview.Show(weapon);
        RefreshSelection();
    }

    void Compare(TextMeshProUGUI text, int difference)
    {
        text.text = difference == 0 ? "EQUAL" : (difference > 0 ? "+" : "") + difference + " VS EQUIPPED";
        text.color = difference > 0 ? Green : difference < 0 ? new Color(.96f, .48f, .42f) : Muted;
    }

    public void PurchaseOrEquip()
    {
        if (Selected == null) return;
        string message;
        if (catalog.IsOwned(Selected))
        {
            catalog.Equip(Selected);
            message = Selected.displayName + " equipped. Ready for the next run.";
        }
        else if (!catalog.Purchase(Selected, out message))
        {
            statusText.text = message;
            statusText.color = new Color(.96f, .48f, .42f);
            return;
        }
        ActualizeazaInterfata();
        SelectWeapon(Selected);
        statusText.text = message;
        statusText.color = Green;
        int owned = 0;
        foreach (var c in cards) if (catalog.IsOwned(c.weapon)) owned++;
        collectionText.text = "YOUR ARSENAL  /  " + owned + " OF " + cards.Count + " OWNED";
    }

    void RefreshSelection()
    {
        var equipped = catalog.Equipped;
        equippedText.text = "LOADOUT  /  " + (equipped?.displayName ?? "NONE");
        foreach (var card in cards)
        {
            card.background.color = card.weapon == Selected ? new Color(.27f, .11f, .11f, .98f) : new Color(.075f, .095f, .1f, .96f);
            card.ownership.text = card.weapon == equipped ? "EQUIPPED" : catalog.IsOwned(card.weapon) ? "OWNED" : card.weapon.price.ToString("N0") + " COINS";
            card.ownership.color = card.weapon == equipped ? Green : catalog.IsOwned(card.weapon) ? Muted : new Color(.94f, .76f, .41f);
        }
        bool owned = catalog.IsOwned(Selected), active = Selected == equipped;
        bool affordable = monede >= Selected.price;
        actionText.text = active ? "EQUIPPED" : owned ? "EQUIP WEAPON" : affordable ? "UNLOCK & EQUIP" : "NOT ENOUGH COINS";
        priceText.text = owned ? "OWNED  /  READY TO DEPLOY" : Selected.price.ToString("N0") + " COINS";
        purchaseButton.interactable = !active && (owned || affordable);
        purchaseButton.GetComponent<Image>().color = active ? new Color(.17f, .29f, .23f) : owned || affordable ? Red : new Color(.22f, .24f, .24f);
    }

    public void ActualizeazaInterfata()
    {
        monede = PlayerPrefs.GetInt(WeaponCatalog.CoinsKey, WeaponCatalog.StartingCoins);
        gemuri = PlayerPrefs.GetInt(WeaponCatalog.GemsKey, 50);
        if (textMonede != null) textMonede.text = monede.ToString("N0");
        if (textGemuri != null) textGemuri.text = gemuri.ToString("N0");
    }

    public void AdaugaMonede(int suma) { WeaponCatalog.AwardCoins(suma); PlayerPrefs.Save(); ActualizeazaInterfata(); }
    public void CheltuieMonede(int suma)
    {
        ActualizeazaInterfata();
        if (suma < 0 || suma > monede) return;
        PlayerPrefs.SetInt(WeaponCatalog.CoinsKey, monede - suma);
        PlayerPrefs.Save();
        ActualizeazaInterfata();
    }

    public void Layout()
    {
        if (safeArea == null) return;
        lastSize = new Vector2(Screen.width, Screen.height);
        lastSafe = Screen.safeArea;
        safeArea.anchorMin = new Vector2(lastSafe.xMin / Mathf.Max(1, Screen.width), lastSafe.yMin / Mathf.Max(1, Screen.height));
        safeArea.anchorMax = new Vector2(lastSafe.xMax / Mathf.Max(1, Screen.width), lastSafe.yMax / Mathf.Max(1, Screen.height));
        safeArea.offsetMin = safeArea.offsetMax = Vector2.zero;
        bool wide = Screen.width > Screen.height * 1.15f;
        Place(header, .035f, .89f, .965f, .985f);
        Place(categories, .035f, wide ? .76f : .815f, wide ? .355f : .965f, wide ? .86f : .875f);
        Place(collection, .035f, wide ? .035f : .115f, wide ? .355f : .965f, wide ? .735f : .385f);
        Place(details, wide ? .38f : .035f, wide ? .185f : .405f, .965f, wide ? .86f : .795f);
        Place(actionBar, wide ? .38f : .035f, .025f, .965f, wide ? .16f : .095f);
        Canvas.ForceUpdateCanvases();
        int columns = wide ? 1 : 2;
        float width = weaponScroll.viewport.rect.width;
        cardGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        cardGrid.constraintCount = columns;
        float cellWidth = (width - cardGrid.padding.horizontal - cardGrid.spacing.x * (columns - 1)) / columns;
        cardGrid.cellSize = new Vector2(Mathf.Max(1, cellWidth), Mathf.Clamp(cellWidth * .36f, 125, 190));
    }

    static void Place(RectTransform r, float x0, float y0, float x1, float y1)
    {
        r.anchorMin = new Vector2(x0, y0); r.anchorMax = new Vector2(x1, y1);
        r.offsetMin = r.offsetMax = Vector2.zero;
    }

    void OnDestroy() { if (instance == this) instance = null; }
}
