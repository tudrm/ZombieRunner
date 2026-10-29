#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

// Integration checks use the real menu and player, and restore every touched save key.
public static class ShopIntegrationChecks
{
    static IEnumerator routine;
    static double nextStep, deadline;
    static readonly Dictionary<string, int?> savedInts = new Dictionary<string, int?>();
    static string savedEquipped;
    static bool hadEquipped;
    static readonly List<string> results = new List<string>();
    const string Report = "Library/ShopIntegrationChecks.txt";

    [MenuItem("Tools/ZombieRunner/Run Shop Integration Checks (Play Mode)")]
    public static void Begin()
    {
        if (!EditorApplication.isPlaying) throw new Exception("Run these checks in Play Mode from MainMenu.");
        if (routine != null) throw new Exception("Checks already running.");
        var catalog = AssetDatabase.LoadAssetAtPath<WeaponCatalog>("Assets/Settings/WeaponCatalog.asset");
        savedInts.Clear();
        foreach (string key in catalog.weapons.Select(w => WeaponCatalog.OwnedPrefix + w.id).Concat(new[] {WeaponCatalog.CoinsKey, WeaponCatalog.GemsKey, "HighScore"}))
            savedInts[key] = PlayerPrefs.HasKey(key) ? PlayerPrefs.GetInt(key) : (int?)null;
        hadEquipped = PlayerPrefs.HasKey(WeaponCatalog.EquippedKey);
        savedEquipped = PlayerPrefs.GetString(WeaponCatalog.EquippedKey);
        results.Clear();
        routine = Run(catalog);
        deadline = EditorApplication.timeSinceStartup + 120;
        nextStep = 0;
        EditorApplication.update += Tick;
        EditorApplication.playModeStateChanged += OnMode;
        File.WriteAllText(Report, "RUNNING; saves will be restored.");
    }

    static void OnMode(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingPlayMode && routine != null) Finish(new Exception("Test interrupted."));
    }

    static void Tick()
    {
        if (EditorApplication.timeSinceStartup < nextStep) return;
        try
        {
            if (EditorApplication.timeSinceStartup > deadline) throw new Exception("Integration timeout.");
            if (!routine.MoveNext()) { Finish(null); return; }
            nextStep = EditorApplication.timeSinceStartup + (routine.Current is float f ? f : .05);
        }
        catch (Exception e) { Finish(e); }
    }

    static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception("FAIL: " + message);
        results.Add("PASS: " + message);
    }

    static IEnumerator Run(WeaponCatalog c)
    {
        ShopAuthoring.Validate();
        results.Add("PASS: 20 unique catalog entries, models, thumbnails and progressive stats.");
        foreach (var w in c.weapons) PlayerPrefs.DeleteKey(WeaponCatalog.OwnedPrefix+w.id);
        PlayerPrefs.DeleteKey(WeaponCatalog.EquippedKey);
        PlayerPrefs.SetInt(WeaponCatalog.CoinsKey,0);
        var locked=c.weapons[1];
        Check(c.IsOwned(c.Starter) && c.Equipped == c.Starter,"Starter available on a fresh save.");
        Check(!c.Equip(locked) && c.Equipped==c.Starter,"Locked weapon cannot be equipped.");
        Check(!c.Purchase(locked,out _) && PlayerPrefs.GetInt(WeaponCatalog.CoinsKey)==0 && !c.IsOwned(locked),"Insufficient funds leave balance and inventory unchanged.");
        PlayerPrefs.SetInt(WeaponCatalog.CoinsKey,locked.price);
        Check(c.Purchase(locked,out _) && c.IsOwned(locked) && c.Equipped==locked && PlayerPrefs.GetInt(WeaponCatalog.CoinsKey)==0,"Exact-price purchase debits once, owns and equips weapon.");
        Check(!c.Purchase(locked,out _) && PlayerPrefs.GetInt(WeaponCatalog.CoinsKey)==0,"Duplicate purchase cannot charge twice.");
        PlayerPrefs.SetInt(WeaponCatalog.CoinsKey,1000000);
        foreach(var w in c.weapons)if(!c.IsOwned(w))Check(c.Purchase(w,out _),"Purchase "+w.id);
        var best=c.weapons.Last();c.Equip(best);
        SceneManager.LoadScene("MainMenu");
        yield return .6f;
        var shop=Object.FindFirstObjectByType<ShopManager>();
        Check(shop!=null && shop.catalog==c,"Existing MainMenu loads the shared catalog.");
        shop.OpenShop();
        Check(shop.Selected==best && c.IsOwned(locked),"Owned weapons and equipped loadout survive scene reload.");
        foreach(var w in c.weapons)
        {
            shop.SelectCategory((int)w.category);shop.SelectWeapon(w);
            yield return .07f;
            Check(shop.titleText.text==w.displayName && shop.damageText.text==w.damage.ToString()
                && shop.preview.display.texture!=null && shop.preview.display.texture.width>0,"Shop stats and 3D preview: "+w.id);
        }
        shop.SelectCategory(0);
        Check(shop.cardContent.GetComponentsInChildren<UnityEngine.UI.Button>(false).Length==10,"Pistol list has ten selectable cards.");
        shop.SelectCategory(1);
        Check(shop.cardContent.GetComponentsInChildren<UnityEngine.UI.Button>(false).Length==5,"SMG list has five selectable cards.");
        shop.SelectCategory(2);
        Check(shop.cardContent.GetComponentsInChildren<UnityEngine.UI.Button>(false).Length==5,"Rifle list has five selectable cards.");
        shop.CloseShop();
        Check(!shop.shopPanel.activeSelf,"Back closes shop without loading another scene.");
        c.Equip(best);
        SceneManager.LoadScene("ScenaJoc");
        yield return 1f;
        var wm=Object.FindFirstObjectByType<WeaponManager>();
        var shoot=Object.FindFirstObjectByType<PlayerShoot>();
        Check(wm!=null && shoot!=null && wm.CurrentWeapon==best,"ScenaJoc equips the saved rifle on the existing Player.");
        ScreenCapture.CaptureScreenshot("Library/ShopGameplay.png");
        var spawner=Object.FindFirstObjectByType<SpawnerManager>();
        Check(spawner!=null,"Existing zombie spawner is bound and running.");
        var fire=typeof(PlayerShoot).GetMethod("ExecutaTragere",BindingFlags.NonPublic|BindingFlags.Instance);
        foreach(var w in c.weapons)
        {
            Check(wm.Equip(w),"Gameplay equips "+w.id);
            Check(wm.currentDamage==w.damage && Mathf.Approximately(wm.currentFireRate,w.FireInterval),"Gameplay damage/cadence match catalog: "+w.id);
            var previous=new HashSet<int>(Object.FindObjectsByType<BulletMove>(FindObjectsSortMode.None).Select(b=>b.GetInstanceID()));
            fire.Invoke(shoot,null);
            var spawned=Object.FindObjectsByType<BulletMove>(FindObjectsSortMode.None).Where(b=>!previous.Contains(b.GetInstanceID())).ToArray();
            Check(spawned.Length==1 && spawned[0].damage==w.damage,"Actual projectile receives weapon damage: "+w.id);
            Check(Vector3.Distance(wm.currentFirePoint.position,wm.transform.position)<6,"Weapon/muzzle attached to player: "+w.id);
            yield return .07f;
        }
        yield return .5f;
        var zombie=Object.FindObjectsByType<ZombieHealth>(FindObjectsSortMode.None).FirstOrDefault(z=>!z.EsteMort());
        Check(zombie!=null,"Spawner creates the existing zombie prefab.");
        int before=PlayerPrefs.GetInt(WeaponCatalog.CoinsKey);
        zombie.PrimesteDamage(10000);zombie.PrimesteDamage(10000);
        Check(PlayerPrefs.GetInt(WeaponCatalog.CoinsKey)==before+25,"Zombie death grants 25 coins exactly once.");
        Time.timeScale=0;
        int count=Object.FindObjectsByType<BulletMove>(FindObjectsSortMode.None).Length;
        shoot.StartShooting();
        Check(Object.FindObjectsByType<BulletMove>(FindObjectsSortMode.None).Length==count,"Paused gameplay rejects shooting input.");
        Time.timeScale=1;
        SceneManager.LoadScene("MainMenu");
        yield return .5f;
    }

    static void Finish(Exception error)
    {
        EditorApplication.update-=Tick;
        EditorApplication.playModeStateChanged-=OnMode;
        (routine as IDisposable)?.Dispose();routine=null;
        foreach(var pair in savedInts)
        {
            if(pair.Value.HasValue)PlayerPrefs.SetInt(pair.Key,pair.Value.Value);
            else PlayerPrefs.DeleteKey(pair.Key);
        }
        if(hadEquipped)PlayerPrefs.SetString(WeaponCatalog.EquippedKey,savedEquipped);
        else PlayerPrefs.DeleteKey(WeaponCatalog.EquippedKey);
        PlayerPrefs.Save();Time.timeScale=1;
        if(error!=null)results.Add(error.ToString());
        results.Add("Original PlayerPrefs restored.");
        File.WriteAllText(Report,(error==null?"SUCCESS":"FAILED")+"\n"+string.Join("\n",results));
        if(error==null)Debug.Log("Shop integration checks passed; original save restored.");
        else Debug.LogError("Shop integration check failed: "+error.Message);
        if(EditorApplication.isPlaying)
        {
            if(SceneManager.GetActiveScene().name!="MainMenu")SceneManager.LoadScene("MainMenu");
            else Object.FindFirstObjectByType<ShopManager>()?.OpenShop();
        }
    }
}
#endif
