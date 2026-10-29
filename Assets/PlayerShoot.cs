using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    [Header("Setari Generale")]
    public GameObject glontPrefab; 

    private AudioSource sunetArma; 
    private WeaponManager weaponManager; 

    private bool seTineApasat = false;
    private float nextFireTime = 0f;

    void Start()
    {
        sunetArma = GetComponent<AudioSource>();
        weaponManager = GetComponent<WeaponManager>();
    }

    void Update()
    {
        if (Time.timeScale > 0 && seTineApasat && weaponManager != null && weaponManager.isAutomatic)
        {
            if (Time.time >= nextFireTime)
            {
                ExecutaTragere();
                nextFireTime = Time.time + weaponManager.currentFireRate;
            }
        }
    }

    public void StartShooting()
    {
        if (!isActiveAndEnabled || Time.timeScale <= 0 || weaponManager == null) return;
        if (Time.time >= nextFireTime)
        {
            ExecutaTragere();
            nextFireTime = Time.time + weaponManager.currentFireRate;
        }
        seTineApasat = true;
    }

    public void StopShooting()
    {
        seTineApasat = false;
    }

    void OnDisable() { seTineApasat = false; }

    void ExecutaTragere()
    {
        if (weaponManager == null || glontPrefab == null) return;

        Transform punctTragere = weaponManager.currentFirePoint;
        if (punctTragere == null) return;

        GameObject efect = weaponManager.currentFlashPrefab;
        AudioClip sunet = weaponManager.currentAudioClip;
        
        // Luam volumul setat in manager
        float volum = weaponManager.currentAudioVolume;

        // GLONT
        GameObject bullet = Instantiate(glontPrefab, punctTragere.position, transform.rotation);
        if (bullet.TryGetComponent<BulletMove>(out var projectile)) projectile.damage = weaponManager.currentDamage;

        // MUZZLE FLASH
        if (efect != null)
        {
            GameObject flash = Instantiate(efect, punctTragere.position, punctTragere.rotation);
            flash.transform.parent = punctTragere;
            Destroy(flash, 0.1f);
        }

        // SUNET CU VOLUM REGLABIL
        if(sunetArma != null && sunet != null)
        {
            // Aici e magia: al doilea parametru este volumul (0.0 la 1.0)
            sunetArma.PlayOneShot(sunet, volum);
        }
    }
}
