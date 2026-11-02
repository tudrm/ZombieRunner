#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object=UnityEngine.Object;

// Applies only the approved Shop presentation. Never calls ShopAuthoring.Setup:
// that older tool also touches Settings, audio, ScenaJoc and catalog defaults.
public static class CanvaShopAuthoring
{
    const string Root="Assets/UI/ShopCanva/";
    static TMP_FontAsset heading,body;
    static Sprite frame,coin,gem;
    static readonly Color Ink=ShopPresentation.Ink, White=ShopPresentation.White, Lime=ShopPresentation.Lime, Muted=ShopPresentation.Muted;

    [MenuItem("Tools/ZombieRunner/Apply Approved Canva Shop (MainMenu Only)")]
    public static void Apply()
    {
        var scene=EditorSceneManager.GetActiveScene();
        if(EditorApplication.isPlaying||scene.path!="Assets/Scenes/MainMenu.unity"||scene.isDirty)
            throw new InvalidOperationException("Open the saved MainMenu scene outside Play Mode first.");
        var shop=Object.FindFirstObjectByType<ShopManager>();
        if(shop.presentation!=null)throw new InvalidOperationException("Canva Shop is already installed; edit its components directly.");
        ImportArt();
        heading=CreateFont();body=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
        frame=MakeFrame();coin=Art("Coin");gem=Art("Gem");
        // Record a recoverable UI prefab as well as the original full scene in Backups.
        Directory.CreateDirectory(Root+"Backup");
        PrefabUtility.SaveAsPrefabAsset(shop.shopPanel,Root+"Backup/OriginalShop.prefab");
        var view=shop.gameObject.AddComponent<ShopPresentation>();view.shop=shop;shop.presentation=view;
        view.coinButtonSprite=Art("BuyCoins");view.gemButtonSprite=Art("BuyGems");view.equippedButtonSprite=MakeFrame(true);view.activeTabSprite=Art("TabActive");view.inactiveTabSprite=frame;
        shop.shopPanel.GetComponent<Image>().color=new Color32(7,12,16,255);
        var background=shop.shopPanel.transform.Find("ShopBackground");if(background!=null)background.gameObject.SetActive(false);
        foreach(var t in shop.safeArea.GetComponentsInChildren<TextMeshProUGUI>(true))
        {t.font=body;t.color=White;t.enableAutoSizing=false;t.raycastTarget=false;}
        Header(shop);
        for(int i=0;i<3;i++)
        {
            var b=shop.categoryButtons[i];Style(b,frame,new Color(.08f,.1f,.12f));
            var label=b.GetComponentInChildren<TextMeshProUGUI>();Font(label,heading,32);label.alignment=TextAlignmentOptions.Center;
            var line=b.transform.Find("Line");Rect(line,.18f,0,.82f,.04f);line.GetComponent<Image>().color=Lime;
        }
        Hero(shop);
        Cards(shop);
        Gems(shop,view);
        var notice=shop.noticePanel.transform.Find("Message");Panel(notice,Ink);
        Font(shop.noticeTitle,heading,40);shop.noticeTitle.color=Lime;
        Font(shop.noticeText,body,25);shop.noticeText.textWrappingMode=TextWrappingModes.Normal;
        var close=notice.Find("Close").GetComponent<Button>();Style(close,Art("BuyGems"),Color.white);
        Font(close.GetComponentInChildren<TextMeshProUGUI>(),heading,30);
        foreach(var image in shop.shopPanel.GetComponentsInChildren<Image>(true))image.pixelsPerUnitMultiplier=shop.shopPanel.GetComponentInParent<Canvas>().referencePixelsPerUnit/100f;
        shop.Layout();shop.shopPanel.SetActive(false);
        EditorUtility.SetDirty(shop);EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
        Debug.Log("Approved Canva Shop applied to MainMenu only. Economy and saved data unchanged.");
    }

    static void Header(ShopManager s)
    {
        s.header.GetComponent<Image>().color=new Color32(7,12,16,255);
        var title=s.header.Find("Title").GetComponent<TextMeshProUGUI>();title.text="SHOP";Font(title,heading,40);Rect(title.transform,0,.03f,.31f,.55f);
        Label("Brand",s.header,"ZOMBIE<color=#BEF24A>RUNNER</color>",heading,29,White,0,.57f,.38f,1);
        var wallets=new[]{s.header.Find("Coins"),s.header.Find("Gems")};
        for(int i=0;i<2;i++)
        {
            var r=wallets[i];Rect(r,.39f+i*.235f,.1f,.615f+i*.235f,.9f);Style(r.GetComponent<Button>(),frame,new Color(.35f,.42f,.48f));
            var icon=r.GetComponentsInChildren<Image>().First(x=>x.transform!=r);icon.sprite=i==0?coin:gem;icon.color=Color.white;
            Rect(icon.transform,.04f,.25f,.27f,.75f);icon.preserveAspect=true;icon.raycastTarget=false;
            var text=i==0?s.textMonede:s.textGemuri;Font(text,heading,28);Rect(text.transform,.29f,.12f,.82f,.88f);text.alignment=TextAlignmentOptions.MidlineLeft;
            text.enableAutoSizing=true;text.fontSizeMin=20;text.fontSizeMax=28;
            Label("Add",r,"+",body,28,Muted,.82f,.12f,.98f,.88f,TextAlignmentOptions.Center);
        }
        var exit=s.header.Find("Btn_ExitToMenu");Rect(exit,.88f,.1f,1,.9f);Style(exit.GetComponent<Button>(),null,new Color(.07f,.09f,.12f));
        var x=exit.GetComponentInChildren<TextMeshProUGUI>();x.text="×";Font(x,body,40);
        var rule=s.header.Find("Rule");rule.GetComponent<Image>().color=new Color(.18f,.22f,.25f);
    }

    static void Hero(ShopManager s)
    {
        Panel(s.details,new Color(.1f,.14f,.08f));
        // Crop only the city on the left of the exact Canva artwork, never its static gun.
        var city=Node("CanvaCityAtmosphere",s.details);Rect(city,.01f,.22f,.99f,.99f);city.SetAsFirstSibling();
        var raw=city.gameObject.AddComponent<RawImage>();raw.texture=AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"Art/Hero.png");raw.uvRect=new Rect(.03f,.04f,.28f,.92f);raw.color=new Color(.7f,.7f,.7f,.48f);raw.raycastTarget=false;
        Rect(s.tierText.transform,.04f,.87f,.66f,.96f);Font(s.tierText,heading,24);s.tierText.color=Muted;
        Rect(s.titleText.transform,.04f,.73f,.60f,.89f);Font(s.titleText,heading,48);s.titleText.color=Lime;s.titleText.enableAutoSizing=true;s.titleText.fontSizeMin=36;s.titleText.fontSizeMax=48;
        Rect(s.preview.display.transform,.34f,.47f,.98f,.91f);
        s.preview.transparentBackground=true;s.preview.rimColor=new Color(1,.61f,.22f);
        var hint=s.preview.transform.Find("RotateHint");hint.SetParent(s.details,false);Rect(hint,.04f,.60f,.45f,.68f);var hintText=hint.GetComponent<TextMeshProUGUI>();hintText.text="360°  DRAG TO ROTATE";Font(hintText,heading,22);hintText.color=Muted;hintText.alignment=TextAlignmentOptions.MidlineLeft;
        var values=new[]{s.damageText,s.rateText,s.dpsText};var compares=new[]{s.damageCompare,s.rateCompare,s.dpsCompare};var meters=new[]{s.damageFill,s.rateFill,s.dpsFill};
        for(int i=0;i<3;i++)
        {
            var stat=values[i].transform.parent;Rect(stat,.04f+i*.31f,.30f,.33f+i*.31f,.47f);Panel(stat,new Color(.3f,.36f,.39f));
            var label=stat.Find("Caption").GetComponent<TextMeshProUGUI>();label.text=new[]{"DAMAGE","RPM","DPS"}[i];Font(label,heading,22);label.color=Muted;label.alignment=TextAlignmentOptions.Center;Rect(label.transform,.02f,.60f,.98f,.98f);
            Rect(values[i].transform,.05f,.03f,.75f,.66f);Font(values[i],heading,39);values[i].color=Lime;values[i].alignment=TextAlignmentOptions.Center;
            Font(compares[i],heading,21);Rect(compares[i].transform,.72f,.1f,.96f,.58f);
            meters[i].gameObject.SetActive(false);
        }
        Style(s.purchaseButton,Art("BuyCoins"),Color.white);Style(s.gemPurchaseButton,Art("BuyGems"),Color.white);
        Font(s.actionText,heading,32);Font(s.gemActionText,heading,32);
        Rect(s.actionText.transform,.16f,.08f,.97f,.92f);Rect(s.gemActionText.transform,.16f,.08f,.97f,.92f);
        Icon("CurrencyIcon",s.purchaseButton.transform,coin,.06f,.30f,.20f,.70f);
        Icon("CurrencyIcon",s.gemPurchaseButton.transform,gem,.06f,.30f,.20f,.70f);
        Font(s.descriptionText,body,20);Rect(s.descriptionText.transform,0,0,1,.19f);s.descriptionText.alignment=TextAlignmentOptions.Center;s.descriptionText.color=Muted;
        Font(s.statusText,body,22);s.statusText.color=Lime;Rect(s.statusText.transform,.04f,.005f,.96f,.06f);s.statusText.alignment=TextAlignmentOptions.Center;
    }

    static void Cards(ShopManager s)
    {
        Font(s.collectionText,heading,34);
        var card=s.cardTemplate;
        Style(card.GetComponent<Button>(),Art("Card"),Color.white);
        var thumb=card.transform.Find("Thumbnail");Rect(thumb,.025f,.12f,.46f,.88f);
        var title=card.transform.Find("Name").GetComponent<TextMeshProUGUI>();Font(title,heading,28);Rect(title.transform,.48f,.56f,.96f,.92f);
        var ownership=card.transform.Find("Ownership").GetComponent<TextMeshProUGUI>();Font(ownership,heading,24);ownership.textWrappingMode=TextWrappingModes.NoWrap;
        var badge=Node("OwnershipBadge",card.transform);Rect(badge,.48f,.08f,.96f,.39f);badge.SetSiblingIndex(2);var badgeImage=badge.gameObject.AddComponent<Image>();badgeImage.sprite=frame;badgeImage.type=Image.Type.Sliced;badgeImage.raycastTarget=false;
        var selected=card.transform.Find("Selected");Rect(selected,0,0,1,1);var mark=selected.GetComponent<Image>();mark.sprite=frame;mark.type=Image.Type.Sliced;mark.color=Lime;mark.raycastTarget=false;
        badgeImage.sprite=null;
        var view=card.AddComponent<ShopWeaponCard>();view.frame=mark;view.badge=badgeImage;view.ownership=ownership;
        Directory.CreateDirectory(Root+"Prefabs");
        // The template is a real prefab; all category instances reuse its layout.
        var prefab=PrefabUtility.SaveAsPrefabAssetAndConnect(card,Root+"Prefabs/WeaponCard.prefab",InteractionMode.AutomatedAction);
        s.cardTemplate=card;
    }

    static void Gems(ShopManager s,ShopPresentation view)
    {
        s.gemGrid.enabled=false; // Last offer spans both columns; presentation lays out five existing offers.
        var headingText=s.gemsSection.Find("Heading").GetComponent<TextMeshProUGUI>();Font(headingText,heading,42);ShopManager.Top(headingText.rectTransform,52,18,220,52);
        var icon=Icon("GemHeading",s.gemsSection,gem,0,0,1,1);ShopManager.Top(icon.rectTransform,0,27,40,40);
        var subtitle=s.gemsSection.Find("Subtitle").GetComponent<TextMeshProUGUI>();subtitle.text="Upgrade your arsenal.";Font(subtitle,body,24);ShopManager.Top(subtitle.rectTransform,0,76,660,36);subtitle.color=Muted;
        view.offerRects=new RectTransform[s.gemStore.offers.Length];
        for(int i=0;i<s.gemStore.offers.Length;i++)
        {
            var offer=s.gemStore.offers[i];var t=s.gemCards.Find("Gems_"+offer.gems);view.offerRects[i]=(RectTransform)t;
            Style(t.GetComponent<Button>(),Art("Card"),new Color(1,.62f,.7f));
            var image=t.Find("Gem").GetComponent<Image>();image.sprite=Art("Gems"+offer.gems);image.color=Color.white;
            bool last=i==s.gemStore.offers.Length-1;
            Rect(image.transform,.03f,.34f,last?.45f:.38f,.93f);
            var amount=t.Find("Amount").GetComponent<TextMeshProUGUI>();amount.text=offer.gems.ToString("N0")+" Gems";Font(amount,heading,30);Rect(amount.transform,last?.48f:.40f,.56f,.97f,.92f);amount.alignment=TextAlignmentOptions.MidlineLeft;
            var label=t.Find("Pack").GetComponent<TextMeshProUGUI>();label.text=offer.label;Font(label,heading,21);label.color=Muted;Rect(label.transform,last?.48f:.40f,.37f,.97f,.57f);label.alignment=TextAlignmentOptions.MidlineLeft;
            var price=t.Find("Price");Rect(price,last?.48f:.06f,.035f,.94f,.34f);var priceImage=price.GetComponent<Image>();priceImage.sprite=Art("BuyGems");priceImage.type=Image.Type.Sliced;priceImage.color=Color.white;priceImage.raycastTarget=false;
            Font(price.GetComponentInChildren<TextMeshProUGUI>(),heading,32);
            if(last)Label("BestValue",t,"BEST VALUE",heading,22,Lime,.03f,.04f,.45f,.3f,TextAlignmentOptions.Center);
        }
        view.availability=s.gemsSection.Find("Availability").GetComponent<TextMeshProUGUI>();Font(view.availability,body,22);view.availability.color=Muted;view.availability.textWrappingMode=TextWrappingModes.Normal;
    }

    static Sprite Art(string name)=>AssetDatabase.LoadAssetAtPath<Sprite>(Root+"Art/"+name+".png");
    static void ImportArt()
    {
        AssetDatabase.Refresh();
        foreach(var file in Directory.GetFiles(Root+"Art","*.png"))
        {
            var importer=(TextureImporter)AssetImporter.GetAtPath(file.Replace('\\','/'));
            importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.maxTextureSize=2048;
            string name=Path.GetFileNameWithoutExtension(file);
            if(name=="BuyCoins"||name=="BuyGems")importer.spriteBorder=new Vector4(20,14,20,14);
            if(name=="Card")importer.spriteBorder=new Vector4(16,16,16,16);
            if(name=="TabActive")importer.spriteBorder=new Vector4(20,14,20,14);
            importer.SaveAndReimport();
        }
    }
    static TMP_FontAsset CreateFont()
    {
        string path=Root+"Fonts/BarlowCondensed SDF.asset";
        var existing=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);if(existing!=null)return existing;
        var font=TMP_FontAsset.CreateFontAsset(AssetDatabase.LoadAssetAtPath<Font>(Root+"Fonts/BarlowCondensed-Bold.ttf"),72,8,UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA,1024,1024);
        font.name="BarlowCondensed SDF";font.TryAddCharacters("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789 .,;:/-+%×°€!?()'•");font.atlasPopulationMode=AtlasPopulationMode.Static;
        AssetDatabase.CreateAsset(font,path);AssetDatabase.AddObjectToAsset(font.material,font);foreach(var tex in font.atlasTextures)AssetDatabase.AddObjectToAsset(tex,font);EditorUtility.SetDirty(font);return font;
    }
    static Sprite MakeFrame(bool filled=false)
    {
        string path=Root+"Art/"+(filled?"ButtonNeutral":"Frame")+".png";var texture=new Texture2D(64,64,TextureFormat.RGBA32,false);
        for(int y=0;y<64;y++)for(int x=0;x<64;x++)
        {
            int edge=Mathf.Min(x,y,63-x,63-y);bool inside=x+y>=8&&x+63-y>=8&&63-x+y>=8&&126-x-y>=8;
            bool border=edge<2||x+y<11||x+63-y<11||63-x+y<11||126-x-y<11;
            var fill=filled?Color.Lerp(new Color(.78f,.78f,.78f,1),Color.white,y/63f):Color.clear;
            texture.SetPixel(x,y,inside?(border?Color.white:fill):Color.clear);
        }
        texture.Apply();File.WriteAllBytes(path,texture.EncodeToPNG());Object.DestroyImmediate(texture);AssetDatabase.ImportAsset(path);
        var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.spriteBorder=new Vector4(12,12,12,12);importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.SaveAndReimport();return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }
    static void Panel(Transform t,Color color){var im=t.GetComponent<Image>()??t.gameObject.AddComponent<Image>();im.sprite=frame;im.type=Image.Type.Sliced;im.color=color;im.raycastTarget=false;}
    static void Style(Button b,Sprite sprite,Color color)
    {var image=b.GetComponent<Image>();image.sprite=sprite;image.type=sprite==null?Image.Type.Simple:Image.Type.Sliced;image.color=color;b.targetGraphic=image;var c=b.colors;c.normalColor=Color.white;c.highlightedColor=new Color(.96f,1,.90f);c.pressedColor=new Color(.7f,.77f,.65f);c.disabledColor=new Color(.65f,.65f,.65f,.8f);c.fadeDuration=.09f;b.colors=c;}
    static RectTransform Node(string name,Transform parent){var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);return r;}
    static void Font(TextMeshProUGUI t,TMP_FontAsset font,float size){t.font=font;t.fontSize=size;t.enableAutoSizing=false;t.fontStyle=FontStyles.Normal;t.raycastTarget=false;}
    static TextMeshProUGUI Label(string name,Transform parent,string text,TMP_FontAsset font,float size,Color color,float x0,float y0,float x1,float y1,TextAlignmentOptions align=TextAlignmentOptions.MidlineLeft)
    {var r=Node(name,parent);Rect(r,x0,y0,x1,y1);var t=r.gameObject.AddComponent<TextMeshProUGUI>();Font(t,font,size);t.text=text;t.color=color;t.alignment=align;t.textWrappingMode=TextWrappingModes.NoWrap;return t;}
    static Image Icon(string name,Transform parent,Sprite sprite,float x0,float y0,float x1,float y1){var r=Node(name,parent);Rect(r,x0,y0,x1,y1);var i=r.gameObject.AddComponent<Image>();i.sprite=sprite;i.preserveAspect=true;i.raycastTarget=false;return i;}
    static void Rect(Transform t,float x0,float y0,float x1,float y1){var r=(RectTransform)t;r.anchorMin=new Vector2(x0,y0);r.anchorMax=new Vector2(x1,y1);r.offsetMin=r.offsetMax=Vector2.zero;r.pivot=new Vector2(.5f,.5f);r.localScale=Vector3.one;}
}
#endif
