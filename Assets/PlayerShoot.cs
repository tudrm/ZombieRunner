using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    [Header("Setari Arma")]
    public GameObject glontPrefab; // Aici vom pune GlontRealist

    private AudioSource sunetArma; 
    private WeaponManager weaponManager; // Legatura cu scriptul care stie ce arma avem

    void Start()
    {
        sunetArma = GetComponent<AudioSource>();
        
        // Cautam scriptul WeaponManager pe acelasi obiect (Player)
        weaponManager = GetComponent<WeaponManager>();
    }

    // Aceasta functie este apelata de Butonul de Tragere de pe ecran
    public void Trage()
    {
        // 1. Siguranta: Verificam daca avem manager si glont
        if (weaponManager == null || glontPrefab == null) return;

        // 2. Intrebam Managerul: "De unde trag acum?"
        Transform punctTragereCurent = weaponManager.currentFirePoint;

        // Daca nu exista un punct setat (poate nu s-a incarcat arma), nu tragem
        if (punctTragereCurent == null) return;

        // 3. Cream glontul la pozitia Muzzle-ului activ
        // Folosim transform.rotation (rotatia corpului playerului), care e mereu drept in fata
        Instantiate(glontPrefab, punctTragereCurent.position, transform.rotation);
        
        // 4. Redam sunetul
        if(sunetArma != null)
        {
            // Micul truc: Oprim sunetul anterior ca sa putem trage rapid (taca-taca-taca)
            if(sunetArma.isPlaying)
            {
                sunetArma.Stop();
            }
            sunetArma.Play();
        }
    }
}