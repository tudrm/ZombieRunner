using UnityEngine;

public class ZombieMove : MonoBehaviour
{
    private ZombieHealth healthScript; 
    private Animator anim; // Referinta la Animator
    
    // Viteza este setata de SpawnerManager cand apare zombiul
    public float vitezaZombie = 5f; 

    void Start()
    {
        healthScript = GetComponent<ZombieHealth>();
        anim = GetComponent<Animator>(); // Conectam "creierul" animatiilor

        // --- REZOLVARE PROBLEMA 1: DIVERSITATE ---
        // Alegem random intre 0 si 1 pentru parametrul "TipMers"
        // Asta ii va face sa mearga diferit (schiopat vs normal)
        if (anim != null)
        {
            int stilMers = Random.Range(0, 2); // Alege 0 sau 1
            anim.SetInteger("TipMers", stilMers);
        }

        // --- REZOLVARE PROBLEMA 2: ALERGATUL ---
        // Verificam viteza primita. Daca e mare, activam animatia de alergare.
        if (anim != null)
        {
            // Daca viteza e mai mare de 7, consideram ca trebuie sa alerge
            if (vitezaZombie > 7f)
            {
                anim.SetBool("Alearga", true);
            }
            else
            {
                anim.SetBool("Alearga", false);
            }
        }
    }

    void Update()
    {
        // Daca e mort, nu se mai misca
        if (healthScript != null && healthScript.EsteMort()) return;

        // Miscarea fizica
        transform.Translate(Vector3.forward * vitezaZombie * Time.deltaTime);
    }
}