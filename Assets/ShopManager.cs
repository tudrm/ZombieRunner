using UnityEngine;
using TMPro; // Obligatoriu pentru a lucra cu textele noi

public class ShopManager : MonoBehaviour
{
    public static ShopManager instance; // Ca sa putem chema banca din orice alt script

    [Header("Banii Jucatorului")]
    public int monede;
    public int gemuri;

    [Header("Legaturi Interfata (UI)")]
    public TextMeshProUGUI textMonede;
    public TextMeshProUGUI textGemuri;

    void Awake()
    {
        // Ne asiguram ca exista o singura banca in joc
        if(instance == null) 
        {
            instance = this;
        }
    }

    void Start()
    {
        // 1. Incarcam banii din memoria telefonului. 
        // Daca jucatorul intra prima data, ii dam 1500 monede si 50 gemuri de bun venit (poti modifica).
        monede = PlayerPrefs.GetInt("MonedeSalvate", 1000000); 
        gemuri = PlayerPrefs.GetInt("GemuriSalvate", 50);   

        // 2. Afisam banii pe ecran
        ActualizeazaInterfata();
    }

    // Functia care scrie pe ecran
    public void ActualizeazaInterfata()
    {
        textMonede.text = FormateazaNumar(monede);
        textGemuri.text = FormateazaNumar(gemuri);
    }

    // TRUCUL MAGIC PENTRU K si M
    string FormateazaNumar(int numar)
    {
        if (numar >= 1000000)
        {
            return (numar / 1000000f).ToString("0.#") + "M"; // Ex: 1500000 devine 1.5M
        }
        else if (numar >= 10000)
        {
            return (numar / 1000f).ToString("0.#") + "K"; // Ex: 10500 devine 10.5K
        }
        
        return numar.ToString(); // Daca e sub 10.000, il lasa normal (ex: 9999)
    }

    // --- FUNCTII PENTRU MAI TARZIU (Cumparaturi si Reward-uri) ---

    public void AdaugaMonede(int suma)
    {
        monede += suma;
        PlayerPrefs.SetInt("MonedeSalvate", monede); // Salvam in telefon
        ActualizeazaInterfata();
    }

    public void CheltuieMonede(int suma)
    {
        monede -= suma;
        PlayerPrefs.SetInt("MonedeSalvate", monede); // Salvam in telefon
        ActualizeazaInterfata();
    }
}