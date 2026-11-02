using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Canva visual layer. Contains no inventory, economy, persistence or payment logic.
public sealed class ShopPresentation : MonoBehaviour
{
    public ShopManager shop;
    public Sprite coinButtonSprite, gemButtonSprite, equippedButtonSprite;
    public Sprite activeTabSprite, inactiveTabSprite;
    public RectTransform[] offerRects;
    public TextMeshProUGUI availability;
    public static readonly Color Lime=new Color32(190,242,74,255);
    public static readonly Color Ink=new Color32(17,22,29,255);
    public static readonly Color White=new Color32(243,245,235,255);
    public static readonly Color Muted=new Color32(165,176,183,255);

    public void Refresh()
    {
        for(int i=0;i<shop.categoryButtons.Length;i++)
        {
            bool selected=i==(int)shop.Category;
            shop.categoryButtons[i].image.sprite=selected?activeTabSprite:inactiveTabSprite;
            shop.categoryButtons[i].image.type=UnityEngine.UI.Image.Type.Sliced;
            shop.categoryButtons[i].image.color=selected?Color.white:new Color(.35f,.42f,.48f);
            shop.categoryButtons[i].GetComponentInChildren<TextMeshProUGUI>().color=selected?Ink:White;
        }
        var cardViews=shop.cardContent.GetComponentsInChildren<ShopWeaponCard>(false);
        int ownedCount=0;
        foreach(var card in cardViews)
        {
            card.Refresh(shop.catalog,shop.Selected);
            if(shop.catalog.IsOwned(card.weapon))ownedCount++;
        }
        if(shop.Selected==null)return;
        bool owned=shop.catalog.IsOwned(shop.Selected), equipped=shop.Selected==shop.catalog.Equipped;
        shop.titleText.color=Lime;
        shop.actionText.text=equipped?"EQUIPPED":owned?"EQUIP":"BUY  "+shop.Selected.price.ToString("N0");
        shop.gemActionText.text="BUY  "+shop.Selected.gemPrice.ToString("N0");
        shop.actionText.color=Ink;shop.gemActionText.color=White;
        shop.purchaseButton.image.sprite=owned?equippedButtonSprite:coinButtonSprite;
        shop.purchaseButton.image.color=owned?Lime:Color.white;
        var buttonColors=shop.purchaseButton.colors;
        buttonColors.disabledColor=owned?Color.white:new Color(.65f,.65f,.65f,.8f);
        shop.purchaseButton.colors=buttonColors;
        shop.gemPurchaseButton.image.color=Color.white;
        shop.priceText.text="";
        shop.descriptionText.text=owned?"":(!shop.purchaseButton.interactable&&!shop.gemPurchaseButton.interactable?"Not enough Coins or Gems.":"Pay with Coins or Gems.");
        shop.collectionText.text="YOUR ARSENAL  <size=65%><color=#A5B0B7>"+ownedCount+" / "+cardViews.Length+" OWNED</color></size>";
        var coinIcon=shop.purchaseButton.transform.Find("CurrencyIcon");
        if(coinIcon!=null)coinIcon.gameObject.SetActive(!owned);
        float width=shop.actionBar.rect.width;
        ShopManager.Top((RectTransform)shop.purchaseButton.transform,0,0,owned?width:(width-16)/2,88);
        ShopManager.Top((RectTransform)shop.gemPurchaseButton.transform,(width+16)/2,0,(width-16)/2,88);
        bool available=shop.purchaseProvider!=null&&shop.purchaseProvider.IsAvailable;
        availability.text=available?"Coins are earned in combat.":"STORE UNAVAILABLE  ·  Offers preview only\nCoins are earned in combat.";
    }

    public void Layout()
    {
        // Use the existing CanvasScaler. Limit reading width on tablets without shrinking text.
        float safeWidth=shop.safeArea.rect.width;
        float width=Mathf.Min(880,safeWidth-40), x=(safeWidth-width)/2;
        ShopManager.Top(shop.header,x,8,width,112);
        var viewport=shop.weaponScroll.viewport;
        viewport.anchorMin=Vector2.zero;viewport.anchorMax=Vector2.one;
        viewport.offsetMin=new Vector2(x,20);viewport.offsetMax=new Vector2(-x,-132);
        Canvas.ForceUpdateCanvases();
        ShopManager.Top(shop.categories,0,0,width,88);
        ShopManager.Top(shop.details,0,108,width,490);
        ShopManager.Top(shop.actionBar,24,350,width-48,118);
        int count=0;foreach(Transform child in shop.cardContent)if(child.gameObject.activeSelf)count++;
        shop.cardGrid.constraintCount=2;shop.cardGrid.cellSize=new Vector2((width-16)/2,170);shop.cardGrid.spacing=new Vector2(16,16);
        float gridHeight=Mathf.Max(0,Mathf.Ceil(count/2f)*186-16);
        ShopManager.Top(shop.collection,0,626,width,64+gridHeight);
        ShopManager.Top(shop.collectionText.rectTransform,0,0,width,44);
        ShopManager.Top(shop.cardContent,0,64,width,gridHeight);
        float gemTop=626+64+gridHeight+40;
        float rowHeight=184;
        int rows=Mathf.CeilToInt(offerRects.Length/2f);
        float offersHeight=rows*(rowHeight+16)-16;
        ShopManager.Top(shop.gemsSection,0,gemTop,width,136+offersHeight+96);
        ShopManager.Top(shop.gemCards,0,136,width,offersHeight);
        for(int i=0;i<offerRects.Length;i++)
        {
            bool full=i==offerRects.Length-1&&offerRects.Length%2==1;
            float cell=(width-16)/2;
            ShopManager.Top(offerRects[i],full?0:(i%2)*(cell+16),(i/2)*(rowHeight+16),full?width:cell,rowHeight);
        }
        ShopManager.Top(availability.rectTransform,0,136+offersHeight+16,width,76);
        shop.pageContent.sizeDelta=new Vector2(0,gemTop+136+offersHeight+112);
    }
}
