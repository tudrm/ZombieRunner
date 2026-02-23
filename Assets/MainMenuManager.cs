using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // Asta ne lasă să schimbăm scenele!

public class MainMenuManager : MonoBehaviour
{
    // Această funcție va fi apelată de butonul PLAY
    public void StartGame()
    {
        // Încărcăm scena cu numele "ScenaJoc" (trebuie să fie exact numele fișierului tău)
        SceneManager.LoadScene("ScenaJoc"); 
    }
}