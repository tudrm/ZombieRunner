using System.Collections;
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
    public GemStoreCatalog gemStore;
    public GemPurchaseProvider purchaseProvider;
    public GameObject shopPanel;
    public RectTransform safeArea, header, categories, collection, details, actionBar;
    public RectTransform cardContent, pageContent, gemsSection, gemCards;
    public GameObject cardTemplate;
    public ScrollRect weaponScroll;
    public GridLayoutGroup cardGrid, gemGrid;
    public Button[] categoryButtons;
    public Button purchaseButton, gemPurchaseButton;
    public TextMeshProUGUI titleText, tierText, descriptionText, damageText, rateText, dpsText;
    public TextMeshProUGUI damageCompare, rateCompare, dpsCompare;
    public Image damageFill, rateFill, dpsFill;
    public TextMeshProUGUI actionText, gemActionText, priceText, statusText, collectionText, equippedText;
    public GameObject noticePanel;
    public TextMeshProUGUI noticeTitle, noticeText;
    public WeaponPreview preview;
    public ShopPresentation presentation;
    public WeaponDefinition Selected { get; private set; }
    public WeaponCategory Category { get; private set; }
    readonly List<Card> cards = new List<Card>();
    Vector2 lastSize;
    Vector2 lastLayoutSize;
    Rect lastSafe;
    Coroutine scrollAnimation;
    bool buyingGems;
    static readonly Color Red = new Color(.66f,.22f,.19f);
    static readonly Color Muted = new Color(.61f,.63f,.59f);
    static readonly Color Green = new Color(.62f,.75f,.53f);
    class Card { public WeaponDefinition weapon; public Image background; public TextMeshProUGUI ownership; public GameObject selectedMark; }

    void Awake() { instance = this; }
    void Start()
    {
        for(int i=0;i<categoryButtons.Length;i++) { int index=i; categoryButtons[i].onClick.AddListener(()=>SelectCategory(index)); }
        purchaseButton.onClick.AddListener(PurchaseOrEquip);
        gemPurchaseButton.onClick.AddListener(PurchaseWithGems);
        ActualizeazaInterfata();
        if(shopPanel.activeSelf) OpenShop();
    }
    void Update()
    {
        if (!shopPanel.activeInHierarchy) return;
        if(lastSize!=new Vector2(Screen.width,Screen.height)||lastSafe!=Screen.safeArea||lastLayoutSize!=safeArea.rect.size) Layout();
        if(Input.GetKeyDown(KeyCode.Escape)) { if(noticePanel.activeSelf) CloseNotice(); else CloseShop(); }
    }
    public void OpenShop()
    {
        shopPanel.SetActive(true); noticePanel.SetActive(false);
        ActualizeazaInterfata(); SelectCategory((int)catalog.Equipped.category);
    }
    public void CloseShop() { StopScroll(); shopPanel.SetActive(false); }
    public void SelectCategory(int index)
    {
        Category=(WeaponCategory)Mathf.Clamp(index,0,2);
        foreach(var c in cards) { c.background.gameObject.SetActive(false); Destroy(c.background.gameObject); }
        cards.Clear(); WeaponDefinition first=null;
        foreach(var w in catalog.weapons)
        {
            if(w.category!=Category)continue;
            if(first==null)first=w;
            var go=Instantiate(cardTemplate,cardContent);go.name="Weapon_"+w.id;go.SetActive(true);
            go.transform.Find("Name").GetComponent<TextMeshProUGUI>().text=w.displayName;
            go.transform.Find("Thumbnail").GetComponent<Image>().sprite=w.thumbnail;
            var cardView=go.GetComponent<ShopWeaponCard>();
            if(cardView!=null)cardView.weapon=w;
            var c=new Card{weapon=w,background=go.GetComponent<Image>(),ownership=go.transform.Find("Ownership").GetComponent<TextMeshProUGUI>(),selectedMark=go.transform.Find("Selected").gameObject};
            cards.Add(c); go.GetComponent<Button>().onClick.AddListener(()=>{ SelectWeapon(w); ScrollTop(); });
        }
        for(int i=0;i<categoryButtons.Length;i++)
        {
            categoryButtons[i].GetComponent<Image>().color=i==(int)Category?new Color(.22f,.24f,.21f):new Color(.10f,.115f,.105f);
            categoryButtons[i].transform.Find("Line").gameObject.SetActive(i==(int)Category);
        }
        SelectWeapon(catalog.Equipped.category==Category?catalog.Equipped:first);
        Layout(); ScrollTop();
    }
    public void SelectWeapon(WeaponDefinition weapon)
    {
        if(weapon==null)return;
        Selected=weapon; titleText.text=weapon.displayName;
        tierText.text=(weapon.Automatic?"AUTOMATIC":"SEMI-AUTO")+"   /   "+"TIER "+weapon.tier.ToString("00");
        damageText.text=weapon.damage.ToString(); rateText.text=weapon.roundsPerMinute.ToString();
        dpsText.text=Mathf.RoundToInt(weapon.damage/weapon.FireInterval).ToString();
        damageFill.fillAmount=weapon.damage/100f;rateFill.fillAmount=weapon.roundsPerMinute/1000f;dpsFill.fillAmount=Mathf.Clamp01(weapon.damage/weapon.FireInterval/1200f);
        var equipped=catalog.Equipped;
        Compare(damageCompare,weapon.damage-equipped.damage);
        Compare(rateCompare,weapon.roundsPerMinute-equipped.roundsPerMinute);
        Compare(dpsCompare,Mathf.RoundToInt(weapon.damage/weapon.FireInterval-equipped.damage/equipped.FireInterval));
        statusText.text=""; preview.Show(weapon); RefreshSelection();
    }
    void Compare(TextMeshProUGUI label,int difference)
    {
        label.text=difference==0?"":(difference>0?"+":"")+difference;
        label.color=difference>0?Green:Red;
    }
    public void PurchaseOrEquip() { BuyWeapon(false); }
    public void PurchaseWithGems() { BuyWeapon(true); }
    void BuyWeapon(bool gems)
    {
        if(Selected==null)return;
        string message;
        if(catalog.IsOwned(Selected)) { catalog.Equip(Selected);message="Weapon equipped."; }
        else if(!catalog.Purchase(Selected,gems,out message)) { statusText.text=message; return; }
        ActualizeazaInterfata(); SelectWeapon(Selected); statusText.text=message;
    }
    void RefreshSelection()
    {
        var equipped=catalog.Equipped; int ownedCount=0;
        equippedText.text="EQUIPPED  "+equipped.displayName;
        foreach(var c in cards)
        {
            bool owned=catalog.IsOwned(c.weapon);if(owned)ownedCount++;
            c.background.color=c.weapon==Selected?new Color(.20f,.225f,.19f):new Color(.105f,.12f,.11f);
            c.selectedMark.SetActive(c.weapon==Selected);
            c.ownership.text=c.weapon==equipped?"EQUIPPED":owned?"OWNED":c.weapon.price.ToString("N0")+" Coins / "+c.weapon.gemPrice.ToString("N0")+" Gems";
            c.ownership.color=owned?Green:Muted;
        }
        collectionText.text="WEAPONS   "+ownedCount+" / "+cards.Count;
        bool ownedSelected=catalog.IsOwned(Selected),active=Selected==equipped;
        actionText.text=active?"EQUIPPED":ownedSelected?"EQUIP":Selected.price.ToString("N0")+" COINS";
        gemActionText.text=Selected.gemPrice.ToString("N0")+" GEMS";
        gemPurchaseButton.gameObject.SetActive(!ownedSelected);
        purchaseButton.interactable=!active&&(ownedSelected||monede>=Selected.price);
        gemPurchaseButton.interactable=!ownedSelected&&gemuri>=Selected.gemPrice;
        priceText.text=ownedSelected?"In your arsenal":"UNLOCK WITH";
        descriptionText.text=ownedSelected?"": "Choose Coins or Gems. Includes automatic equip.";
        purchaseButton.GetComponent<Image>().color=ownedSelected?new Color(.25f,.31f,.22f):new Color(.44f,.37f,.23f);
        Top((RectTransform)purchaseButton.transform,0,26,ownedSelected?actionBar.rect.width:actionBar.rect.width*.49f,62);
        if(presentation!=null)presentation.Refresh();
    }
    public void ActualizeazaInterfata()
    {
        monede=PlayerPrefs.GetInt(WeaponCatalog.CoinsKey,WeaponCatalog.StartingCoins);gemuri=PlayerPrefs.GetInt(WeaponCatalog.GemsKey,50);
        textMonede.text=monede.ToString("N0");textGemuri.text=gemuri.ToString("N0");
    }
    public void AdaugaMonede(int amount) { WeaponCatalog.AwardCoins(amount);PlayerPrefs.Save();ActualizeazaInterfata(); }
    public void CheltuieMonede(int amount)
    {
        ActualizeazaInterfata();if(amount<0||amount>monede)return;
        PlayerPrefs.SetInt(WeaponCatalog.CoinsKey,monede-amount);PlayerPrefs.Save();ActualizeazaInterfata();
    }
    public void ShowGems()
    {
        Layout();StopScroll();
        float range=pageContent.rect.height-weaponScroll.viewport.rect.height;
        float target=range>0?1-Mathf.Clamp01((-gemsSection.anchoredPosition.y)/range):1;
        scrollAnimation=StartCoroutine(ScrollTo(target));
    }
    IEnumerator ScrollTo(float target)
    {
        weaponScroll.StopMovement();float start=weaponScroll.verticalNormalizedPosition;
        for(float t=0;t<1;t+=Time.unscaledDeltaTime/.32f) { weaponScroll.verticalNormalizedPosition=Mathf.Lerp(start,target,Mathf.SmoothStep(0,1,t));yield return null; }
        weaponScroll.verticalNormalizedPosition=target;scrollAnimation=null;
    }
    void StopScroll() { if(scrollAnimation!=null)StopCoroutine(scrollAnimation);scrollAnimation=null;weaponScroll.StopMovement(); }
    void ScrollTop() { StopScroll();weaponScroll.verticalNormalizedPosition=1; }
    public void BuyGemPack(int index)
    {
        if(index<0||index>=gemStore.offers.Length||buyingGems)return;
        var offer=gemStore.offers[index];
        if(purchaseProvider==null||!purchaseProvider.IsAvailable)
        { Notice("STORE UNAVAILABLE","Gem purchases are not available yet.\nNo payment has been made.");return; }
        buyingGems=true;
        Notice("CONNECTING","Opening the store...");
        purchaseProvider.Purchase(offer.productId,(success,message)=>
        {
            buyingGems=false;if(this==null)return;
            ActualizeazaInterfata();if(Selected!=null)RefreshSelection();
            Notice(success?"PURCHASE COMPLETE":"PURCHASE NOT COMPLETED",message);
        });
    }
    void Notice(string title,string body) { noticeTitle.text=title;noticeText.text=body;noticePanel.SetActive(true); }
    public void CloseNotice() { noticePanel.SetActive(false); }
    public void Layout()
    {
        lastSize=new Vector2(Screen.width,Screen.height);lastSafe=Screen.safeArea;
        safeArea.anchorMin=new Vector2(lastSafe.xMin/Screen.width,lastSafe.yMin/Screen.height);
        safeArea.anchorMax=new Vector2(lastSafe.xMax/Screen.width,lastSafe.yMax/Screen.height);safeArea.offsetMin=safeArea.offsetMax=Vector2.zero;
        Canvas.ForceUpdateCanvases();
        if(presentation!=null)
        {
            presentation.Layout();
            if(Selected!=null)RefreshSelection();
            lastLayoutSize=safeArea.rect.size;
            return;
        }
        float w=safeArea.rect.width;bool wide=w>1000;
        Top(header,24,12,w-48,82);
        var viewport=weaponScroll.viewport;viewport.anchorMin=Vector2.zero;viewport.anchorMax=Vector2.one;viewport.offsetMin=new Vector2(24,12);viewport.offsetMax=new Vector2(-24,-110);
        Canvas.ForceUpdateCanvases();float width=viewport.rect.width;
        float left=wide?width*.44f:width,right=wide?width-left-32:width;
        Top(categories,wide?left+32:0,0,right,54);
        Top(details,0,wide?0:74,left,540);
        int columns=wide?3:2;
        float cell=(right-12*(columns-1))/columns;
        cardGrid.constraintCount=columns;cardGrid.cellSize=new Vector2(cell,180);
        float gridHeight=Mathf.Ceil(cards.Count/(float)columns)*192-12;
        float collectionTop=wide?76:638;
        Top(collection,wide?left+32:0,collectionTop,right,44+gridHeight);
        Top(cardContent,0,44,right,gridHeight);
        float bottom=Mathf.Max(wide?540:614,collectionTop+44+gridHeight)+40;
        int gemColumns=wide?5:2;float gemCell=(width-12*(gemColumns-1))/gemColumns;
        gemGrid.constraintCount=gemColumns;gemGrid.cellSize=new Vector2(gemCell,224);
        float gemHeight=Mathf.Ceil(gemStore.offers.Length/(float)gemColumns)*236-12;
        Top(gemsSection,0,bottom,width,100+gemHeight+64);
        Top(gemCards,0,100,width,gemHeight);
        pageContent.sizeDelta=new Vector2(0,bottom+100+gemHeight+80);
        Top(actionBar,24,402,left-48,110);
        Top((RectTransform)gemPurchaseButton.transform,actionBar.rect.width*.51f,26,actionBar.rect.width*.49f,62);
        if(Selected!=null)RefreshSelection();
        lastLayoutSize=safeArea.rect.size;
    }
    public static void Top(RectTransform r,float x,float y,float w,float h)
    {
        r.anchorMin=r.anchorMax=new Vector2(0,1);r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);
    }
    void OnDestroy(){if(instance==this)instance=null;}
}
