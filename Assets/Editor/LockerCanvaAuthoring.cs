#if UNITY_EDITOR
using System;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public static class LockerCanvaAuthoring
{
    static TMP_FontAsset font;
    static Sprite frame, fill, card;
    static float ppu;
    static readonly Color White = ShopPresentation.White, Lime = ShopPresentation.Lime, Muted = ShopPresentation.Muted;
    const string PrefabPath = "Assets/UI/Locker/Prefabs/OwnedWeaponCard.prefab";

    [MenuItem("Tools/ZombieRunner/Apply Approved Locker (MainMenu Only)")]
    public static void Apply()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (EditorApplication.isPlaying || scene.path != "Assets/Scenes/MainMenu.unity" || scene.isDirty)
            throw new InvalidOperationException("Open the saved MainMenu scene outside Play Mode.");
        if (Object.FindFirstObjectByType<LockerMenu>() != null)
            throw new InvalidOperationException("Locker is already installed. Do not apply twice.");
        if (!File.Exists("Backups/LockerCanva-2026-11-02/MainMenu.unity"))
            throw new InvalidOperationException("Locker baseline backup is missing.");
        var menu = Object.FindFirstObjectByType<MainMenuManager>();
        var shop = Object.FindFirstObjectByType<ShopManager>();
        font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/UI/ShopCanva/Fonts/BarlowCondensed SDF.asset");
        frame = Art("Frame"); fill = Art("ButtonNeutral"); card = Art("Card");
        var city = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/MainMenuCanva/CityBackground.jpg");
        if (font == null || frame == null || fill == null || card == null || city == null)
            throw new InvalidOperationException("Approved shared UI assets are missing.");
        ppu = menu.GetComponent<Canvas>().referencePixelsPerUnit / 100f;

        var locker = menu.gameObject.AddComponent<LockerMenu>();
        var view = menu.gameObject.AddComponent<LockerPresentation>();
        locker.presentation = view; view.locker = locker; locker.catalog = shop.catalog; locker.shop = shop;
        var panel = Node("Panel_Locker", menu.transform); Stretch(panel);
        locker.panel = panel.gameObject;
        // Keep loading last, so the original loading flow still covers the menu.
        panel.SetSiblingIndex(menu.loadingPanel.transform.GetSiblingIndex());
        var bg = Image("CityBackground", panel, city, Color.white); Stretch(bg.rectTransform); view.background = bg;
        var shade = Image("ReadabilityOverlay", panel, null, new Color(0, .01f, .02f, .48f)); Stretch(shade.rectTransform); shade.raycastTarget = true;
        view.safeArea = Node("LockerSafeArea", panel); Stretch(view.safeArea);
        view.header = Node("Header", view.safeArea);
        var headerBackground = view.header.gameObject.AddComponent<Image>(); headerBackground.color = new Color(.025f,.035f,.045f,.86f); headerBackground.raycastTarget = false;
        Label("Brand", view.header, "ZOMBIE<color=#BEF24A>RUNNER</color>", 22, Muted, 0,.73f,.47f,1);
        Label("Title", view.header, "LOCKER", 42, White, 0,.28f,.40f,.78f);
        Label("Subtitle", view.header, "YOUR WEAPONS. YOUR LOADOUT.", 20, Muted, 0,0,.83f,.29f);
        locker.coinsText = Wallet(view.header, "Coins", Art("Coin"), .45f, .65f);
        locker.gemsText = Wallet(view.header, "Gems", Art("Gem"), .67f, .87f);
        locker.closeButton = Button("Close", view.header, "×", 38, frame, new Color(.33f,.4f,.45f));
        Rect(locker.closeButton.transform, .89f,.30f,1,.97f);
        UnityEventTools.AddPersistentListener(locker.closeButton.onClick, locker.Close);

        var scrollNode = Node("InventoryScroll", view.safeArea); Stretch(scrollNode);
        locker.scroll = scrollNode.gameObject.AddComponent<ScrollRect>();
        locker.scroll.horizontal = false; locker.scroll.vertical = true;
        locker.scroll.movementType = ScrollRect.MovementType.Clamped;
        locker.scroll.scrollSensitivity = 36; locker.scroll.decelerationRate = .08f;
        var viewport = Node("Viewport", scrollNode); Stretch(viewport);
        var hit = viewport.gameObject.AddComponent<Image>(); hit.color = Color.clear; hit.raycastTarget = true;
        viewport.gameObject.AddComponent<RectMask2D>(); locker.scroll.viewport = viewport;
        // Layout controls the viewport; the ScrollRect's root cannot block the fixed header.
        scrollNode.SetAsFirstSibling();
        view.page = Node("Page", viewport); view.page.anchorMin = new Vector2(0,1); view.page.anchorMax = Vector2.one; view.page.pivot = new Vector2(.5f,1);
        locker.scroll.content = view.page;
        view.details = Node("SelectedWeapon", view.page);
        var panelImage = view.details.gameObject.AddComponent<Image>(); panelImage.color = new Color32(17,24,26,245); panelImage.raycastTarget = false;
        Border(view.details, new Color(.29f,.36f,.37f));
        locker.categoryText = Label("Category", view.details, "PISTOL / SEMI-AUTO", 23, Muted, .04f,.91f,.96f,.98f);
        locker.titleText = Label("Name", view.details, "", 42, White, .04f,.79f,.96f,.92f);
        locker.stateText = Label("State", view.details, "", 23, Lime, .04f,.71f,.40f,.79f);
        var preview = Node("WeaponPreview", view.details); Rect(preview,.07f,.30f,.97f,.78f);
        var raw = preview.gameObject.AddComponent<RawImage>(); raw.color = Color.white; raw.raycastTarget = true;
        locker.preview = preview.gameObject.AddComponent<WeaponPreview>(); locker.preview.display = raw;
        locker.preview.transparentBackground = true; locker.preview.rimColor = new Color(1,.68f,.31f);
        Label("RotateHint", view.details, "360°  DRAG TO ROTATE", 22, Muted, .04f,.23f,.96f,.325f);
        locker.damageText = Stat(view.details, 0, "DAMAGE"); locker.rateText = Stat(view.details, 1, "FIRE RATE / RPM"); locker.dpsText = Stat(view.details, 2, "DAMAGE / SEC");
        locker.equipButton = Button("Equip", view.page, "EQUIPPED", 32, fill, Color.white);
        locker.actionText = locker.equipButton.GetComponentInChildren<TextMeshProUGUI>(); locker.actionText.color = ShopPresentation.Ink;
        var colors = locker.equipButton.colors; colors.normalColor = Lime; colors.highlightedColor = new Color32(210,255,108,255); colors.selectedColor = colors.highlightedColor; colors.pressedColor = new Color32(146,194,46,255); colors.disabledColor = new Color32(101,125,58,255); locker.equipButton.colors = colors;
        UnityEventTools.AddPersistentListener(locker.equipButton.onClick, locker.EquipSelected); view.action = (RectTransform)locker.equipButton.transform;
        locker.equippedText = Label("EquippedWeapon", view.page, "", 24, White,0,0,1,1,TextAlignmentOptions.Center); view.equipped = locker.equippedText.rectTransform;
        view.collectionHeader = Node("OwnedHeader", view.page);
        Label("Heading", view.collectionHeader, "OWNED WEAPONS", 32, White,0,0,.83f,1);
        locker.countText = Label("Count", view.collectionHeader, "00", 32, Lime,.85f,0,1,1,TextAlignmentOptions.MidlineRight);
        view.cards = Node("Cards", view.page); locker.cardContent = view.cards;
        view.cardLayout = view.cards.gameObject.AddComponent<GridLayoutGroup>(); view.cardLayout.childAlignment = TextAnchor.UpperLeft;
        locker.emptyText = Label("EmptyInventory", view.page, "No weapons available.\nUnlock weapons in the Shop to expand your Locker.", 26, Muted,0,0,1,1,TextAlignmentOptions.Center);
        locker.emptyText.textWrappingMode = TextWrappingModes.Normal; view.empty = locker.emptyText.rectTransform;
        view.footer = Label("InventoryHint", view.page, "Unlock more weapons in the Shop.",24,Muted,0,0,1,1,TextAlignmentOptions.Center).rectTransform;
        locker.cardPrefab = CreateCard();

        var button = menu.lockerButton.GetComponent<Button>();
        if (button.onClick.GetPersistentEventCount() != 1 || button.onClick.GetPersistentMethodName(0) != "OpenShop" || button.onClick.GetPersistentTarget(0) != shop)
            throw new InvalidOperationException("Unexpected Locker binding; inspect before replacing.");
        UnityEventTools.RemovePersistentListener(button.onClick,0);
        UnityEventTools.AddPersistentListener(button.onClick,locker.Open);
        locker.Layout(); locker.panel.SetActive(false);
        EditorUtility.SetDirty(menu.gameObject); EditorUtility.SetDirty(button);
        EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
        Debug.Log("Locker created in MainMenu. Existing Shop, catalog, gameplay and save keys retained.");
    }

    static LockerWeaponCard CreateCard()
    {
        Directory.CreateDirectory("Assets/UI/Locker/Prefabs");
        var b = Button("OwnedWeaponCard", null, "", 30, card, Color.white);
        Object.DestroyImmediate(b.GetComponentInChildren<TextMeshProUGUI>().gameObject);
        var v = b.gameObject.AddComponent<LockerWeaponCard>(); v.button = b;
        v.thumbnail = Image("Thumbnail",b.transform,null,Color.white); Rect(v.thumbnail.transform,.02f,.10f,.26f,.90f); v.thumbnail.preserveAspect = true;
        v.title = Label("Name",b.transform,"WEAPON",30,White,.29f,.44f,.74f,.92f);
        v.stats = Label("Stats",b.transform,"",22,Muted,.29f,.09f,.97f,.43f);
        v.state = Label("State",b.transform,"OWNED",24,Muted,.75f,.46f,.97f,.88f,TextAlignmentOptions.MidlineRight);
        v.selectedFrame = Border(b.transform,Lime); v.selectedFrame.name = "Selected";
        var prefab = PrefabUtility.SaveAsPrefabAsset(b.gameObject,PrefabPath);
        Object.DestroyImmediate(b.gameObject);
        return prefab.GetComponent<LockerWeaponCard>();
    }

    static TextMeshProUGUI Stat(Transform parent,int index,string caption)
    {
        var r = Node("Stat_"+index,parent); Rect(r,.04f+index*.315f,.015f,.33f+index*.315f,.23f);
        var im = r.gameObject.AddComponent<Image>(); im.color = new Color(.02f,.03f,.035f,.94f); im.raycastTarget = false; Border(r,new Color(.27f,.33f,.36f));
        Label("Caption",r,caption,22,Muted,.02f,.58f,.98f,.96f,TextAlignmentOptions.Center);
        return Label("Value",r,"—",36,White,.02f,.02f,.98f,.61f,TextAlignmentOptions.Center);
    }
    static TextMeshProUGUI Wallet(Transform parent,string name,Sprite sprite,float min,float max)
    {
        var r = Node(name,parent); Rect(r,min,.34f,max,.94f); Border(r,new Color(.32f,.39f,.43f));
        var icon = Image("Icon",r,sprite,Color.white); Rect(icon.transform,.05f,.24f,.30f,.76f); icon.preserveAspect = true;
        return Label("Balance",r,"0",27,White,.33f,.12f,.96f,.88f);
    }
    static Button Button(string name,Transform parent,string text,float size,Sprite sprite,Color color)
    {
        var image = Image(name,parent,sprite,color); image.raycastTarget = true;
        var b = image.gameObject.AddComponent<Button>(); b.targetGraphic = image;
        var c = b.colors; c.highlightedColor = new Color(.91f,1,.83f); c.selectedColor = c.highlightedColor; c.pressedColor = new Color(.65f,.78f,.53f); c.fadeDuration = .12f; b.colors = c;
        Label("Label",b.transform,text,size,White,.025f,.08f,.975f,.92f,TextAlignmentOptions.Center);
        return b;
    }
    static Image Border(Transform parent,Color color) { var i=Image("Border",parent,frame,color); Stretch(i.rectTransform); return i; }
    static Image Image(string name,Transform parent,Sprite sprite,Color color)
    {
        var r=Node(name,parent);var i=r.gameObject.AddComponent<Image>();i.sprite=sprite;i.color=color;i.raycastTarget=false;
        i.type=sprite!=null?UnityEngine.UI.Image.Type.Sliced:UnityEngine.UI.Image.Type.Simple;i.pixelsPerUnitMultiplier=ppu;return i;
    }
    static TextMeshProUGUI Label(string name,Transform parent,string text,float size,Color color,float x0,float y0,float x1,float y1,TextAlignmentOptions align=TextAlignmentOptions.MidlineLeft)
    {
        var r=Node(name,parent);Rect(r,x0,y0,x1,y1);var t=r.gameObject.AddComponent<TextMeshProUGUI>();t.font=font;t.text=text;t.fontSize=t.fontSizeMax=size;t.fontSizeMin=Mathf.Max(20,size*.8f);t.enableAutoSizing=true;t.color=color;t.alignment=align;t.raycastTarget=false;t.textWrappingMode=TextWrappingModes.NoWrap;return t;
    }
    static Sprite Art(string name)=>AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/ShopCanva/Art/"+name+".png");
    static RectTransform Node(string name,Transform parent){var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);return r;}
    static void Stretch(RectTransform r)=>Rect(r,0,0,1,1);
    static void Rect(Transform t,float x0,float y0,float x1,float y1){var r=(RectTransform)t;r.anchorMin=new Vector2(x0,y0);r.anchorMax=new Vector2(x1,y1);r.offsetMin=r.offsetMax=Vector2.zero;r.localScale=Vector3.one;}
}
#endif
