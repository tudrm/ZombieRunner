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

    [Header("Sunete Arme & Volum")]
    public AudioClip pistolSound;
    [Range(0f, 1f)] public float pistolVolume = 1f; // Slider volum (Maxim)

    public AudioClip smgSound;
    [Range(0f, 1f)] public float smgVolume = 0.4f; // Slider volum (Mai incet)

    public AudioClip rifleSound;
    [Range(0f, 1f)] public float rifleVolume = 0.3f; // Slider volum (Cel mai incet, ca trage des)

    // Variabile interne
    [HideInInspector] public Transform currentFirePoint; 
    [HideInInspector] public GameObject currentFlashPrefab; 
    [HideInInspector] public AudioClip currentAudioClip;
    [HideInInspector] public float currentAudioVolume; // Aici tinem minte volumul armei curente
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
        
        // Audio
        currentAudioClip = pistolSound;
        currentAudioVolume = pistolVolume; // Setam volumul de pistol

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

        // Audio
        currentAudioClip = smgSound;
        currentAudioVolume = smgVolume; // Setam volumul de SMG

        currentFireRate = 0.15f; 
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

        // Audio
        currentAudioClip = rifleSound;
        currentAudioVolume = rifleVolume; // Setam volumul de Rifle

        currentFireRate = 0.08f; 
        isAutomatic = true; 
    }
}