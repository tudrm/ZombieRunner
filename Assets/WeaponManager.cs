using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    [Header("Referinte")]
    public Animator soldierAnimator; 

    // --- AICI SUNT SLIDERELE UNIVERSALE ---
    [Header("SETTINGS: MASTER VOLUME (Categorii)")]
    [Range(0f, 1f)] public float globalPistolVolume = 1f; 
    [Range(0f, 1f)] public float globalSmgVolume = 0.4f; 
    [Range(0f, 1f)] public float globalRifleVolume = 0.3f; 
    // ---------------------------------------

    [Header("ARMA 1: PISTOL STANDARD")]
    public GameObject pistolObject;
    public Transform pistolMuzzle; 
    public GameObject pistolFlashPrefab;
    public AudioClip pistolSound;

    [Header("ARMA 2: SMG STANDARD")]
    public GameObject smgObject;
    public Transform smgMuzzle;    
    public GameObject smgFlashPrefab;
    public AudioClip smgSound;

    [Header("ARMA 3: RIFLE STANDARD")]
    public GameObject rifleObject;
    public Transform rifleMuzzle;  
    public GameObject rifleFlashPrefab;
    public AudioClip rifleSound;

    // Variabile interne
    [HideInInspector] public Transform currentFirePoint; 
    [HideInInspector] public GameObject currentFlashPrefab; 
    [HideInInspector] public AudioClip currentAudioClip;
    [HideInInspector] public float currentAudioVolume; 
    [HideInInspector] public float currentFireRate;      
    [HideInInspector] public bool isAutomatic;           

    void Start()
    {
        EquipPistol1(); 
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) EquipPistol1();
        if (Input.GetKeyDown(KeyCode.Alpha2)) EquipSMG1();
        if (Input.GetKeyDown(KeyCode.Alpha3)) EquipRifle1();
    }

    void EquipPistol1()
    {
        pistolObject.SetActive(true);
        smgObject.SetActive(false);
        rifleObject.SetActive(false);

        if(soldierAnimator != null) soldierAnimator.SetBool("HasRifle", false);

        currentFirePoint = pistolMuzzle;
        currentFlashPrefab = pistolFlashPrefab;
        currentAudioClip = pistolSound;

        // AICI FOLOSIM VOLUMUL GLOBAL PENTRU TOATE PISTOALELE
        currentAudioVolume = globalPistolVolume; 

        currentFireRate = 0.05f; 
        isAutomatic = false; 
    }

    void EquipSMG1()
    {
        pistolObject.SetActive(false);
        smgObject.SetActive(true);
        rifleObject.SetActive(false);

        if (soldierAnimator != null) soldierAnimator.SetBool("HasRifle", true);

        currentFirePoint = smgMuzzle;
        currentFlashPrefab = smgFlashPrefab;
        currentAudioClip = smgSound;

        // VOLUM GLOBAL SMG
        currentAudioVolume = globalSmgVolume; 

        currentFireRate = 0.13f; 
        isAutomatic = true; 
    }

    void EquipRifle1()
    {
        pistolObject.SetActive(false);
        smgObject.SetActive(false);
        rifleObject.SetActive(true);

        if (soldierAnimator != null) soldierAnimator.SetBool("HasRifle", true);

        currentFirePoint = rifleMuzzle;
        currentFlashPrefab = rifleFlashPrefab;
        currentAudioClip = rifleSound;

        // VOLUM GLOBAL RIFLE
        currentAudioVolume = globalRifleVolume; 

        currentFireRate = 0.10f; 
        isAutomatic = true; 
    }
}