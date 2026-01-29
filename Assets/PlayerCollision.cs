using UnityEngine;

public class PlayerCollision : MonoBehaviour
{
    private bool jocTerminat = false; 

    void OnCollisionEnter(Collision collision)
    {
        if (jocTerminat) return; 

        if (collision.gameObject.CompareTag("Inamic"))
        {
            jocTerminat = true;
            Debug.Log("M-a prins! Astept meniul..."); 

            // Oprim Player
            PlayerMove moveScript = GetComponent<PlayerMove>();
            if (moveScript != null) moveScript.enabled = false;

            PlayerShoot shootScript = GetComponent<PlayerShoot>(); 
            if (shootScript != null) shootScript.enabled = false;

            // Oprim Zombie
            ZombieMove zombieMove = collision.gameObject.GetComponent<ZombieMove>();
            if(zombieMove != null) zombieMove.enabled = false; 

            // Animatie
            Animator zombieAnim = collision.gameObject.GetComponent<Animator>();
            if (zombieAnim != null) zombieAnim.SetTrigger("Ataca"); 

            // --- AICI ERA PROBLEMA TA ---
            // NU mai dam RestartAutomat! Chemam meniul.
            Invoke("ArataMeniul", 2f);
        }
    }

    void ArataMeniul()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.DeclanseazaGameOver();
        }
    }
}