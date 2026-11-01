#if UNITY_EDITOR
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using Object=UnityEngine.Object;

public static class ShopAuthoring
{
    static TMP_FontAsset body, heading;
    static Color Ink=new Color(.075f,.09f,.08f), Panel=new Color(.10f,.115f,.105f), White=new Color(.91f,.91f,.85f), Muted=new Color(.59f,.62f,.56f), Accent=new Color(.67f,.29f,.23f);
    const string CatalogPath="Assets/Settings/WeaponCatalog.asset";
    [MenuItem("Tools/ZombieRunner/Build Shop in Existing Scenes")]
    public static void Setup()
    {
        if(EditorApplication.isPlaying || EditorSceneManager.GetActiveScene().isDirty)throw new Exception("Stop Play Mode and save the active scene first.");
        body=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
        heading=CreateHeading();
        var mixer=CreateMixer();
        var catalog=AssetDatabase.LoadAssetAtPath<WeaponCatalog>(CatalogPath);
        foreach(var w in catalog.weapons)if(w.gemPrice==0 && w.price>0)w.gemPrice=Mathf.CeilToInt(w.price/25f);
        EditorUtility.SetDirty(catalog);
        var store=AssetDatabase.LoadAssetAtPath<GemStoreCatalog>("Assets/Settings/GemStore.asset");
        if(store==null)
        {
            store=ScriptableObject.CreateInstance<GemStoreCatalog>();
            int[] amounts={200,550,1200,7000,16000}, prices={2,5,10,50,100};
            string[] names={"POCKET STASH","SUPPLY POUCH","SURVIVOR PACK","WAR CHEST","VAULT"};
            store.offers=Enumerable.Range(0,5).Select(i=>new GemOffer{productId="zombierunner.gems."+amounts[i],gems=amounts[i],euroPrice=prices[i],label=names[i]}).ToArray();
            AssetDatabase.CreateAsset(store,"Assets/Settings/GemStore.asset");
        }
        var scene=EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
        var shop=Object.FindFirstObjectByType<ShopManager>();shop.catalog=catalog;shop.gemStore=store;
        var canvas=Find("Canvas");var main=canvas.GetComponent<MainMenuManager>();
        var panel=shop.shopPanel;panel.SetActive(true);
        var tabs=new[]{Find("Btn_Category_Pistols"),Find("Btn_Category_SMGs"),Find("Btn_Category_Rifles")};
        var back=Find("Btn_ExitToMenu");var coin=Find("Icon_Coins");var gem=Find("Icon_Gems");
        foreach(var go in tabs.Concat(new[]{back,coin,gem,shop.textMonede.gameObject,shop.textGemuri.gameObject}))go.transform.SetParent(panel.transform,false);
        var previous=panel.transform.Find("ArmorySafeArea");if(previous!=null)Object.DestroyImmediate(previous.gameObject);
        panel.GetComponent<Image>().color=new Color(.045f,.055f,.047f);
        var art=panel.transform.Find("ShopBackground");
        if(art!=null){var image=art.GetComponent<Image>();image.sprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Poze/MeniuZombieRunner.png");image.color=new Color(.14f,.16f,.13f,.4f);}
        shop.safeArea=Node("ArmorySafeArea",panel.transform);Stretch(shop.safeArea);
        shop.header=Box("Header",shop.safeArea,new Color(.045f,.055f,.047f,.96f));
        Label("Title",shop.header,"SHOP",48,White,0,.05f,.32f,.95f,true);
        var coinButton=Button("Coins",shop.header,new Color(.19f,.18f,.13f));Rect(coinButton.transform,.43f,.2f,.655f,.85f);Click(coinButton,shop.ShowGems);
        var gemButton=Button("Gems",shop.header,new Color(.23f,.12f,.115f));Rect(gemButton.transform,.67f,.2f,.895f,.85f);Click(gemButton,shop.ShowGems);
        Wallet(coin,shop.textMonede,coinButton.transform);Wallet(gem,shop.textGemuri,gemButton.transform);
        back.transform.SetParent(shop.header,false);Rect(back.transform,.915f,.2f,1,.85f);Style(back.GetComponent<Button>(),new Color(.15f,.17f,.145f));
        var oldLabel=back.transform.Find("Label");if(oldLabel!=null)Object.DestroyImmediate(oldLabel.gameObject);
        Label("Label",back.transform,"X",26,White,0,0,1,1,false,TextAlignmentOptions.Center);Click(back.GetComponent<Button>(),shop.CloseShop);
        var rule=Box("Rule",shop.header,new Color(.25f,.27f,.23f));Rect(rule,0,0,1,.012f);
        var scrollRoot=Node("ShopScroll",shop.safeArea);Stretch(scrollRoot);scrollRoot.SetAsFirstSibling();
        var viewport=Box("Viewport",scrollRoot,Color.clear);viewport.gameObject.AddComponent<RectMask2D>();
        var content=Node("Page",viewport);content.anchorMin=new Vector2(0,1);content.anchorMax=Vector2.one;content.pivot=new Vector2(.5f,1);content.anchoredPosition=Vector2.zero;
        shop.pageContent=content;
        var scroll=scrollRoot.gameObject.AddComponent<ScrollRect>();scroll.viewport=viewport;scroll.content=content;scroll.horizontal=false;scroll.vertical=true;scroll.movementType=ScrollRect.MovementType.Clamped;scroll.scrollSensitivity=55;scroll.decelerationRate=.09f;shop.weaponScroll=scroll;
        shop.categories=Node("Categories",content);shop.categoryButtons=new Button[3];
        for(int i=0;i<3;i++)
        {
            tabs[i].transform.SetParent(shop.categories,false);Rect(tabs[i].transform,i/3f+.004f,0,(i+1)/3f-.004f,1);
            var btn=tabs[i].GetComponent<Button>();Style(btn,Panel);btn.onClick=new Button.ButtonClickedEvent();
            foreach(Transform child in tabs[i].transform.Cast<Transform>().ToArray())Object.DestroyImmediate(child.gameObject);
            Label("Label",btn.transform,new[]{"PISTOLS","SMGs","RIFLES"}[i],25,White,0,0,1,1,false,TextAlignmentOptions.Center);
            var line=Box("Line",btn.transform,Accent);Rect(line,.2f,0,.8f,.045f);shop.categoryButtons[i]=btn;
        }
        shop.details=Box("SelectedWeapon",content,new Color(.09f,.105f,.092f,.98f));
        shop.titleText=Label("WeaponName",shop.details,"OUTRIDER",42,White,.04f,.87f,.96f,.98f,true);
        shop.tierText=Label("Tier",shop.details,"SEMI-AUTO / TIER 03",19,Muted,.04f,.82f,.96f,.88f);
        var display=Node("ModelPreview",shop.details).gameObject.AddComponent<RawImage>();Rect(display.transform,.03f,.39f,.97f,.81f);
        var preview=display.gameObject.AddComponent<WeaponPreview>();preview.display=display;shop.preview=preview;
        Label("RotateHint",display.transform,"DRAG TO ROTATE",17,Muted,.02f,0,.98f,.1f,false,TextAlignmentOptions.Right);
        Stat(shop.details,0,"DAMAGE",out shop.damageText,out shop.damageCompare,out shop.damageFill);
        Stat(shop.details,1,"FIRE RATE",out shop.rateText,out shop.rateCompare,out shop.rateFill);
        Stat(shop.details,2,"DPS",out shop.dpsText,out shop.dpsCompare,out shop.dpsFill);
        shop.actionBar=Node("Purchase",shop.details);
        shop.priceText=Label("Caption",shop.actionBar,"UNLOCK WITH",17,Muted,0,.8f,1,1);
        shop.purchaseButton=Button("BuyWithCoins",shop.actionBar,new Color(.44f,.37f,.23f));
        shop.actionText=Label("Label",shop.purchaseButton.transform,"COINS",22,White,.02f,.05f,.98f,.95f,false,TextAlignmentOptions.Center);
        shop.gemPurchaseButton=Button("BuyWithGems",shop.actionBar,Accent);
        shop.gemActionText=Label("Label",shop.gemPurchaseButton.transform,"GEMS",22,White,.02f,.05f,.98f,.95f,false,TextAlignmentOptions.Center);
        shop.descriptionText=Label("PaymentHint",shop.actionBar,"",16,Muted,0,0,1,.18f);
        shop.statusText=Label("Status",shop.details,"",19,new Color(.72f,.8f,.62f),.04f,.005f,.96f,.055f);
        shop.equippedText=Label("Loadout",shop.safeArea,"",18,Muted,0,0,0,0);shop.equippedText.gameObject.SetActive(false);
        shop.collection=Node("Weapons",content);
        shop.collectionText=Label("CollectionTitle",shop.collection,"WEAPONS",23,White,0,0,1,1);
        ShopManager.Top(shop.collectionText.rectTransform,0,0,600,32);
        shop.cardContent=Node("Cards",shop.collection);
        var grid=shop.cardContent.gameObject.AddComponent<GridLayoutGroup>();grid.spacing=new Vector2(12,12);grid.constraint=GridLayoutGroup.Constraint.FixedColumnCount;shop.cardGrid=grid;
        var card=Button("WeaponCardTemplate",shop.cardContent,Panel);
        var thumb=Box("Thumbnail",card.transform,Color.white).GetComponent<Image>();Rect(thumb.transform,.08f,.37f,.92f,.94f);thumb.preserveAspect=true;thumb.raycastTarget=false;
        Label("Name",card.transform,"SIDEARM",23,White,.06f,.19f,.94f,.37f);
        Label("Ownership",card.transform,"750 C / 30 G",19,Muted,.06f,.02f,.94f,.19f);
        var mark=Box("Selected",card.transform,Accent);Rect(mark,0,0,.012f,1);mark.GetComponent<Image>().raycastTarget=false;
        shop.cardTemplate=card.gameObject;card.gameObject.SetActive(false);
        shop.gemsSection=Node("GemStore",content);
        var separator=Box("Rule",shop.gemsSection,new Color(.28f,.27f,.22f));ShopManager.Top(separator,0,0,900,1);separator.anchorMax=new Vector2(1,1);separator.sizeDelta=new Vector2(0,1);
        var gemTitle=Label("Heading",shop.gemsSection,"GEMS",44,White,0,0,1,1,true);ShopManager.Top(gemTitle.rectTransform,0,20,600,50);
        var subtitle=Label("Subtitle",shop.gemsSection,"Unlock weapons with red Gems.",21,Muted,0,0,1,1);ShopManager.Top(subtitle.rectTransform,0,68,650,28);
        shop.gemCards=Node("Offers",shop.gemsSection);shop.gemGrid=shop.gemCards.gameObject.AddComponent<GridLayoutGroup>();shop.gemGrid.spacing=new Vector2(12,12);shop.gemGrid.constraint=GridLayoutGroup.Constraint.FixedColumnCount;
        for(int i=0;i<store.offers.Length;i++)
        {
            int index=i;var offer=store.offers[i];var go=Button("Gems_"+offer.gems,shop.gemCards,new Color(.16f,.105f,.10f));
            var icon=Box("Gem",go.transform,Color.white).GetComponent<Image>();icon.sprite=gem.GetComponent<Image>().sprite;icon.preserveAspect=true;icon.raycastTarget=false;Rect(icon.transform,.36f,.53f,.64f,.83f);
            int bonus=Mathf.RoundToInt((offer.gems/(offer.euroPrice*100f)-1)*100);
            Label("Pack",go.transform,bonus>0?"+"+bonus+"% BONUS":"GEMS",17,Muted,.04f,.86f,.96f,.98f,false,TextAlignmentOptions.Center);
            Label("Amount",go.transform,offer.gems.ToString("N0"),36,White,.02f,.32f,.98f,.53f,false,TextAlignmentOptions.Center);
            var price=Box("Price",go.transform,new Color(.32f,.15f,.13f));Rect(price,.06f,.055f,.94f,.28f);
            var priceLabel=Label("Label",price,"€"+offer.euroPrice,24,White,0,0,1,1,false,TextAlignmentOptions.Center);
            var handler=go.gameObject.AddComponent<GemOfferButton>();handler.shop=shop;handler.offerIndex=index;handler.priceLabel=priceLabel;
            Click(go,handler.Purchase);
        }
        var foot=Label("Availability",shop.gemsSection,"Gem purchases will be available when the store opens.",19,Muted,0,0,1,.065f);foot.alignment=TextAlignmentOptions.Center;
        BuildNotice(shop);
        BuildSettings(canvas,main);
        Click(Find("Btn_Shop").GetComponent<Button>(),shop.OpenShop);Click(Find("Btn_Locker").GetComponent<Button>(),shop.OpenShop);
        panel.transform.SetAsLastSibling();main.loadingPanel.transform.SetAsLastSibling();
        shop.Layout();panel.SetActive(false);EditorUtility.SetDirty(shop);EditorSceneManager.SaveScene(scene);
        var game=EditorSceneManager.OpenScene("Assets/Scenes/ScenaJoc.unity");
        var effects=mixer.FindMatchingGroups("Effects")[0];
        foreach(var source in Object.FindObjectsByType<AudioSource>(FindObjectsInactive.Include,FindObjectsSortMode.None))if(source.outputAudioMixerGroup==null){source.outputAudioMixerGroup=effects;EditorUtility.SetDirty(source);}
        EditorSceneManager.SaveScene(game);
        RoutePrefabs(effects);
        AssetDatabase.SaveAssets();EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
        Debug.Log("Shop upgrade saved: clean shop, dual currency, gem offers and audio settings.");
    }
    static void Wallet(GameObject icon,TextMeshProUGUI amount,Transform parent)
    {
        icon.transform.SetParent(parent,false);Rect(icon.transform,.055f,.17f,.29f,.83f);icon.GetComponent<Image>().preserveAspect=true;
        amount.transform.SetParent(parent,false);Rect(amount.transform,.32f,.08f,.91f,.92f);amount.font=body;amount.fontSize=23;amount.enableAutoSizing=true;amount.fontSizeMin=16;amount.fontSizeMax=23;amount.color=White;amount.alignment=TextAlignmentOptions.MidlineRight;
        amount.raycastTarget=false;
    }
    static void Stat(Transform parent,int index,string name,out TextMeshProUGUI value,out TextMeshProUGUI compare,out Image fill)
    {
        var box=Node("Stat_"+name,parent);Rect(box,.04f+index*.31f,.29f,.33f+index*.31f,.39f);
        Label("Caption",box,name=="FIRE RATE"?"ROUNDS / MIN":name=="DPS"?"DAMAGE / SEC":name,17,Muted,0,.58f,1,1);
        value=Label("Value",box,"25",30,White,0,.03f,.64f,.63f);
        compare=Label("Comparison",box,"",17,Muted,.65f,.08f,1,.6f,false,TextAlignmentOptions.Right);
        fill=Box("Meter",box,new Color(.36f,.4f,.29f)).GetComponent<Image>();Rect(fill.transform,0,0,1,.02f);
        fill.sprite=AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");fill.type=Image.Type.Filled;fill.fillMethod=Image.FillMethod.Horizontal;
    }
    static void BuildNotice(ShopManager shop)
    {
        var overlay=Box("StoreNotice",shop.safeArea,new Color(0,0,0,.86f));Stretch(overlay);shop.noticePanel=overlay.gameObject;
        var box=Box("Message",overlay,Ink);Rect(box,.06f,.32f,.94f,.68f);
        shop.noticeTitle=Label("Title",box,"STORE UNAVAILABLE",28,White,.06f,.73f,.94f,.92f);
        shop.noticeText=Label("Message",box,"",23,Muted,.06f,.29f,.94f,.7f);shop.noticeText.textWrappingMode=TextWrappingModes.Normal;
        var close=Button("Close",box,Accent);Rect(close.transform,.06f,.07f,.94f,.25f);Label("Label",close.transform,"OK",23,White,0,0,1,1,false,TextAlignmentOptions.Center);Click(close,shop.CloseNotice);overlay.gameObject.SetActive(false);
    }
    static void BuildSettings(GameObject canvas,MainMenuManager main)
    {
        var menu=canvas.GetComponent<SettingsMenu>();if(menu==null)menu=canvas.AddComponent<SettingsMenu>();
        var previous=canvas.transform.Find("Panel_Settings");if(previous!=null)Object.DestroyImmediate(previous.gameObject);
        var overlay=Box("Panel_Settings",canvas.transform,new Color(.02f,.025f,.02f,.97f));Stretch(overlay);menu.panel=overlay.gameObject;
        menu.safeArea=Node("SettingsSafeArea",overlay);Stretch(menu.safeArea);
        menu.sheet=Box("AudioSettings",menu.safeArea,Ink);menu.sheet.anchorMin=menu.sheet.anchorMax=menu.sheet.pivot=new Vector2(.5f,.5f);menu.sheet.anchoredPosition=Vector2.zero;menu.sheet.sizeDelta=new Vector2(660,570);
        Label("Title",menu.sheet,"SETTINGS",50,White,.055f,.79f,.85f,.96f,true);
        Label("Subtitle",menu.sheet,"AUDIO",20,Muted,.055f,.73f,.9f,.8f);
        var close=Button("Close",menu.sheet,Panel);Rect(close.transform,.85f,.85f,.95f,.95f);Label("Label",close.transform,"X",25,White,0,0,1,1,false,TextAlignmentOptions.Center);Click(close,menu.Close);
        MakeSlider(menu.sheet,"SOUND EFFECTS","Gunfire, zombies & other effects",.44f,out menu.effectsSlider,out menu.effectsValue);
        MakeSlider(menu.sheet,"MUSIC","Background music",.14f,out menu.musicSlider,out menu.musicValue);
        Label("Saved",menu.sheet,"Changes are saved automatically.",19,Muted,.055f,.035f,.95f,.1f);
        Click(main.settingsButton.GetComponent<Button>(),menu.Open);
        EditorUtility.SetDirty(menu);overlay.gameObject.SetActive(false);
    }
    static void MakeSlider(Transform parent,string name,string hint,float y,out Slider slider,out TextMeshProUGUI value)
    {
        var row=Node(name,parent);Rect(row,.055f,y,.945f,y+.24f);
        Label("Label",row,name,24,White,0,.68f,.8f,1);
        value=Label("Value",row,"100%",24,White,.8f,.68f,1,1,false,TextAlignmentOptions.Right);
        Label("Hint",row,hint,20,Muted,0,.43f,1,.68f);
        var trackRoot=Box("Slider",row,Color.clear);Rect(trackRoot,0,0,1,.4f);
        var track=Box("Track",trackRoot,new Color(.23f,.26f,.21f));Rect(track,.025f,.42f,.975f,.58f);
        var fill=Box("Fill",track,new Color(.61f,.65f,.42f));Stretch(fill);
        var handleArea=Node("HandleArea",trackRoot);Rect(handleArea,.025f,.5f,.975f,.5f);
        var handle=Box("Handle",handleArea,White);handle.anchorMin=handle.anchorMax=new Vector2(.5f,.5f);handle.sizeDelta=new Vector2(28,28);
        slider=trackRoot.gameObject.AddComponent<Slider>();slider.fillRect=fill;slider.handleRect=handle;slider.targetGraphic=handle.GetComponent<Image>();slider.minValue=0;slider.maxValue=1;slider.value=1;slider.direction=Slider.Direction.LeftToRight;
    }
    static TMP_FontAsset CreateHeading()
    {
        const string path="Assets/Settings/ShopHeading.asset";
        var font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);if(font!=null)return font;
        font=TMP_FontAsset.CreateFontAsset(AssetDatabase.LoadAssetAtPath<Font>("Assets/The Asesino.otf"),90,9,UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA,1024,1024);
        font.name="ShopHeading";font.TryAddCharacters("ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 /-.");font.atlasPopulationMode=AtlasPopulationMode.Static;
        AssetDatabase.CreateAsset(font,path);AssetDatabase.AddObjectToAsset(font.material,font);foreach(var tex in font.atlasTextures)AssetDatabase.AddObjectToAsset(tex,font);EditorUtility.SetDirty(font);return font;
    }
    static AudioMixer CreateMixer()
    {
        const string path="Assets/Resources/GameAudio.mixer";
        if(!AssetDatabase.IsValidFolder("Assets/Resources"))AssetDatabase.CreateFolder("Assets","Resources");
        var mixer=AssetDatabase.LoadAssetAtPath<AudioMixer>(path);if(mixer!=null)return mixer;
        var flags=BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static;
        var type=typeof(Editor).Assembly.GetType("UnityEditor.Audio.AudioMixerController");
        var controller=type.GetMethod("CreateMixerControllerAtPath",flags).Invoke(null,new object[]{path});
        var master=type.GetProperty("masterGroup",flags).GetValue(controller);
        var paramType=typeof(Editor).Assembly.GetType("UnityEditor.Audio.ExposedAudioParameter");var parameters=Array.CreateInstance(paramType,2);
        string[] names={"Effects","Music"};
        for(int i=0;i<2;i++)
        {
            var group=type.GetMethod("CreateNewGroup",flags).Invoke(controller,new object[]{names[i],true});
            type.GetMethod("AddChildToParent",flags).Invoke(controller,new[]{group,master});
            var guid=group.GetType().GetMethod("GetGUIDForVolume",flags).Invoke(group,null);
            var param=Activator.CreateInstance(paramType);paramType.GetField("guid",flags).SetValue(param,guid);paramType.GetField("name",flags).SetValue(param,names[i]+"Volume");parameters.SetValue(param,i);
        }
        type.GetProperty("exposedParameters",flags).SetValue(controller,parameters);EditorUtility.SetDirty((Object)controller);AssetDatabase.SaveAssets();return (AudioMixer)controller;
    }
    static void RoutePrefabs(AudioMixerGroup effects)
    {
        foreach(var guid in AssetDatabase.FindAssets("t:Prefab",new[]{"Assets"}))
        {
            string path=AssetDatabase.GUIDToAssetPath(guid);var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if(prefab==null||prefab.GetComponentsInChildren<AudioSource>(true).Length==0)continue;
            var root=PrefabUtility.LoadPrefabContents(path);bool changed=false;
            foreach(var source in root.GetComponentsInChildren<AudioSource>(true))if(source.outputAudioMixerGroup==null){source.outputAudioMixerGroup=effects;changed=true;}
            if(changed)PrefabUtility.SaveAsPrefabAsset(root,path);PrefabUtility.UnloadPrefabContents(root);
        }
    }
    static GameObject Find(string name)
    {
        foreach(var root in EditorSceneManager.GetActiveScene().GetRootGameObjects())foreach(var t in root.GetComponentsInChildren<Transform>(true))if(t.name==name)return t.gameObject;
        throw new Exception("Missing object: "+name);
    }
    static RectTransform Node(string name,Transform parent){var go=new GameObject(name,typeof(RectTransform));go.layer=5;go.transform.SetParent(parent,false);return (RectTransform)go.transform;}
    static RectTransform Box(string name,Transform parent,Color c){var r=Node(name,parent);r.gameObject.AddComponent<Image>().color=c;return r;}
    static Button Button(string name,Transform parent,Color c){var r=Box(name,parent,c);var b=r.gameObject.AddComponent<Button>();Style(b,c);return b;}
    static void Style(Button b,Color c){var image=b.GetComponent<Image>();image.sprite=null;image.color=c;b.targetGraphic=image;var colors=b.colors;colors.highlightedColor=new Color(1.12f,1.12f,1.12f);colors.pressedColor=new Color(.8f,.8f,.8f);colors.disabledColor=new Color(.55f,.55f,.55f,.8f);b.colors=colors;}
    static TextMeshProUGUI Label(string name,Transform parent,string text,float size,Color color,float x0,float y0,float x1,float y1,bool title=false,TextAlignmentOptions alignment=TextAlignmentOptions.MidlineLeft)
    {
        var r=Node(name,parent);Rect(r,x0,y0,x1,y1);var t=r.gameObject.AddComponent<TextMeshProUGUI>();t.font=title?heading:body;t.text=text;t.fontSize=size;t.fontSizeMin=size*.8f;t.fontSizeMax=size;t.enableAutoSizing=true;t.color=color;t.raycastTarget=false;t.alignment=alignment;t.textWrappingMode=TextWrappingModes.NoWrap;t.overflowMode=TextOverflowModes.Ellipsis;return t;
    }
    static void Rect(Transform transform,float x0,float y0,float x1,float y1){var r=(RectTransform)transform;r.anchorMin=new Vector2(x0,y0);r.anchorMax=new Vector2(x1,y1);r.pivot=new Vector2(.5f,.5f);r.offsetMin=r.offsetMax=Vector2.zero;r.localScale=Vector3.one;}
    static void Stretch(RectTransform r){Rect(r,0,0,1,1);}
    static void Click(Button button,UnityEngine.Events.UnityAction action){button.onClick=new Button.ButtonClickedEvent();UnityEventTools.AddPersistentListener(button.onClick,action);EditorUtility.SetDirty(button);}
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
