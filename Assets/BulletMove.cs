using UnityEngine;

public class BulletMove : MonoBehaviour
{
    [Header("Setari Glont")]
    public float viteza = 40f;
    public float timpViata = 2f;
    public int damage = 25; // 25 Damage x 4 Gloante = 100 Viata

    [Header("Efecte Vizuale")]
    // Aici am pus numele cerut de tine
    public GameObject collisionEffectPrefab; 

    void Start()
    {
        // Distruge glontul automat dupa 2 secunde daca nu atinge nimic
        Destroy(gameObject, timpViata);
    }

    void Update()
    {
        // Misca glontul in fata
        transform.Translate(Vector3.forward * viteza * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        // 1. Verificam daca am lovit ceva cu tag-ul "Inamic"
        if (other.CompareTag("Inamic"))
        {
            // Cautam scriptul TAU vechi "ZombieHealth" pe obiectul lovit
            ZombieHealth zombieScript = other.GetComponent<ZombieHealth>();

            // Daca l-am gasit, ii dam damage
            if (zombieScript != null)
            {
                zombieScript.PrimesteDamage(damage);
            }

            // --- AICI ESTE CODUL NOU PENTRU EFECT ---
            if (collisionEffectPrefab != null)
            {
                // Cream efectul exact unde e glontul
                GameObject effect = Instantiate(collisionEffectPrefab, transform.position, Quaternion.identity);
                
                // Il distrugem dupa 1 secunda (curatenie)
                Destroy(effect, 1f);
            }
            // ----------------------------------------

            // Distrugem GLONTUL
            Destroy(gameObject);
        }
        // 2. Daca lovim decorul (orice nu e Player si nu e Inamic)
        else if (!other.CompareTag("Player") && !other.CompareTag("Untagged"))
        {
            // Optional: Poti pune efectul si aici daca vrei sa iasa scantei din pereti
            if (collisionEffectPrefab != null)
            {
                GameObject effect = Instantiate(collisionEffectPrefab, transform.position, Quaternion.identity);
                Destroy(effect, 1f);
            }

            Destroy(gameObject);
        }
    }
}