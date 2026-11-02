using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Presentation only. Navigation, PlayerPrefs and the existing panels remain in their original systems.
public sealed class MainMenuPresentation : MonoBehaviour
{
    public MainMenuManager menu;
    public TextMeshProUGUI zombieTitle, runnerTitle;

    public void Layout()
    {
        Canvas.ForceUpdateCanvases();
        Rect safe = menu.menuSafeArea.rect;
        float width = Mathf.Min(safe.width - 48f, 860f);
        float height = safe.height;
        float left = (safe.width - width) * .5f;
        bool landscape = safe.width > height * 1.15f;

        // Cover, never stretch: preserve the original cinematic image's aspect ratio.
        var canvasRect = (RectTransform)menu.transform;
        var artwork = menu.menuArtwork;
        var sprite = artwork.GetComponent<Image>().sprite;
        float scale = Mathf.Max(canvasRect.rect.width / sprite.rect.width, canvasRect.rect.height / sprite.rect.height);
        artwork.anchorMin = artwork.anchorMax = new Vector2(.5f, .5f);
        artwork.pivot = new Vector2(.5f, .5f);
        artwork.anchoredPosition = Vector2.zero;
        artwork.sizeDelta = sprite.rect.size * scale;
        artwork.localScale = Vector3.one;

        if (landscape)
        {
            float column = Mathf.Min(safe.width * .37f, 520f);
            float x = safe.width - column - 36;
            float gap = 18, secondary = 66, primary = 90;
            float bottom = Mathf.Max(30, (height - primary - 3 * secondary - 3 * gap) * .5f);
            Bottom(menu.settingsButton,x,bottom,column,secondary);
            Bottom(menu.lockerButton,x,bottom+secondary+gap,column,secondary);
            Bottom(menu.shopButton,x,bottom+2*(secondary+gap),column,secondary);
            Bottom(menu.playButton,x,bottom+3*(secondary+gap),column,primary);
            float titleWidth = safe.width * .52f;
            Title(zombieTitle,30,height*.65f,titleWidth,100,82);
            Title(runnerTitle,30,height*.65f-96,titleWidth,112,94);
            return;
        }

        float buttonScale = Mathf.Clamp(height / 1280f, .9f, 1.08f);
        float settingsH = 68 * buttonScale, secondaryH = 82 * buttonScale, playH = 110 * buttonScale;
        float gapY = 20 * buttonScale;
        float bottomInset = Mathf.Clamp(height * .07f, 40, 110);
        float half = (width - 22) * .5f;
        Bottom(menu.settingsButton,left+(width-half)*.5f,bottomInset,half,settingsH);
        float secondaryY=bottomInset+settingsH+gapY;
        Bottom(menu.shopButton,left,secondaryY,half,secondaryH);
        Bottom(menu.lockerButton,left+half+22,secondaryY,half,secondaryH);
        Bottom(menu.playButton,left,secondaryY+secondaryH+gapY,width,playH);

        float titleScale = Mathf.Clamp(height / 1280f, .78f, 1f);
        float titleTop = height - Mathf.Clamp(height * .065f, 42, 100);
        float titleH = 108 * titleScale;
        Title(zombieTitle,left,titleTop-titleH,width,titleH,92*titleScale);
        Title(runnerTitle,left,titleTop-titleH*2,width,titleH,104*titleScale);
    }

    static void Title(TextMeshProUGUI text,float x,float y,float width,float height,float size)
    {
        Bottom(text.rectTransform,x,y,width,height);
        text.fontSizeMax=size;
        text.fontSizeMin=size*.8f;
        text.fontSize=size;
    }

    static void Bottom(RectTransform rect,float x,float y,float width,float height)
    {
        rect.anchorMin=rect.anchorMax=Vector2.zero;
        rect.pivot=Vector2.zero;
        rect.anchoredPosition=new Vector2(x,y);
        rect.sizeDelta=new Vector2(width,height);
        rect.localScale=Vector3.one;
    }
}
