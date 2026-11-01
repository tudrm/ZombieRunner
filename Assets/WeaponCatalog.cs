using System;
using UnityEngine;

public enum WeaponCategory { Pistol, SMG, Rifle }

[Serializable]
public class WeaponDefinition
{
    public string id;
    public string displayName;
    [TextArea] public string description;
    public WeaponCategory category;
    public int tier;
    public int price;
    [Min(0)] public int gemPrice;
    public int damage;
    public int roundsPerMinute;
    public GameObject modelPrefab;
    public Sprite thumbnail;
    public float FireInterval => 60f / Mathf.Max(1, roundsPerMinute);
    public bool Automatic => category != WeaponCategory.Pistol;
}

// One shared asset supplies both the shop and gameplay. IDs remain stable when names change.
[CreateAssetMenu(menuName = "ZombieRunner/Weapon Catalog")]
public class WeaponCatalog : ScriptableObject
{
    public WeaponDefinition[] weapons = Array.Empty<WeaponDefinition>();
    public const string EquippedKey = "ZR_EquippedWeapon";
    public const string OwnedPrefix = "ZR_Owned_";
    public const string CoinsKey = "MonedeSalvate";
    public const string GemsKey = "GemuriSalvate";
    public const int StartingCoins = 1500;

    public WeaponDefinition Find(string id) => Array.Find(weapons, w => w != null && w.id == id);
    public WeaponDefinition Starter => weapons.Length > 0 ? weapons[0] : null;
    public bool IsOwned(WeaponDefinition weapon) => weapon != null &&
        (weapon == Starter || PlayerPrefs.GetInt(OwnedPrefix + weapon.id, 0) == 1);

    public WeaponDefinition Equipped
    {
        get
        {
            var weapon = Find(PlayerPrefs.GetString(EquippedKey, ""));
            return IsOwned(weapon) ? weapon : Starter;
        }
    }

    public bool Equip(WeaponDefinition weapon)
    {
        if (!IsOwned(weapon)) return false;
        PlayerPrefs.SetString(EquippedKey, weapon.id);
        PlayerPrefs.Save();
        return true;
    }

    public bool Purchase(WeaponDefinition weapon, out string message)
        => Purchase(weapon, false, out message);

    public bool Purchase(WeaponDefinition weapon, bool useGems, out string message)
    {
        if (weapon == null || Find(weapon.id) != weapon) { message = "Weapon unavailable."; return false; }
        if (IsOwned(weapon)) { message = "Already in your arsenal."; return false; }
        string key = useGems ? GemsKey : CoinsKey;
        int cost = useGems ? weapon.gemPrice : weapon.price;
        int balance = PlayerPrefs.GetInt(key, useGems ? 50 : StartingCoins);
        if (cost <= 0 || balance < cost) { message = useGems ? "Not enough Gems." : "Not enough Coins."; return false; }
        PlayerPrefs.SetInt(key, balance - cost);
        PlayerPrefs.SetInt(OwnedPrefix + weapon.id, 1);
        PlayerPrefs.SetString(EquippedKey, weapon.id);
        PlayerPrefs.Save();
        message = weapon.displayName + " unlocked and equipped.";
        return true;
    }

    public static void AwardCoins(int amount)
    {
        if (amount <= 0) return;
        long balance = (long)PlayerPrefs.GetInt(CoinsKey, StartingCoins) + amount;
        PlayerPrefs.SetInt(CoinsKey, (int)Math.Min(int.MaxValue, balance));
    }
}
