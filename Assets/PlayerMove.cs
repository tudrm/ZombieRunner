using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [Header("Legaturi")]
    public Animator soldierAnimator; // <--- TRAGE AICI SURVIVALIST-UL (COPILUL)

    [Header("Viteze")]
    public float vitezaInainte = 15f; 
    
    [Header("Setari Touch & PC")]
    public float limitaStangaDreapta = 5f; 
    public float sensibilitateTouch = 0.01f; 
    public float vitezaTaste = 10f; 

    private float valoareAnimatie = 0f; // Tinem minte directia pentru animatie

    void Update()
    {
        // 1. Calculam miscarea (Momentan e 0)
        float miscareX = 0f;
        float targetAnimatie = 0f; // Unde vrea sa ajunga animatia (-1, 0, sau 1)

        // --- VARIANTA PC (Taste) ---
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            miscareX = -vitezaTaste * Time.deltaTime;
            targetAnimatie = -1f; // Animatie Stanga
        }
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            miscareX = vitezaTaste * Time.deltaTime;
            targetAnimatie = 1f; // Animatie Dreapta
        }

        // --- VARIANTA TELEFON (Touch / Swipe) ---
        if (Input.touchCount > 0)
        {
            Touch atingere = Input.GetTouch(0);

            if (atingere.phase == TouchPhase.Moved)
            {
                // Calculam miscarea fizica
                miscareX = atingere.deltaPosition.x * sensibilitateTouch;

                // Calculam animatia bazat pe cat de tare tragi
                // Daca tragi spre stanga, valoarea devine negativa. Spre dreapta, pozitiva.
                // Inmultim cu 0.5f ca sa fie nevoie de o miscare mai ampla pentru a inclina maxim.
                targetAnimatie = Mathf.Clamp(atingere.deltaPosition.x * 0.2f, -1f, 1f);
            }
        }

        // 2. Trimitem datele la ANIMATOR (Cu Smoothness)
        // Asta face trecerea fina, sa nu sara brusc animatia
        if (soldierAnimator != null)
        {
            // Lerp face trecerea lina de la valoarea curenta la cea dorita
            valoareAnimatie = Mathf.Lerp(valoareAnimatie, targetAnimatie, 10f * Time.deltaTime);
            soldierAnimator.SetFloat("InputX", valoareAnimatie);
        }

        // 3. Aplicam miscarea fizica la pozitie
        Vector3 pozitieNoua = transform.position;
        
        // Adaugam miscarea laterala (X)
        pozitieNoua.x += miscareX;
        
        // Limitam miscarea sa nu iasa de pe drum
        pozitieNoua.x = Mathf.Clamp(pozitieNoua.x, -limitaStangaDreapta, limitaStangaDreapta);

        // Mutam playerul
        transform.position = pozitieNoua;
    }
}