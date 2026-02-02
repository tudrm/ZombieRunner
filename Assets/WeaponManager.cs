using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    [Header("Referinte")]
    public Animator soldierAnimator; 

    [Header("Arme (Modele 3D)")]
    public GameObject pistolObject;
    public GameObject smgObject;
    public GameObject rifleObject;

    [Header("Puncte de Tragere (Muzzles)")]
    public Transform pistolMuzzle; // Trage aici Muzzle de la Pistol
    public Transform smgMuzzle;    // Trage aici Muzzle de la SMG
    public Transform rifleMuzzle;  // Trage aici Muzzle de la Rifle

    // Aceasta variabila va fi citita de scriptul de tragere
    // [HideInInspector] inseamna ca e public dar nu vrem sa il vedem in editor
    [HideInInspector] public Transform currentFirePoint; 

    void Start()
    {
        EquipPistol1(); // Start cu pistol
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

        // ACTUALIZAM PUNCTUL DE TRAGERE
        currentFirePoint = pistolMuzzle;
    }

    void EquipSMG1()
    {
        pistolObject.SetActive(false);
        smgObject.SetActive(true);
        rifleObject.SetActive(false);

        if (soldierAnimator != null) soldierAnimator.SetBool("HasRifle", true);

        // ACTUALIZAM PUNCTUL DE TRAGERE
        currentFirePoint = smgMuzzle;
    }

    void EquipRifle1()
    {
        pistolObject.SetActive(false);
        smgObject.SetActive(false);
        rifleObject.SetActive(true);

        if (soldierAnimator != null) soldierAnimator.SetBool("HasRifle", true);

        // ACTUALIZAM PUNCTUL DE TRAGERE
        currentFirePoint = rifleMuzzle;
    }
}