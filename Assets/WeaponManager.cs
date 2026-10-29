using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    public Animator soldierAnimator;
    public WeaponCatalog catalog;
    [Range(0, 1)] public float globalPistolVolume = 1;
    [Range(0, 1)] public float globalSmgVolume = .4f;
    [Range(0, 1)] public float globalRifleVolume = .3f;
    public GameObject pistolObject, smgObject, rifleObject;
    public Transform pistolMuzzle, smgMuzzle, rifleMuzzle;
    public GameObject pistolFlashPrefab, smgFlashPrefab, rifleFlashPrefab;
    public AudioClip pistolSound, smgSound, rifleSound;
    [HideInInspector] public Transform currentFirePoint;
    [HideInInspector] public GameObject currentFlashPrefab;
    [HideInInspector] public AudioClip currentAudioClip;
    [HideInInspector] public float currentAudioVolume, currentFireRate;
    [HideInInspector] public bool isAutomatic;
    [HideInInspector] public int currentDamage = 25;
    public WeaponDefinition CurrentWeapon { get; private set; }
    GameObject equippedModel;

    void Start()
    {
        if (catalog != null) Equip(catalog.Equipped);
        else EquipLegacyPistol();
    }

    void Update()
    {
        if (Time.timeScale <= 0) return;
        if (Input.GetKeyDown(KeyCode.Alpha1)) EquipCategory(WeaponCategory.Pistol);
        if (Input.GetKeyDown(KeyCode.Alpha2)) EquipCategory(WeaponCategory.SMG);
        if (Input.GetKeyDown(KeyCode.Alpha3)) EquipCategory(WeaponCategory.Rifle);
    }

    void EquipCategory(WeaponCategory category)
    {
        if (catalog == null) return;
        WeaponDefinition best = null;
        foreach (var w in catalog.weapons)
            if (w.category == category && catalog.IsOwned(w) && (best == null || w.tier > best.tier)) best = w;
        if (best != null) { catalog.Equip(best); Equip(best); }
    }

    public bool Equip(WeaponDefinition weapon)
    {
        if (weapon == null || catalog == null || !catalog.IsOwned(weapon) || weapon.modelPrefab == null) return false;
        GameObject anchor = weapon.category == WeaponCategory.Pistol ? pistolObject : weapon.category == WeaponCategory.SMG ? smgObject : rifleObject;
        Transform muzzle = weapon.category == WeaponCategory.Pistol ? pistolMuzzle : weapon.category == WeaponCategory.SMG ? smgMuzzle : rifleMuzzle;
        if (anchor == null) return false;
        GetComponent<PlayerShoot>()?.StopShooting();
        if (equippedModel != null) { equippedModel.SetActive(false); Destroy(equippedModel); }
        equippedModel = Instantiate(weapon.modelPrefab, anchor.transform.parent);
        equippedModel.name = "Equipped_" + weapon.id;
        equippedModel.transform.localPosition = anchor.transform.localPosition;
        equippedModel.transform.localRotation = anchor.transform.localRotation;
        equippedModel.transform.localScale = anchor.transform.localScale;
        if (pistolObject != null) pistolObject.SetActive(false);
        if (smgObject != null) smgObject.SetActive(false);
        if (rifleObject != null) rifleObject.SetActive(false);
        currentFirePoint = new GameObject("FirePoint").transform;
        currentFirePoint.SetParent(equippedModel.transform, false);
        currentFirePoint.localPosition = muzzle != null ? muzzle.localPosition : new Vector3(0, .05f, .3f);
        // Models in the existing packs expose their barrel as a separate mesh.
        foreach (var mesh in equippedModel.GetComponentsInChildren<MeshFilter>())
        {
            if (!mesh.name.ToLowerInvariant().Contains("barrel") || mesh.sharedMesh == null) continue;
            Bounds b = mesh.sharedMesh.bounds;
            Vector3 tip = b.center + Vector3.forward * b.extents.z;
            currentFirePoint.position = mesh.transform.TransformPoint(tip);
            break;
        }
        currentDamage = weapon.damage;
        currentFireRate = weapon.FireInterval;
        isAutomatic = weapon.Automatic;
        CurrentWeapon = weapon;
        if (soldierAnimator != null) soldierAnimator.SetBool("HasRifle", weapon.category != WeaponCategory.Pistol);
        currentFlashPrefab = weapon.category == WeaponCategory.Pistol ? pistolFlashPrefab : weapon.category == WeaponCategory.SMG ? smgFlashPrefab : rifleFlashPrefab;
        currentAudioClip = weapon.category == WeaponCategory.Pistol ? pistolSound : weapon.category == WeaponCategory.SMG ? smgSound : rifleSound;
        currentAudioVolume = weapon.category == WeaponCategory.Pistol ? globalPistolVolume : weapon.category == WeaponCategory.SMG ? globalSmgVolume : globalRifleVolume;
        return true;
    }

    void EquipLegacyPistol()
    {
        if (pistolObject != null) pistolObject.SetActive(true);
        if (smgObject != null) smgObject.SetActive(false);
        if (rifleObject != null) rifleObject.SetActive(false);
        currentFirePoint = pistolMuzzle;
        currentFlashPrefab = pistolFlashPrefab;
        currentAudioClip = pistolSound;
        currentAudioVolume = globalPistolVolume;
        currentFireRate = .3f;
        currentDamage = 25;
        isAutomatic = false;
    }
}
