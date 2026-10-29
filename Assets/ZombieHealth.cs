using UnityEngine;

public class ZombieHealth : MonoBehaviour
{
    [Header("Viata Zombie")]
    public int viata = 1;

    [Header("Curatenie (Auto-Distrugere)")]
    private Transform player;
    public float distantaStergere = 10f; 

    private Animator anim;
    private bool esteMort = false; // Ca sa stim sa nu ne mai miscam
    private Collider col; // Coliziunea (Hitbox-ul)

    void Start()
    {
        anim = GetComponent<Animator>();
        col = GetComponent<Collider>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    void Update()
    {
        // Daca e mort, nu mai verificam distanta, doar asteptam sa dispara
        if (esteMort) return;

        if (player != null)
        {
            if (transform.position.z < player.position.z - distantaStergere)
            {
                Destroy(gameObject); 
            }
        }
    }

    public void PrimesteDamage(int damageAmount)
    {
        if (esteMort) return; // Daca e deja mort, ignoram gloantele

        viata -= damageAmount;

        if (viata <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        esteMort = true;
        
        if(anim != null) 
        {
            anim.SetTrigger("Die"); 
        }

        if(col != null)
        {
            col.enabled = false;
        }

        if (ScoreManager.instance != null) ScoreManager.instance.AddScore(10);
        WeaponCatalog.AwardCoins(25);

        Destroy(gameObject, 3f);
    }
    
    // Functie publica pentru ca scriptul de miscare sa stie sa se opreasca
    public bool EsteMort()
    {
        return esteMort;
    }
}
