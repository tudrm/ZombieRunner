#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

// Authoring tools edit the two existing scenes. No runtime-generated replacement menu.
public static class ShopAuthoring
{
    const string CatalogPath = "Assets/Settings/WeaponCatalog.asset";
    static TMP_FontAsset font;
    static readonly Color White = new Color(.92f, .93f, .89f);
    static readonly Color Muted = new Color(.57f, .63f, .65f);
    static readonly Color Panel = new Color(.035f, .05f, .055f, .95f);
    static readonly Color Red = new Color(.76f, .18f, .16f);

    [MenuItem("Tools/ZombieRunner/Build Shop in Existing Scenes")]
    public static void Setup()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play Mode first.");
        if (EditorSceneManager.GetActiveScene().isDirty) throw new InvalidOperationException("Save current scene before setup.");
        Directory.CreateDirectory("Assets/Settings/WeaponThumbnails");
        AssetDatabase.Refresh();
        var catalog = BuildCatalog();
        var menu = EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
        font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
        if (font == null) font = TMP_Settings.defaultFontAsset;
        GameObject canvas = Find("Canvas");
        var manager = Find("ShopManager").GetComponent<ShopManager>();
        manager.catalog = catalog;
        var panel = Find("Panel_Shop");
        manager.shopPanel = panel;
        panel.SetActive(true);
        var panelRect = (RectTransform)panel.transform;
        panelRect.localScale = Vector3.one;
        Rect(panelRect, 0, 0, 1, 1);
        var bg = panel.GetComponent<Image>();
        var art = panel.transform.Find("ShopBackground") as RectTransform;
        if (art == null) art = Box("ShopBackground", panel.transform, Color.white);
        art.SetAsFirstSibling();
        var artImage = art.GetComponent<Image>();
        artImage.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Poze/PozaShop.png");
        artImage.color = new Color(.28f, .31f, .31f);
        artImage.raycastTarget = false;
        var fit = art.GetComponent<AspectRatioFitter>();
        if (fit == null) fit = art.gameObject.AddComponent<AspectRatioFitter>();
        fit.aspectRatio = artImage.sprite.rect.width / artImage.sprite.rect.height;
        fit.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
        bg.sprite = null; bg.color = new Color(.025f,.035f,.04f);
        // Reuse the existing category buttons, wallet and back button.
        var oldTabs = new[] { Find("Btn_Category_Pistols"), Find("Btn_Category_SMGs"), Find("Btn_Category_Rifles") };
        var back = Find("Btn_ExitToMenu");
        var coinsIcon = Find("Icon_Coins");
        var gemsIcon = Find("Icon_Gems");
        var coins = manager.textMonede;
        var gems = manager.textGemuri;
        foreach (var tab in oldTabs) tab.transform.SetParent(panel.transform, false);
        back.transform.SetParent(panel.transform, false);
        coins.transform.SetParent(panel.transform, false);
        gems.transform.SetParent(panel.transform, false);
        coinsIcon.transform.SetParent(panel.transform, false);
        gemsIcon.transform.SetParent(panel.transform, false);
        var oldShelf = panel.transform.Find("Raft_Categorii");
        if (oldShelf != null) oldShelf.gameObject.SetActive(false);
        var previous = panel.transform.Find("ArmorySafeArea");
        if (previous != null) Object.DestroyImmediate(previous.gameObject);
        manager.safeArea = Node("ArmorySafeArea", panel.transform);
        Rect(manager.safeArea, 0, 0, 1, 1);
        manager.header = Box("Header", manager.safeArea, Panel);
        manager.categories = Node("Categories", manager.safeArea);
        manager.collection = Box("Collection", manager.safeArea, Panel);
        manager.details = Box("WeaponDetails", manager.safeArea, Panel);
        manager.actionBar = Box("ActionBar", manager.safeArea, Panel);
        Rect(manager.header, .035f,.89f,.965f,.985f);
        Rect(manager.categories,.035f,.815f,.965f,.875f);
        Rect(manager.collection,.035f,.115f,.965f,.385f);
        Rect(manager.details,.035f,.405f,.965f,.795f);
        Rect(manager.actionBar,.035f,.025f,.965f,.095f);

        Text("Eyebrow", manager.header, "ZOMBIE RUNNER  /  FIELD SUPPLY", 20, Muted, .025f,.72f,.62f,.95f);
        Text("Heading", manager.header, "ARMORY", 58, White, .025f,.2f,.57f,.75f, FontStyles.Bold);
        manager.equippedText = Text("Equipped", manager.header, "LOADOUT  /  SIDEARM", 19, Muted, .025f,.02f,.62f,.23f);
        Reparent(coinsIcon, manager.header,.64f,.57f,.71f,.88f);
        Reparent(gemsIcon, manager.header,.64f,.12f,.71f,.43f);
        coinsIcon.GetComponent<Image>().preserveAspect = true;
        gemsIcon.GetComponent<Image>().preserveAspect = true;
        Reparent(coins.gameObject, manager.header,.72f,.57f,.92f,.88f);
        Reparent(gems.gameObject, manager.header,.72f,.12f,.92f,.43f);
        StyleText(coins, 28, new Color(.96f,.79f,.42f));
        StyleText(gems, 26, new Color(.62f,.81f,.95f));
        back.transform.SetParent(manager.header, false);
        Rect((RectTransform)back.transform,.935f,.2f,.995f,.8f);
        ButtonStyle(back, new Color(.16f,.19f,.2f), "X", 30);
        SetClick(back.GetComponent<Button>(), manager.CloseShop);

        manager.categoryButtons = new Button[3];
        string[] labels = { "PISTOLS  /  10", "SMGs  /  05", "RIFLES  /  05" };
        for (int i=0;i<3;i++)
        {
            Reparent(oldTabs[i],manager.categories,i/3f+.006f,0,(i+1)/3f-.006f,1);
            ButtonStyle(oldTabs[i], i==0?Red:new Color(.1f,.13f,.14f), labels[i], 25);
            manager.categoryButtons[i]=oldTabs[i].GetComponent<Button>();
            ClearClick(manager.categoryButtons[i]);
        }
        manager.collectionText=Text("CollectionTitle",manager.collection,"YOUR ARSENAL",22,Muted,.03f,.9f,.97f,.99f);
        var scrollRoot=Node("WeaponScroll",manager.collection);
        Rect(scrollRoot,.02f,.025f,.98f,.87f);
        var scroll=scrollRoot.gameObject.AddComponent<ScrollRect>();
        var viewport=Box("Viewport",scrollRoot,new Color(0,0,0,0));
        Rect(viewport,0,0,1,1);
        viewport.gameObject.AddComponent<RectMask2D>();
        var content=Node("WeaponCards",viewport);
        content.anchorMin=new Vector2(0,1); content.anchorMax=Vector2.one; content.pivot=new Vector2(.5f,1);
        content.anchoredPosition=Vector2.zero; content.sizeDelta=Vector2.zero;
        var grid=content.gameObject.AddComponent<GridLayoutGroup>();
        grid.cellSize=new Vector2(450,160);grid.spacing=new Vector2(12,12);grid.padding=new RectOffset(8,8,8,8);
        grid.constraint=GridLayoutGroup.Constraint.FixedColumnCount;grid.constraintCount=2;
        var fitter=content.gameObject.AddComponent<ContentSizeFitter>();fitter.verticalFit=ContentSizeFitter.FitMode.PreferredSize;
        scroll.viewport=viewport;scroll.content=content;scroll.horizontal=false;scroll.vertical=true;
        scroll.movementType=ScrollRect.MovementType.Clamped;scroll.scrollSensitivity=35;
        manager.weaponScroll=scroll;manager.cardContent=content;manager.cardGrid=grid;
        var card=Box("WeaponCardTemplate",content,new Color(.075f,.095f,.1f));
        card.gameObject.AddComponent<Button>();
        var thumb=Box("Thumbnail",card,Color.white).GetComponent<Image>();
        Rect(thumb.rectTransform,.01f,.18f,.4f,.95f);thumb.raycastTarget=false;
        Text("Name",card,"SIDEARM",27,White,.43f,.57f,.97f,.92f,FontStyles.Bold);
        Text("Stats",card,"25 DMG / 180 RPM",22,Muted,.43f,.32f,.98f,.58f);
        Text("Ownership",card,"OWNED",20,new Color(.94f,.76f,.41f),.43f,.06f,.98f,.32f);
        card.gameObject.SetActive(false);manager.cardTemplate=card.gameObject;

        manager.tierText=Text("Tier",manager.details,"PISTOL / TIER 01 / SEMI-AUTO",20,new Color(.85f,.45f,.4f),.04f,.94f,.96f,.995f);
        var display=Node("ModelPreview",manager.details).gameObject.AddComponent<RawImage>();
        Rect(display.rectTransform,.02f,.43f,.98f,.93f);
        display.color=Color.white;
        var preview=display.gameObject.AddComponent<WeaponPreview>();preview.display=display;manager.preview=preview;
        Text("PreviewHint",display.transform,"360°  /  DRAG TO ROTATE",22,Muted,.04f,.02f,.97f,.12f);
        manager.titleText=Text("WeaponName",manager.details,"SIDEARM",38,White,.04f,.355f,.96f,.435f,FontStyles.Bold);
        manager.descriptionText=Text("Description",manager.details,"A reliable companion for your first runs.",24,Muted,.04f,.29f,.96f,.355f);
        Stat(manager.details,0,"DAMAGE / HIT",out manager.damageText,out manager.damageCompare,out manager.damageFill);
        Stat(manager.details,1,"FIRE RATE / RPM",out manager.rateText,out manager.rateCompare,out manager.rateFill);
        Stat(manager.details,2,"DAMAGE / SEC",out manager.dpsText,out manager.dpsCompare,out manager.dpsFill);
        manager.statusText=Text("Status",manager.details,"Drag to inspect. Stats compared with your equipped weapon.",22,Muted,.04f,.015f,.96f,.075f);
        manager.priceText=Text("Price",manager.actionBar,"OWNED / READY TO DEPLOY",26,new Color(.94f,.76f,.41f),.03f,.46f,.49f,.88f,FontStyles.Bold);
        Text("EarnHint",manager.actionBar,"EARN 25 COINS PER ZOMBIE",22,Muted,.03f,.12f,.49f,.45f);
        var buy=Box("PurchaseOrEquip",manager.actionBar,Red);
        Rect(buy,.52f,.14f,.975f,.86f);
        manager.purchaseButton=buy.gameObject.AddComponent<Button>();
        manager.actionText=Text("Label",buy,"EQUIPPED",28,White,.02f,.1f,.98f,.9f,FontStyles.Bold);
        manager.actionText.alignment=TextAlignmentOptions.Center;

        SetClick(Find("Btn_Shop").GetComponent<Button>(),manager.OpenShop);
        SetClick(Find("Btn_Locker").GetComponent<Button>(),manager.OpenShop);
        var main = canvas.GetComponent<MainMenuManager>();
        var controls = canvas.transform.Find("MenuSafeArea") as RectTransform;
        if (controls == null) controls = Node("MenuSafeArea",canvas.transform);
        main.menuSafeArea = controls;
        Rect(controls,0,0,1,1);
        main.menuArtwork = (RectTransform)Find("MainMenu").transform;
        main.menuArtwork.GetComponent<Image>().preserveAspect = true;
        main.playButton = (RectTransform)Find("Btn_Play").transform;
        main.shopButton = (RectTransform)Find("Btn_Shop").transform;
        main.lockerButton = (RectTransform)Find("Btn_Locker").transform;
        main.settingsButton = (RectTransform)Find("Btn_Settings").transform;
        foreach (var button in new[] {main.playButton, main.shopButton, main.lockerButton, main.settingsButton})
        { button.SetParent(controls,false); button.GetComponent<Image>().preserveAspect = true; }
        Rect(main.menuArtwork,0,0,1,1);
        Rect(main.playButton,.18f,.255f,.82f,.33f);
        Rect(main.shopButton,.18f,.165f,.49f,.235f);
        Rect(main.lockerButton,.51f,.165f,.82f,.235f);
        Rect(main.settingsButton,.33f,.075f,.67f,.145f);
        panel.transform.SetAsLastSibling();
        main.loadingPanel.transform.SetAsLastSibling();
        var menuCamera = Find("Main Camera").GetComponent<Camera>();
        menuCamera.clearFlags = CameraClearFlags.SolidColor;
        menuCamera.backgroundColor = new Color(.025f,.035f,.04f);
        EditorUtility.SetDirty(main);
        // Preserve the original menu and loading artwork; shop opens through its existing buttons.
        Find("Panel_Loading").SetActive(false);
        panel.SetActive(false);
        EditorUtility.SetDirty(manager);
        EditorSceneManager.SaveScene(menu);

        var game=EditorSceneManager.OpenScene("Assets/Scenes/ScenaJoc.unity");
        var weaponManager=Object.FindFirstObjectByType<WeaponManager>();
        if (weaponManager==null)throw new InvalidOperationException("Player WeaponManager missing.");
        weaponManager.catalog=catalog;
        ClearClick(Find("ButonTrage").GetComponent<Button>());
        EditorUtility.SetDirty(weaponManager);
        EditorSceneManager.SaveScene(game);
        EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
        AssetDatabase.SaveAssets();
        Debug.Log("ZombieRunner Shop ready: 20 weapons, 3 categories, existing scenes updated.");
    }

    static WeaponCatalog BuildCatalog()
    {
        var catalog=AssetDatabase.LoadAssetAtPath<WeaponCatalog>(CatalogPath);
        // Keep Inspector balancing and existing references when rebuilding only the UI.
        if (catalog != null && catalog.weapons.Length > 0) return catalog;
        if(catalog==null){catalog=ScriptableObject.CreateInstance<WeaponCatalog>();AssetDatabase.CreateAsset(catalog,CatalogPath);}
        var all=new List<WeaponDefinition>();
        string[][] names={new[]{"Sidearm","Sentinel","Outrider","Vanguard","Ironclad","Warden","Revenant","Executioner","Nightfall","Last Word"},
            new[]{"Scavenger","Hornet","Ravager","Ghost","Tempest"},
            new[]{"Ranger","Marauder","Reclaimer","Warlord","Oblivion"}};
        int[][] damage={new[]{25,28,32,36,40,46,52,60,70,85},new[]{18,23,29,36,44},new[]{34,43,54,67,82}};
        int[][] rpm={new[]{180,200,220,240,260,280,300,330,360,400},new[]{420,480,560,650,750},new[]{300,360,420,500,600}};
        int[][] prices={new[]{0,750,1500,2600,4200,6500,9500,14000,20000,30000},new[]{1200,3200,6500,12000,21000},new[]{2200,5000,10000,18000,32000}};
        string[] dirs={"Assets/Low Poly Pistol Weapon Pack 1/Prefabs/Weapons/","Assets/Low Poly SMG Weapon Pack 2/Prefabs/Weapons/","Assets/Low Poly AR Weapon Pack 3/Prefabs/Weapons/"};
        string[] prefixes={"Pistol_","SMG_","AR_"};char[] starts={'A','F','L'};
        for(int category=0;category<3;category++)
        for(int i=0;i<names[category].Length;i++)
        {
            string id=prefixes[category].ToLowerInvariant()+(char)(starts[category]+i);
            var w=new WeaponDefinition{id=id,displayName=names[category][i].ToUpperInvariant(),category=(WeaponCategory)category,tier=i+1,
                damage=damage[category][i],roundsPerMinute=rpm[category][i],price=prices[category][i],
                description=category==0?"Precision in your hands. Every tap counts.":category==1?"Close-range pressure. Hold the trigger and clear the street.":"Heavy firepower for the waves that refuse to stop.",
                modelPrefab=AssetDatabase.LoadAssetAtPath<GameObject>(dirs[category]+prefixes[category]+(char)(starts[category]+i)+".prefab")};
            if(w.modelPrefab==null)throw new Exception("Missing model "+id);
            w.thumbnail=Thumbnail(w);
            all.Add(w);
        }
        catalog.weapons=all.ToArray();EditorUtility.SetDirty(catalog);AssetDatabase.SaveAssets();
        return catalog;
    }

    static Sprite Thumbnail(WeaponDefinition w)
    {
        string path="Assets/Settings/WeaponThumbnails/"+w.id+".png";
        var preview=new PreviewRenderUtility();
        try
        {
            preview.BeginStaticPreview(new Rect(0,0,512,256));
            var model=Object.Instantiate(w.modelPrefab);
            preview.AddSingleGO(model);
            model.transform.position=Vector3.zero;model.transform.rotation=Quaternion.Euler(-12,-65,0);
            var renderers=model.GetComponentsInChildren<Renderer>();
            Bounds b=renderers[0].bounds;foreach(var r in renderers)b.Encapsulate(r.bounds);
            preview.camera.orthographic=true;
            preview.camera.orthographicSize=Mathf.Max(b.extents.y,b.extents.x/2)*1.3f;
            preview.camera.nearClipPlane=.01f;preview.camera.farClipPlane=30;
            preview.camera.transform.position=b.center+Vector3.back*8;preview.camera.transform.rotation=Quaternion.identity;
            preview.camera.clearFlags=CameraClearFlags.SolidColor;preview.camera.backgroundColor=new Color(.075f,.095f,.1f,0);
            preview.lights[0].intensity=2.3f;preview.lights[0].transform.rotation=Quaternion.Euler(35,-35,0);
            preview.lights[1].intensity=1.6f;preview.lights[1].transform.rotation=Quaternion.Euler(20,145,0);
            preview.ambientColor=new Color(.35f,.35f,.35f);
            preview.Render(true);
            var tex=preview.EndStaticPreview();
            File.WriteAllBytes(path,tex.EncodeToPNG());Object.DestroyImmediate(tex);
        }
        finally {preview.Cleanup();}
        AssetDatabase.ImportAsset(path);
        var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Sprite;
        importer.spriteImportMode=SpriteImportMode.Single;importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.maxTextureSize=512;
        importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    static void Stat(Transform parent,int index,string label,out TextMeshProUGUI value,out TextMeshProUGUI compare,out Image fill)
    {
        float left=.04f+index*.315f;
        var box=Box("Stat_"+index,parent,new Color(.09f,.115f,.12f));
        Rect(box,left,.085f,left+.295f,.28f);
        Text("Label",box,label,22,Muted,.07f,.74f,.95f,.96f);
        value=Text("Value",box,"25",38,White,.07f,.32f,.94f,.78f,FontStyles.Bold);
        compare=Text("Compare",box,"EQUAL",19,Muted,.07f,.09f,.96f,.33f);
        var bar=Box("Track",box,new Color(.18f,.21f,.22f));Rect(bar,.07f,.035f,.93f,.065f);
        fill=Box("Fill",bar,Red).GetComponent<Image>();Rect(fill.rectTransform,0,0,1,1);
        fill.sprite=AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");fill.type=Image.Type.Filled;fill.fillMethod=Image.FillMethod.Horizontal;fill.fillAmount=.25f;
    }

    static GameObject Find(string name)
    {
        foreach(var root in EditorSceneManager.GetActiveScene().GetRootGameObjects())
        foreach(var t in root.GetComponentsInChildren<Transform>(true))if(t.name==name)return t.gameObject;
        throw new Exception("Object not found: "+name);
    }
    static RectTransform Node(string name,Transform parent)
    {
        var go=new GameObject(name,typeof(RectTransform));go.layer=5;go.transform.SetParent(parent,false);return (RectTransform)go.transform;
    }
    static RectTransform Box(string name,Transform parent,Color color)
    {
        var rect=Node(name,parent);var img=rect.gameObject.AddComponent<Image>();img.color=color;return rect;
    }
    static TextMeshProUGUI Text(string name,Transform parent,string value,float size,Color color,float x0,float y0,float x1,float y1,FontStyles style=FontStyles.Normal)
    {
        var rect=Node(name,parent);Rect(rect,x0,y0,x1,y1);var t=rect.gameObject.AddComponent<TextMeshProUGUI>();
        t.font=font;t.text=value;t.fontSize=size;t.fontStyle=style;t.color=color;t.raycastTarget=false;
        t.enableAutoSizing=true;t.fontSizeMin=size*.65f;t.fontSizeMax=size;t.textWrappingMode=TextWrappingModes.NoWrap;
        t.overflowMode=TextOverflowModes.Ellipsis;t.alignment=TextAlignmentOptions.MidlineLeft;return t;
    }
    static void StyleText(TextMeshProUGUI text,float size,Color color)
    {
        text.font=font;text.fontSize=size;text.enableAutoSizing=true;text.fontSizeMin=16;text.fontSizeMax=size;text.color=color;
        text.alignment=TextAlignmentOptions.MidlineLeft;text.textWrappingMode=TextWrappingModes.NoWrap;
    }
    static void Rect(RectTransform r,float x0,float y0,float x1,float y1)
    {
        r.localScale=Vector3.one;r.localRotation=Quaternion.identity;r.anchorMin=new Vector2(x0,y0);r.anchorMax=new Vector2(x1,y1);
        r.offsetMin=r.offsetMax=Vector2.zero;r.pivot=new Vector2(.5f,.5f);
    }
    static void Reparent(GameObject go,Transform parent,float x0,float y0,float x1,float y1)
    {
        go.transform.SetParent(parent,false);Rect((RectTransform)go.transform,x0,y0,x1,y1);
    }
    static void ButtonStyle(GameObject go,Color color,string label,float size)
    {
        var image=go.GetComponent<Image>();image.sprite=null;image.color=color;image.preserveAspect=false;
        var button=go.GetComponent<Button>();button.transition=Selectable.Transition.ColorTint;
        var text=go.transform.Find("Label")?.GetComponent<TextMeshProUGUI>();
        if(text==null)text=Text("Label",go.transform,label,size,White,.025f,.05f,.975f,.95f,FontStyles.Bold);
        text.text=label;text.alignment=TextAlignmentOptions.Center;
    }
    static void ClearClick(Button button) { button.onClick=new Button.ButtonClickedEvent();EditorUtility.SetDirty(button); }
    static void SetClick(Button button,UnityEngine.Events.UnityAction action)
    {
        ClearClick(button);UnityEventTools.AddPersistentListener(button.onClick,action);
    }

    [MenuItem("Tools/ZombieRunner/Validate Weapon Catalog")]
    public static void Validate()
    {
        var c=AssetDatabase.LoadAssetAtPath<WeaponCatalog>(CatalogPath);
        if(c==null||c.weapons.Length!=20)throw new Exception("Expected 20 weapons.");
        var ids=new HashSet<string>();
        foreach(WeaponCategory cat in Enum.GetValues(typeof(WeaponCategory)))
        {
            int lastDamage=0,lastRate=0,lastPrice=-1;
            foreach(var w in c.weapons.Where(w=>w.category==cat))
            {
                if(!ids.Add(w.id)||w.modelPrefab==null||w.thumbnail==null||w.damage<=lastDamage||w.roundsPerMinute<=lastRate||w.price<=lastPrice)
                    throw new Exception("Invalid progression: "+w.id);
                lastDamage=w.damage;lastRate=w.roundsPerMinute;lastPrice=w.price;
            }
        }
        Debug.Log("Weapon catalog validated: unique IDs, all models/previews, increasing damage/rate/prices.");
    }
}
#endif
