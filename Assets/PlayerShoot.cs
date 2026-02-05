using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    [Header("Setari Generale")]
    public GameObject glontPrefab; 

    private AudioSource sunetArma; 
    private WeaponManager weaponManager; 

    // Variabile pentru controlul tragerii
    private bool seTineApasat = false;
    private float nextFireTime = 0f;

    void Start()
    {
        sunetArma = GetComponent<AudioSource>();
        weaponManager = GetComponent<WeaponManager>();
    }

    void Update()
    {
        // Daca tinem butonul apasat SI arma este automata
        if (seTineApasat && weaponManager.isAutomatic)
        {
            // Verificam daca a trecut timpul de racire (FireRate)
            if (Time.time >= nextFireTime)
            {
                ExecutaTragere();
                // Setam urmatorul timp cand avem voie sa tragem
                nextFireTime = Time.time + weaponManager.currentFireRate;
            }
        }
    }

    // Aceasta functie e chemata cand apesi butonul pe ecran (pointer down)
    public void StartShooting()
    {
        // 1. Tragem imediat primul glont (indiferent de arma)
        // Verificam totusi FireRate-ul ca sa nu poata spama click-uri infinite la pistol
        if (Time.time >= nextFireTime)
        {
            ExecutaTragere();
            nextFireTime = Time.time + weaponManager.currentFireRate;
        }

        // 2. Activam modul "Tin Apasat"
        seTineApasat = true;
    }

    // Aceasta functie e chemata cand ridici degetul (pointer up)
    public void StopShooting()
    {
        seTineApasat = false;
    }

    // Functia care face efectiv treaba (Instantierea)
    void ExecutaTragere()
    {
        if (weaponManager == null || glontPrefab == null) return;

        Transform punctTragere = weaponManager.currentFirePoint;
        if (punctTragere == null) return;

        GameObject efect = weaponManager.currentFlashPrefab;
        AudioClip sunet = weaponManager.currentAudioClip;

        // GLONT
        Instantiate(glontPrefab, punctTragere.position, transform.rotation);

        // MUZZLE FLASH
        if (efect != null)
        {
            GameObject flash = Instantiate(efect, punctTragere.position, punctTragere.rotation);
            flash.transform.parent = punctTragere;
            Destroy(flash, 0.1f);
        }

        // SUNET (Specific armei)
        if(sunetArma != null && sunet != null)
        {
            // Putem folosi PlayOneShot ca sa se suprapuna sunetele la SMG (suna mai intens)
            sunetArma.PlayOneShot(sunet);
        }
    }
}