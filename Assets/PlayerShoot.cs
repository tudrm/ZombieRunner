using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    [Header("Setari Arma")]
    public GameObject glontPrefab;
    public Transform firePoint;

    private AudioSource sunetArma; 

    void Start()
    {
        sunetArma = GetComponent<AudioSource>();
    }

    void Update()
    {
        // AM STERS TOT DE AICI!
        // Nu mai verificam Input.GetButtonDown("Fire1").
        // Acum scriptul asteapta cuminte sa fie strigat de Butonul UI.
    }

    // Functia asta ramane Publica, ca sa o vada Butonul
    public void Trage()
    {
        // 1. Cream glontul
        if(glontPrefab != null && firePoint != null)
        {
            Instantiate(glontPrefab, firePoint.position, firePoint.rotation);
        }

        // 2. Redam sunetul
        if(sunetArma != null)
        {
            if(sunetArma.isPlaying)
            {
                sunetArma.Stop();
            }
            sunetArma.Play();
        }
    }
}