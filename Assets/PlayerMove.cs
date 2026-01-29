using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [Header("Viteze")]
    public float vitezaInainte = 15f; // Viteza constanta in fata
    
    [Header("Setari Touch & PC")]
    public float limitaStangaDreapta = 5f; // Cat de mult poti merge in laterale (limita drumului)
    public float sensibilitateTouch = 0.01f; // Cat de fin se misca pe telefon
    public float vitezaTaste = 10f; // Viteza pe PC (A/D)

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // 1. Calculam miscarea (Momentan e 0)
        float miscareX = 0f;

        // --- VARIANTA PC (Taste) ---
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            miscareX = -vitezaTaste * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            miscareX = vitezaTaste * Time.deltaTime;
        }

        // --- VARIANTA TELEFON (Touch / Swipe) ---
        if (Input.touchCount > 0)
        {
            Touch atingere = Input.GetTouch(0);

            // Daca degetul se misca pe ecran
            if (atingere.phase == TouchPhase.Moved)
            {
                // deltaPosition.x ne zice cat s-a miscat degetul fata de frame-ul trecut
                miscareX = atingere.deltaPosition.x * sensibilitateTouch;
            }
        }

        // 2. Aplicam miscarea la pozitia actuala
        Vector3 pozitieNoua = transform.position;
        
        // Adaugam miscarea laterala (X)
        pozitieNoua.x += miscareX;
        
        // Miscare constanta in fata (Z) - Optional, daca vrei sa mearga singur
        // Daca drumul se misca sub el, lasa linia asta comentata sau sterge-o.
        // pozitieNoua.z += vitezaInainte * Time.deltaTime; 

        // 3. Limitam miscarea sa nu iasa de pe drum (Clamp)
        pozitieNoua.x = Mathf.Clamp(pozitieNoua.x, -limitaStangaDreapta, limitaStangaDreapta);

        // 4. Mutam playerul
        transform.position = pozitieNoua;
    }
}