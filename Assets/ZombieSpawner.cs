using UnityEngine;
using UnityEngine.SceneManagement;

public class SpawnerManager : MonoBehaviour
{
    [Header("Ce Spawnăm")]
    public GameObject zombiePrefab;
    public Transform player;

    [Header("Setări Inițiale")]
    public float distantaSpawnFata = 80f;
    public float limitaStanga = -2.5f; 
    public float limitaDreapta = 2.5f;

    [Header("Dificultate")]
    public float intervalStart = 2f;      // La inceput apare unul la 2 secunde
    public float intervalMinim = 0.5f;    // Nu scade sub 0.5 secunde (ca sa fie jucabil)
    public float vitezaStart = 5f;        // Viteza initiala a zombilor
    public float vitezaMaxima = 15f;      // Viteza maxima (sa nu fie imposibil)

    private float timer = 0f; // Cronometrul intern

    void Update()
    {
        if (player == null) return;

        // Scadem timpul
        timer -= Time.deltaTime;

        // Cand cronometrul ajunge la 0, spawnan
        if (timer <= 0)
        {
            SpawnZombie();
            ResetTimer(); // Resetam cronometrul bazat pe dificultate
        }
    }

    void SpawnZombie()
    {
        // 1. Calculam pozitia
        float xRandom = Random.Range(limitaStanga, limitaDreapta);
        Vector3 pozitieSpawn = new Vector3(xRandom, 0, player.position.z + distantaSpawnFata);
        
        // 2. Cream zombiul rotit spre noi (180 grade)
        GameObject zombieNou = Instantiate(zombiePrefab, pozitieSpawn, Quaternion.Euler(0, 180, 0));

        // 3. --- DIFICULTATE VITEZA ---
        // Calculam viteza in functie de scor
        int scorCurent = 0;
        if(ScoreManager.instance != null) scorCurent = ScoreManager.instance.GetScore();

        // Formula: Viteza creste cu 1 punct la fiecare 50 de scor
        float vitezaCalculata = vitezaStart + (scorCurent / 50f);
        
        // Nu depasim viteza maxima
        if(vitezaCalculata > vitezaMaxima) vitezaCalculata = vitezaMaxima;

        // Setam viteza pe zombiul nou creat
        zombieNou.GetComponent<ZombieMove>().vitezaZombie = vitezaCalculata;
    }

    void ResetTimer()
    {
        // --- DIFICULTATE TIMP ---
        int scorCurent = 0;
        if (ScoreManager.instance != null) scorCurent = ScoreManager.instance.GetScore();

        // Formula: Timpul scade cu 0.1 secunde la fiecare 50 de scor
        float intervalCalculat = intervalStart - (scorCurent / 50f) * 0.1f;

        // Nu scadem sub minim (sa nu apara 100 pe secunda)
        if (intervalCalculat < intervalMinim) intervalCalculat = intervalMinim;

        timer = intervalCalculat;
    }

    public void RestartJoc()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}