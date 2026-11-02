#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object=UnityEngine.Object;

// Focused checks: no Settings, gameplay, build or payment-provider changes.
public static class CanvaShopChecks
{
    const string DirectoryPath="Library/CanvaShopValidation";
    static List<string> results;
    static readonly BindingFlags Flags=BindingFlags.Instance|BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic;
    static void Check(bool condition,string message)
    {results.Add((condition?"PASS: ":"FAIL: ")+message);if(!condition)throw new InvalidOperationException(message);}

    [MenuItem("Tools/ZombieRunner/Check Canva Shop (Play Mode)")]
    public static string Run()
    {
        if(!EditorApplication.isPlaying)throw new InvalidOperationException("Enter Play Mode in MainMenu.");
        var s=Object.FindFirstObjectByType<ShopManager>();var c=s.catalog;
        var keys=c.weapons.Select(w=>WeaponCatalog.OwnedPrefix+w.id).Concat(new[]{WeaponCatalog.CoinsKey,WeaponCatalog.GemsKey});
        var saved=keys.ToDictionary(k=>k,k=>PlayerPrefs.HasKey(k)?(int?)PlayerPrefs.GetInt(k):null);
        bool hadEquipped=PlayerPrefs.HasKey(WeaponCatalog.EquippedKey);string equipped=PlayerPrefs.GetString(WeaponCatalog.EquippedKey);
        bool wasOpen=s.shopPanel.activeSelf;
        results=new List<string>();Directory.CreateDirectory(DirectoryPath);
        File.WriteAllLines(DirectoryPath+"/PlayerPrefs-before-test.txt",saved.Select(p=>p.Key+"="+(p.Value.HasValue?p.Value.ToString():"<absent>")).Concat(new[]{WeaponCatalog.EquippedKey+"="+(hadEquipped?equipped:"<absent>")}));
        try
        {
            Check(c.weapons.Length==20,"Existing catalog has 20 weapons.");
            s.OpenShop();
            for(int category=0;category<3;category++)
            {
                s.categoryButtons[category].onClick.Invoke();
                Check((int)s.Category==category,"Tab onClick: "+category);
                Check(s.cardContent.GetComponentsInChildren<ShopWeaponCard>().Length==c.weapons.Count(w=>(int)w.category==category),"Category card count: "+category);
                foreach(var card in s.cardContent.GetComponentsInChildren<ShopWeaponCard>())
                {
                    card.GetComponent<Button>().onClick.Invoke();var w=card.weapon;
                    Check(s.Selected==w&&s.titleText.text==w.displayName&&s.damageText.text==w.damage.ToString()&&s.rateText.text==w.roundsPerMinute.ToString(),"Card selection/stats: "+w.id);
                    Check(s.preview.display.texture!=null&&s.preview.display.texture.width==768,"Live model RenderTexture: "+w.id);
                    Check(card.frame.gameObject.activeSelf,"Selected frame: "+w.id);
                }
            }
            var locked=c.weapons[1];PlayerPrefs.DeleteKey(WeaponCatalog.OwnedPrefix+locked.id);
            PlayerPrefs.SetString(WeaponCatalog.EquippedKey,c.Starter.id);
            PlayerPrefs.SetInt(WeaponCatalog.CoinsKey,0);PlayerPrefs.SetInt(WeaponCatalog.GemsKey,0);
            s.ActualizeazaInterfata();s.SelectCategory(0);s.SelectWeapon(locked);
            Check(!s.purchaseButton.interactable&&!s.gemPurchaseButton.interactable,"Insufficient wallets disable purchase controls.");
            s.PurchaseOrEquip();Check(!c.IsOwned(locked)&&s.statusText.text=="Not enough Coins.","Existing insufficient Coins feedback.");
            s.PurchaseWithGems();Check(!c.IsOwned(locked)&&s.statusText.text=="Not enough Gems.","Existing insufficient Gems feedback.");
            PlayerPrefs.SetInt(WeaponCatalog.CoinsKey,locked.price);PlayerPrefs.SetInt(WeaponCatalog.GemsKey,777);s.ActualizeazaInterfata();s.SelectWeapon(locked);
            s.purchaseButton.onClick.Invoke();
            Check(c.IsOwned(locked)&&c.Equipped==locked&&PlayerPrefs.GetInt(WeaponCatalog.CoinsKey)==0&&PlayerPrefs.GetInt(WeaponCatalog.GemsKey)==777,"Buy Coins UI debits only Coins, owns and equips.");
            s.purchaseButton.onClick.Invoke();Check(PlayerPrefs.GetInt(WeaponCatalog.GemsKey)==777&&PlayerPrefs.GetInt(WeaponCatalog.CoinsKey)==0,"Repeat action cannot double-charge.");
            var second=c.weapons[2];PlayerPrefs.DeleteKey(WeaponCatalog.OwnedPrefix+second.id);PlayerPrefs.SetInt(WeaponCatalog.GemsKey,second.gemPrice);s.ActualizeazaInterfata();s.SelectWeapon(second);s.gemPurchaseButton.onClick.Invoke();
            Check(c.IsOwned(second)&&c.Equipped==second&&PlayerPrefs.GetInt(WeaponCatalog.GemsKey)==0,"Buy Gems UI uses existing purchase logic.");
            s.SelectWeapon(locked);Check(s.actionText.text=="EQUIP"&&s.purchaseButton.interactable,"Owned state exposes Equip.");s.purchaseButton.onClick.Invoke();
            Check(c.Equipped==locked&&s.actionText.text=="EQUIPPED"&&!s.purchaseButton.interactable,"Equip and Equipped state.");
            s.CloseShop();s.OpenShop();Check(c.Equipped==locked&&s.Selected==locked,"Close/reopen keeps actual equipped loadout.");
            Check(s.gemStore.offers.Select(o=>o.euroPrice).SequenceEqual(new[]{2,5,10,50,100}),"Original five euro prices preserved.");
            Check(s.gemStore.offers.Select(o=>o.gems).SequenceEqual(new[]{200,550,1200,7000,16000}),"Original Gem quantities preserved.");
            if(s.purchaseProvider==null)
            {
                int before=PlayerPrefs.GetInt(WeaponCatalog.GemsKey);
                foreach(var offer in s.gemCards.GetComponentsInChildren<GemOfferButton>())
                {offer.GetComponent<Button>().onClick.Invoke();Check(s.noticePanel.activeSelf&&s.noticeTitle.text=="STORE UNAVAILABLE"&&PlayerPrefs.GetInt(WeaponCatalog.GemsKey)==before,"Unavailable pack grants nothing: "+offer.offerIndex);s.CloseNotice();}
            }
            Check(!s.weaponScroll.horizontal&&s.weaponScroll.vertical&&s.weaponScroll.viewport.GetComponent<RectMask2D>()!=null,"One clipped vertical scroll; no horizontal scrolling.");
            foreach(var button in s.shopPanel.GetComponentsInChildren<Button>(true))for(int i=0;i<button.onClick.GetPersistentEventCount();i++)
                Check(button.onClick.GetPersistentTarget(i)!=null&&!string.IsNullOrEmpty(button.onClick.GetPersistentMethodName(i)),"Persistent event: "+button.name);
            Check(s.header.Find("Coins").GetComponent<Button>().onClick.GetPersistentMethodName(0)=="ShowGems"&&s.header.Find("Gems").GetComponent<Button>().onClick.GetPersistentMethodName(0)=="ShowGems","Both wallet shortcuts retain ShowGems binding.");
        }
        catch(Exception e){results.Add("FAILED: "+e.Message);}
        finally
        {
            foreach(var p in saved)if(p.Value.HasValue)PlayerPrefs.SetInt(p.Key,p.Value.Value);else PlayerPrefs.DeleteKey(p.Key);
            if(hadEquipped)PlayerPrefs.SetString(WeaponCatalog.EquippedKey,equipped);else PlayerPrefs.DeleteKey(WeaponCatalog.EquippedKey);
            PlayerPrefs.Save();s.ActualizeazaInterfata();s.OpenShop();if(!wasOpen)s.CloseShop();
            bool restored=saved.All(p=>PlayerPrefs.HasKey(p.Key)==p.Value.HasValue&&(!p.Value.HasValue||PlayerPrefs.GetInt(p.Key)==p.Value.Value))&&PlayerPrefs.HasKey(WeaponCatalog.EquippedKey)==hadEquipped&&(!hadEquipped||PlayerPrefs.GetString(WeaponCatalog.EquippedKey)==equipped);
            results.Add((restored?"PASS: ":"FAIL: ")+"All touched PlayerPrefs restored exactly, including absent keys.");
            File.WriteAllLines(DirectoryPath+"/Regression.txt",results);
        }
        return string.Join("\n",results);
    }

    // Temporary Game View sizes, removed after validation; no project/player settings changed.
    public static void SetSize(int width,int height)
    {
        var asm=typeof(Editor).Assembly;var type=asm.GetType("UnityEditor.GameViewSizes");
        var singleton=typeof(ScriptableSingleton<>).MakeGenericType(type);
        var sizes=singleton.GetProperty("instance",Flags).GetValue(null,null);
        var group=type.GetProperty("currentGroup",Flags).GetValue(sizes,null);
        var sizeType=asm.GetType("UnityEditor.GameViewSizeType");
        var size=Activator.CreateInstance(asm.GetType("UnityEditor.GameViewSize"),new[]{Enum.Parse(sizeType,"FixedResolution"),(object)width,height,"Canva QA "+width+"x"+height});
        group.GetType().GetMethod("AddCustomSize",Flags).Invoke(group,new[]{size});
        int index=(int)group.GetType().GetMethod("GetTotalCount",Flags).Invoke(group,null)-1;
        var view=asm.GetType("UnityEditor.GameView");var window=EditorWindow.GetWindow(view);var property=view.GetProperty("selectedSizeIndex",Flags);
        if(SessionState.GetInt("CanvaShopOriginalSize",-1)<0)SessionState.SetInt("CanvaShopOriginalSize",(int)property.GetValue(window,null));
        property.SetValue(window,index,null);window.Focus();
    }
    public static void RestoreSize()
    {
        var asm=typeof(Editor).Assembly;var type=asm.GetType("UnityEditor.GameViewSizes");var singleton=typeof(ScriptableSingleton<>).MakeGenericType(type);var sizes=singleton.GetProperty("instance",Flags).GetValue(null,null);var group=type.GetProperty("currentGroup",Flags).GetValue(sizes,null);
        var texts=(string[])group.GetType().GetMethod("GetDisplayTexts",Flags).Invoke(group,null);
        for(int i=texts.Length-1;i>=0;i--)if(texts[i].StartsWith("Canva QA "))group.GetType().GetMethod("RemoveCustomSize",Flags).Invoke(group,new object[]{i});
        var view=asm.GetType("UnityEditor.GameView");var window=EditorWindow.GetWindow(view);view.GetProperty("selectedSizeIndex",Flags).SetValue(window,SessionState.GetInt("CanvaShopOriginalSize",36),null);SessionState.EraseInt("CanvaShopOriginalSize");
    }
    public static string LayoutReport()
    {
        var s=Object.FindFirstObjectByType<ShopManager>();s.Layout();Canvas.ForceUpdateCanvases();
        var issues=new List<string>();
        foreach(var label in s.shopPanel.GetComponentsInChildren<TextMeshProUGUI>(false))
        {label.ForceMeshUpdate();if(label.isTextOverflowing)issues.Add("Text overflow: "+label.name+" = "+label.text);}
        foreach(var r in s.presentation.offerRects)
            if(r.anchoredPosition.x<-.1f||r.anchoredPosition.x+r.rect.width>s.gemCards.rect.width+.1f)issues.Add("Gem horizontal overflow: "+r.name);
        var selected=s.cardContent.GetComponentsInChildren<ShopWeaponCard>();
        if(selected.Any(c=>c.GetComponent<RectTransform>().rect.width<280))issues.Add("Cards too narrow.");
        if(s.presentation.offerRects.Last().rect.width!=s.gemCards.rect.width)issues.Add("Last offer does not span full width.");
        var min=s.safeArea.anchorMin;var max=s.safeArea.anchorMax;
        s.safeArea.anchorMin=new Vector2(.025f,.04f);s.safeArea.anchorMax=new Vector2(.975f,.94f);Canvas.ForceUpdateCanvases();s.presentation.Layout();Canvas.ForceUpdateCanvases();
        if(s.header.anchoredPosition.x<0||s.header.anchoredPosition.x+s.header.rect.width>s.safeArea.rect.width+.1f)issues.Add("Simulated safe area clips header.");
        s.safeArea.anchorMin=min;s.safeArea.anchorMax=max;s.Layout();Canvas.ForceUpdateCanvases();
        string report=Screen.width+"x"+Screen.height+": "+(issues.Count==0?"PASS layout, text overflow, Gem bounds, full-width offer, simulated safe insets.":string.Join("; ",issues));
        Directory.CreateDirectory(DirectoryPath);File.AppendAllText(DirectoryPath+"/Layouts.txt",report+"\n");return report;
    }
}
#endif
