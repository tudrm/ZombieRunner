#if UNITY_EDITOR
using System;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object=UnityEngine.Object;

public static class MainMenuCanvaAuthoring
{
    const string Root="Assets/UI/MainMenuCanva/";
    const string ShopArt="Assets/UI/ShopCanva/Art/";

    [MenuItem("Tools/ZombieRunner/Apply Canva Main Menu (MainMenu Only)")]
    public static void Apply()
    {
        var scene=EditorSceneManager.GetActiveScene();
        if(EditorApplication.isPlaying||scene.path!="Assets/Scenes/MainMenu.unity"||scene.isDirty)
            throw new InvalidOperationException("Open the saved MainMenu scene outside Play Mode.");
        var menu=Object.FindFirstObjectByType<MainMenuManager>();
        if(menu.presentation!=null)throw new InvalidOperationException("Main Menu already applied. Do not run migration twice.");
        if(!File.Exists("Backups/MainMenuCanva-2026-11-02/MainMenu.unity"))throw new InvalidOperationException("Backup missing.");

        var importer=AssetImporter.GetAtPath(Root+"CityBackground.jpg") as TextureImporter;
        if(importer==null)throw new InvalidOperationException("Import the original Canva background first.");
        importer.textureType=TextureImporterType.Sprite;
        importer.spriteImportMode=SpriteImportMode.Single;
        importer.mipmapEnabled=false;importer.isReadable=false;importer.maxTextureSize=2048;
        importer.textureCompression=TextureImporterCompression.CompressedHQ;
        importer.wrapMode=TextureWrapMode.Clamp;importer.SaveAndReimport();
        var background=AssetDatabase.LoadAssetAtPath<Sprite>(Root+"CityBackground.jpg");
        if(background==null||background.rect.height<1000)throw new InvalidOperationException("Background must be the original, not a thumbnail.");
        var font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/UI/ShopCanva/Fonts/BarlowCondensed SDF.asset");
        var fill=AssetDatabase.LoadAssetAtPath<Sprite>(ShopArt+"ButtonNeutral.png");
        var frame=AssetDatabase.LoadAssetAtPath<Sprite>(ShopArt+"Frame.png");
        var card=AssetDatabase.LoadAssetAtPath<Sprite>(ShopArt+"Card.png");
        if(font==null||fill==null||frame==null||card==null)throw new InvalidOperationException("Existing Shop presentation assets missing.");

        var artwork=menu.menuArtwork.GetComponent<Image>();
        artwork.sprite=background;artwork.color=Color.white;artwork.type=Image.Type.Simple;
        artwork.preserveAspect=false;artwork.raycastTarget=false;
        var presentation=menu.gameObject.AddComponent<MainMenuPresentation>();
        presentation.menu=menu;menu.presentation=presentation;
        presentation.zombieTitle=Label("Title_Zombie",menu.menuSafeArea,"ZOMBIE",font,92,ShopPresentation.White);
        presentation.runnerTitle=Label("Title_Runner",menu.menuSafeArea,"RUNNER",font,104,ShopPresentation.Lime);
        var buttons=new[]{menu.playButton,menu.shopButton,menu.lockerButton,menu.settingsButton};
        var names=new[]{"PLAY","SHOP","LOCKER","SETTINGS"};
        float ppu=menu.GetComponent<Canvas>().referencePixelsPerUnit/100f;
        for(int i=0;i<buttons.Length;i++)
        {
            var rect=buttons[i];var button=rect.GetComponent<Button>();var image=rect.GetComponent<Image>();
            image.sprite=i==0?fill:card;image.type=Image.Type.Sliced;image.pixelsPerUnitMultiplier=ppu;
            image.color=i==0?ShopPresentation.Lime:Color.white;image.raycastTarget=true;
            var colors=button.colors;colors.fadeDuration=.12f;colors.colorMultiplier=1;
            if(i==0)
            {
                button.targetGraphic=image;
                colors.normalColor=ShopPresentation.Lime;colors.highlightedColor=new Color32(210,255,108,255);
                colors.pressedColor=new Color32(146,194,46,255);colors.selectedColor=new Color32(210,255,108,255);
                colors.disabledColor=new Color32(76,85,59,255);
                // Selectable owns the final tint; do not multiply Lime twice.
                image.color=Color.white;
            }
            else
            {
                var border=new GameObject("Border",typeof(RectTransform),typeof(CanvasRenderer),typeof(Image));
                border.transform.SetParent(rect,false);Stretch((RectTransform)border.transform,0);
                var borderImage=border.GetComponent<Image>();borderImage.sprite=frame;borderImage.type=Image.Type.Sliced;
                borderImage.pixelsPerUnitMultiplier=ppu;borderImage.color=Color.white;borderImage.raycastTarget=false;
                button.targetGraphic=borderImage;
                colors.normalColor=new Color32(101,117,127,255);colors.highlightedColor=ShopPresentation.Lime;
                colors.pressedColor=ShopPresentation.White;colors.selectedColor=ShopPresentation.Lime;
                colors.disabledColor=new Color32(47,55,61,255);
            }
            button.transition=Selectable.Transition.ColorTint;button.colors=colors;
            var label=Label("Label",rect,names[i],font,i==0?48:34,i==0?ShopPresentation.Ink:ShopPresentation.White);
            Stretch(label.rectTransform,12);label.fontSizeMin=i==0?38:26;
            // Deliberately leave button.onClick, interactable and navigation untouched.
        }
        var safe=Screen.safeArea;
        menu.menuSafeArea.anchorMin=new Vector2(safe.xMin/Screen.width,safe.yMin/Screen.height);
        menu.menuSafeArea.anchorMax=new Vector2(safe.xMax/Screen.width,safe.yMax/Screen.height);
        menu.menuSafeArea.offsetMin=menu.menuSafeArea.offsetMax=Vector2.zero;
        presentation.Layout();
        EditorUtility.SetDirty(menu);EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("Canva Main Menu applied. Original UnityEvents and all other panels retained.");
    }

    static TextMeshProUGUI Label(string name,Transform parent,string text,TMP_FontAsset font,float size,Color color)
    {
        var go=new GameObject(name,typeof(RectTransform),typeof(CanvasRenderer),typeof(TextMeshProUGUI));
        go.transform.SetParent(parent,false);
        var label=go.GetComponent<TextMeshProUGUI>();label.text=text;label.font=font;label.fontSize=size;
        label.fontSizeMax=size;label.fontSizeMin=size*.8f;label.enableAutoSizing=true;
        label.color=color;label.alignment=TextAlignmentOptions.Center;label.raycastTarget=false;
        label.textWrappingMode=TextWrappingModes.NoWrap;
        return label;
    }

    static void Stretch(RectTransform r,float inset)
    {r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=new Vector2(inset,inset);r.offsetMax=new Vector2(-inset,-inset);}
}
#endif
