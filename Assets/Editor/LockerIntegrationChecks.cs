#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

// Focused Editor checks. Every temporary ownership/economy change is restored from disk.
public static class LockerIntegrationChecks
{
    const string Root = "LockerCanvaReview/";
    [Serializable] public class Entry { public string key, value, kind; public bool exists; }
    [Serializable] public class Snapshot { public List<Entry> entries = new List<Entry>(); }
    static List<string> results;
    static void Check(bool ok,string text) { results.Add((ok?"PASS: ":"FAIL: ")+text); if(!ok)throw new InvalidOperationException(text); }

    public static void CapturePrefs()
    {
        Directory.CreateDirectory(Root);
        if(File.Exists(Root+"PlayerPrefs-baseline.json"))throw new InvalidOperationException("Baseline exists; do not overwrite it.");
        var c=AssetDatabase.LoadAssetAtPath<WeaponCatalog>("Assets/Settings/WeaponCatalog.asset");
        var snapshot=new Snapshot();
        foreach(var k in c.weapons.Select(w=>WeaponCatalog.OwnedPrefix+w.id).Concat(new[]{WeaponCatalog.CoinsKey,WeaponCatalog.GemsKey,"HighScore"}))
            snapshot.entries.Add(new Entry{key=k,exists=PlayerPrefs.HasKey(k),kind="int",value=PlayerPrefs.GetInt(k).ToString()});
        snapshot.entries.Add(new Entry{key=WeaponCatalog.EquippedKey,exists=PlayerPrefs.HasKey(WeaponCatalog.EquippedKey),kind="string",value=PlayerPrefs.GetString(WeaponCatalog.EquippedKey)});
        foreach(var k in new[]{"ZR_EffectsVolume","ZR_MusicVolume"})snapshot.entries.Add(new Entry{key=k,exists=PlayerPrefs.HasKey(k),kind="float",value=PlayerPrefs.GetFloat(k).ToString("R",System.Globalization.CultureInfo.InvariantCulture)});
        File.WriteAllText(Root+"PlayerPrefs-baseline.json",JsonUtility.ToJson(snapshot,true));
    }
    public static bool PrefsMatch()
    {
        var s=JsonUtility.FromJson<Snapshot>(File.ReadAllText(Root+"PlayerPrefs-baseline.json"));
        return s.entries.All(e=>PlayerPrefs.HasKey(e.key)==e.exists&&(!e.exists||e.value==(e.kind=="int"?PlayerPrefs.GetInt(e.key).ToString():e.kind=="string"?PlayerPrefs.GetString(e.key):PlayerPrefs.GetFloat(e.key).ToString("R",System.Globalization.CultureInfo.InvariantCulture))));
    }
    public static void RestorePrefs()
    {
        var s=JsonUtility.FromJson<Snapshot>(File.ReadAllText(Root+"PlayerPrefs-baseline.json"));
        foreach(var e in s.entries)
            if(!e.exists)PlayerPrefs.DeleteKey(e.key);
            else if(e.kind=="int")PlayerPrefs.SetInt(e.key,int.Parse(e.value));
            else if(e.kind=="float")PlayerPrefs.SetFloat(e.key,float.Parse(e.value,System.Globalization.CultureInfo.InvariantCulture));
            else PlayerPrefs.SetString(e.key,e.value);
        PlayerPrefs.Save();
    }

    public static string Run()
    {
        if(!EditorApplication.isPlaying||SceneManager.GetActiveScene().name!="MainMenu")throw new InvalidOperationException("Run in MainMenu Play Mode.");
        if(!File.Exists(Root+"PlayerPrefs-baseline.json"))throw new InvalidOperationException("Capture baseline before testing.");
        var m=Object.FindFirstObjectByType<MainMenuManager>();var l=Object.FindFirstObjectByType<LockerMenu>();var s=l.shop;var c=l.catalog;
        results=new List<string>();WeaponCatalog empty=null;
        try
        {
            m.lockerButton.GetComponent<Button>().onClick.Invoke();
            Check(l.panel.activeSelf&&!s.shopPanel.activeSelf,"LOCKER opens separate inventory, Shop stays closed.");
            Check(l.Cards.Select(v=>v.Weapon).SequenceEqual(c.weapons.Where(c.IsOwned)),"Only real owned catalog weapons appear, including starter.");
            Check(l.Cards.All(v=>v.thumbnail.sprite==v.Weapon.thumbnail&&v.title.text==v.Weapon.displayName),"Card names and thumbnails match catalog.");
            var own=c.weapons.Where(c.IsOwned).ToArray();var other=own.First(w=>w!=c.Equipped);
            l.Cards.First(v=>v.Weapon==other).button.onClick.Invoke();
            Check(l.Selected==other&&l.titleText.text==other.displayName&&l.damageText.text==other.damage.ToString()&&l.rateText.text==other.roundsPerMinute.ToString(),"Owned card click updates name and stats.");
            Check(l.preview.display.texture!=null&&l.preview.display.texture.width==768,"Existing live 3D preview renderer reused.");
            var stage=GameObject.Find("Shop weapon preview stage");var pivot=stage.transform.Find("Model pivot");var before=pivot.localRotation;
            l.preview.OnDrag(new PointerEventData(EventSystem.current){delta=new Vector2(45,0)});
            Check(Quaternion.Angle(before,pivot.localRotation)>1,"Preview drag rotates the actual 3D weapon.");
            Check(l.Cards.First(v=>v.Weapon==other).state.text=="SELECTED"&&l.Cards.First(v=>v.Weapon==c.Equipped).state.text=="EQUIPPED","Selected and equipped are distinct before equip.");
            int coins=PlayerPrefs.GetInt(WeaponCatalog.CoinsKey),gems=PlayerPrefs.GetInt(WeaponCatalog.GemsKey);
            l.equipButton.onClick.Invoke();
            Check(c.Equipped==other&&PlayerPrefs.GetString(WeaponCatalog.EquippedKey)==other.id,"EQUIP uses existing saved catalog selection.");
            Check(!l.equipButton.interactable&&l.actionText.text=="EQUIPPED","Current weapon has disabled EQUIPPED action; no unsupported unequip.");
            Check(PlayerPrefs.GetInt(WeaponCatalog.CoinsKey)==coins&&PlayerPrefs.GetInt(WeaponCatalog.GemsKey)==gems,"Equip does not debit either wallet.");
            var locked=c.weapons.First(w=>!c.IsOwned(w));l.SelectWeapon(locked);l.EquipSelected();
            Check(l.Selected==other&&c.Equipped==other&&!l.Cards.Any(v=>v.Weapon==locked),"Unowned weapon cannot be selected or equipped.");
            l.SelectWeapon(new WeaponDefinition{id=other.id});
            Check(l.Selected==other,"Non-catalog object with an owned ID is rejected.");
            var instanceIds=l.Cards.Select(v=>v.GetInstanceID()).ToArray();
            for(int i=0;i<5;i++){l.closeButton.onClick.Invoke();m.lockerButton.GetComponent<Button>().onClick.Invoke();}
            Check(l.Cards.Select(v=>v.GetInstanceID()).SequenceEqual(instanceIds)&&l.Selected==other,"Repeated close/open retains selection without duplicate cards.");
            l.closeButton.onClick.Invoke();Check(!l.panel.activeSelf&&!stage.activeSelf,"Close hides Locker and its preview stage.");
            m.shopButton.GetComponent<Button>().onClick.Invoke();
            Check(s.shopPanel.activeSelf&&s.Selected==other&&s.actionText.text=="EQUIPPED","SHOP still opens original Shop and reflects Locker equipment.");
            PlayerPrefs.SetInt(WeaponCatalog.CoinsKey,locked.price);s.ActualizeazaInterfata();s.SelectCategory((int)locked.category);s.SelectWeapon(locked);s.purchaseButton.onClick.Invoke();
            Check(c.IsOwned(locked)&&c.Equipped==locked,"Existing Shop Buy unlocks and equips through unchanged catalog.");
            s.CloseShop();m.lockerButton.GetComponent<Button>().onClick.Invoke();
            Check(l.Cards.Any(v=>v.Weapon==locked)&&l.Cards.First(v=>v.Weapon==locked).state.text=="EQUIPPED","New Shop purchase appears immediately in Locker without restart.");
            l.SelectWeapon(locked);c.Equip(c.Starter);PlayerPrefs.DeleteKey(WeaponCatalog.OwnedPrefix+locked.id);l.EquipSelected();
            Check(c.Equipped==c.Starter&&l.Selected!=locked&&!l.Cards.Any(v=>v.Weapon==locked),"Ownership revoked before Equip is revalidated; stale selection removed.");
            foreach(var w in c.weapons)PlayerPrefs.DeleteKey(WeaponCatalog.OwnedPrefix+w.id);
            l.RefreshInventory();Check(l.Cards.Count==1&&l.Cards[0].Weapon==c.Starter&&!l.emptyText.gameObject.activeSelf,"Minimal inventory shows only the always-owned starter.");
            empty=ScriptableObject.CreateInstance<WeaponCatalog>();l.catalog=empty;l.RefreshInventory();
            Check(l.Cards.Count==0&&l.emptyText.gameObject.activeSelf&&!l.equipButton.interactable&&!l.preview.gameObject.activeSelf,"Empty catalog is handled safely with disabled action and hidden preview.");
            l.catalog=c;
            foreach(var w in c.weapons)PlayerPrefs.SetInt(WeaponCatalog.OwnedPrefix+w.id,1);
            l.RefreshInventory();l.Layout();Canvas.ForceUpdateCanvases();
            Check(l.Cards.Count==c.weapons.Length&&l.cardContent.GetComponentsInChildren<LockerWeaponCard>().Length==c.weapons.Length,"All-owned inventory creates exactly one active card per weapon.");
            Check(l.scroll.content.rect.height>l.scroll.viewport.rect.height&&!l.scroll.horizontal&&l.scroll.viewport.GetComponent<RectMask2D>()!=null,"Long inventory uses a clipped vertical scroll.");
            var center=RectTransformUtility.WorldToScreenPoint(null,l.scroll.viewport.TransformPoint(l.scroll.viewport.rect.center));
            var ev=new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left,position=center};
            l.scroll.verticalNormalizedPosition=1;l.scroll.OnBeginDrag(ev);ev.position=center+new Vector2(0,300);l.scroll.OnDrag(ev);l.scroll.OnEndDrag(ev);
            Check(l.scroll.verticalNormalizedPosition<.99f,"Touch-style upward drag scrolls the inventory.");
            l.scroll.StopMovement();l.scroll.verticalNormalizedPosition=0;Canvas.ForceUpdateCanvases();
            var last=(RectTransform)l.Cards[l.Cards.Count-1].transform;var corners=new Vector3[4];last.GetWorldCorners(corners);
            var top=l.scroll.viewport.InverseTransformPoint(corners[1]);var bottom=l.scroll.viewport.InverseTransformPoint(corners[0]);
            Check(top.y<=l.scroll.viewport.rect.yMax+1&&bottom.y>=l.scroll.viewport.rect.yMin-1,"Last owned card is fully reachable at the scroll end.");
            l.Close();m.settingsButton.GetComponent<Button>().onClick.Invoke();var settings=Object.FindFirstObjectByType<SettingsMenu>();
            Check(settings.panel.activeSelf,"SETTINGS retains existing destination.");
            var close=settings.panel.GetComponentsInChildren<Button>(true).First(b=>Enumerable.Range(0,b.onClick.GetPersistentEventCount()).Any(i=>b.onClick.GetPersistentMethodName(i)=="Close"));close.onClick.Invoke();
            Check(!settings.panel.activeSelf,"Settings existing close button still works.");
            foreach(var b in new[]{l.equipButton,l.closeButton,m.lockerButton.GetComponent<Button>()})
                Check(b.onClick.GetPersistentEventCount()==1&&b.onClick.GetPersistentTarget(0)!=null,"Valid persistent event: "+b.name);
            Check(l.panel.GetComponentsInChildren<Transform>(true).Sum(t=>GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject))==0,"No missing scripts in Locker hierarchy.");
        }
        catch(Exception e){results.Add("FAIL: "+e);}
        finally
        {
            l.catalog=c;if(empty!=null)Object.Destroy(empty);
            RestorePrefs();l.RefreshInventory();l.SelectWeapon(c.Equipped);l.Close();s.CloseShop();s.ActualizeazaInterfata();
            results.Add((PrefsMatch()?"PASS: ":"FAIL: ")+"All audited PlayerPrefs restored exactly, including absent keys.");
            File.WriteAllLines(Root+"InventoryChecks.txt",results);
        }
        return string.Join("\n",results);
    }

    public static string LayoutReport(bool simulateSafeArea=false)
    {
        var l=Object.FindFirstObjectByType<LockerMenu>();l.Layout();var issues=new List<string>();
        Action check=()=>{
            Canvas.ForceUpdateCanvases();
            foreach(var t in l.panel.GetComponentsInChildren<TextMeshProUGUI>()){t.ForceMeshUpdate();if(t.isTextOverflowing)issues.Add("Overflow: "+t.name+"="+t.text);}
            var h=l.presentation.header;if(h.anchoredPosition.x<0||h.anchoredPosition.x+h.rect.width>l.presentation.safeArea.rect.width+.1f)issues.Add("Header outside safe bounds");
            if(l.equipButton.GetComponent<RectTransform>().rect.height<60||l.closeButton.GetComponent<RectTransform>().rect.height<60)issues.Add("Small action target");
            if(l.Cards.Any(c=>((RectTransform)c.transform).rect.width>l.scroll.viewport.rect.width+.1f))issues.Add("Horizontal card overflow");
            if(l.presentation.details.anchoredPosition.y-l.presentation.details.rect.height<l.presentation.action.anchoredPosition.y)issues.Add("Preview overlaps action");
        };
        check();
        if(simulateSafeArea){var r=l.presentation.safeArea;var min=r.anchorMin;var max=r.anchorMax;r.anchorMin=new Vector2(.025f,.04f);r.anchorMax=new Vector2(.975f,.94f);l.presentation.Layout();check();r.anchorMin=min;r.anchorMax=max;l.Layout();}
        var bg=l.presentation.background;float actual=bg.rectTransform.rect.width/bg.rectTransform.rect.height;float wanted=bg.sprite.rect.width/bg.sprite.rect.height;if(Mathf.Abs(actual-wanted)>.001f)issues.Add("Background stretched");
        string report=Screen.width+"x"+Screen.height+": "+(issues.Count==0?"PASS text, safe area, bounds, touch targets, background aspect.":"FAIL "+string.Join("; ",issues));File.AppendAllText(Root+"Layouts.txt",report+"\n");return report;
    }
}
#endif
