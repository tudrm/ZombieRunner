using UnityEngine;

public class GameOverZone : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        // Daca intra Inamicul (scapat) SAU Playerul (impins de zombie)
        if (other.CompareTag("Inamic") || other.CompareTag("Player"))
        {
            Debug.Log("S-a atins zona de final!");
            
            // Nu mai dam RestartJoc()! Chemam Managerul sa afiseze meniul.
            if (GameManager.instance != null)
            {
                GameManager.instance.DeclanseazaGameOver();
            }
        }
    }
}