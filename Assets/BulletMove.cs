using UnityEngine;

public class BulletMove : MonoBehaviour
{
    public float viteza = 100f;
    public int putereLovitura = 1;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        // In Unity 6 se foloseste linearVelocity
        if (rb != null)
        {
            rb.linearVelocity = transform.forward * viteza;
        }
        
        // Se distruge singur dupa 3 secunde daca nu atinge nimic, sa nu incarce memoria
        Destroy(gameObject, 3f);
    }

    void OnCollisionEnter(Collision collision)
    {
        // 1. Verificam daca am lovit un "Inamic"
        if (collision.gameObject.CompareTag("Inamic"))
        {
            ZombieHealth inamic = collision.gameObject.GetComponent<ZombieHealth>();

            if (inamic != null)
            {
                inamic.PrimesteDamage(putereLovitura);
            }
        }

        // 2. Distrugem glontul la orice impact
        Destroy(gameObject);
    }
}