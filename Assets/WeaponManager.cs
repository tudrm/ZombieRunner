using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    [Header("Referinte")]
    public Animator soldierAnimator; 

    [Header("Arme (Modele 3D)")]
    public GameObject pistolObject;
    public GameObject smgObject;
    public GameObject rifleObject;

    [Header("Puncte de Tragere")]
    public Transform pistolMuzzle; 
    public Transform smgMuzzle;    
    public Transform rifleMuzzle;  

    [Header("Efecte Vizuale")]
    public GameObject pistolFlashPrefab;
    public GameObject smgFlashPrefab;
    public GameObject rifleFlashPrefab;

    [Header("Sunete Arme")]
    public AudioClip pistolSound;
    public AudioClip smgSound;
    public AudioClip rifleSound;

    // Variabile interne
    [HideInInspector] public Transform currentFirePoint; 
    [HideInInspector] public GameObject currentFlashPrefab; 
    [HideInInspector] public AudioClip currentAudioClip; 
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

        // CONFIGURARE PISTOL (MODIFICAT)
        currentFirePoint = pistolMuzzle;
        currentFlashPrefab = pistolFlashPrefab;
        currentAudioClip = pistolSound;
        
        // AICI E SCHIMBAREA MARE:
        // Punem 0.05f (50 milisecunde). E practic instantaneu pentru degetul uman,
        // dar pastram o mica limita tehnica sa nu blocheze jocul daca ai un macro.
        currentFireRate = 0.05f; 
        
        isAutomatic = false; // Trebuie apasat de fiecare data
    }

    void EquipSMG1()
    {
        pistolObject.SetActive(false);
        smgObject.SetActive(true);
        rifleObject.SetActive(false);

        if (soldierAnimator != null) soldierAnimator.SetBool("HasRifle", true);

        // CONFIGURARE SMG (MODIFICAT)
        currentFirePoint = smgMuzzle;
        currentFlashPrefab = smgFlashPrefab;
        currentAudioClip = smgSound;
        
        // SMG-ul trage MEDIU (mai rar decat Rifle, dar automat)
        // 0.15f inseamna cam 6-7 gloante pe secunda
        currentFireRate = 0.12f; 
        
        isAutomatic = true; 
    }

    void EquipRifle1()
    {
        pistolObject.SetActive(false);
        smgObject.SetActive(false);
        rifleObject.SetActive(true);

        if (soldierAnimator != null) soldierAnimator.SetBool("HasRifle", true);

        // CONFIGURARE RIFLE (MODIFICAT)
        currentFirePoint = rifleMuzzle;
        currentFlashPrefab = rifleFlashPrefab;
        currentAudioClip = rifleSound;
        
        // RIFLE-ul trage FOARTE RAPID (Mitraliera grea)
        // 0.08f inseamna cam 12 gloante pe secunda
        currentFireRate = 0.10f; 
        
        isAutomatic = true; 
    }
}