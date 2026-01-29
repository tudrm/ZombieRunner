using UnityEngine;
using UnityEngine.SceneManagement; // Avem nevoie de asta pentru a restarta jocul

public class PlayerHealth : MonoBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        // Verificăm dacă obiectul care ne-a lovit are eticheta "Inamic"
        if (collision.gameObject.CompareTag("Inamic"))
        {
            Moare();
        }
    }

    void Moare()
    {
        Debug.Log("Ai murit! Jocul se restartează...");
        
        // Restartăm scena curentă
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}