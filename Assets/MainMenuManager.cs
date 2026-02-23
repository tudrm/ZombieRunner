using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    public GameObject loadingPanel; 
    public Image loadingBar;        

    public void StartGame()
    {
        StartCoroutine(IncarcaNivelul("ScenaJoc")); 
    }

    IEnumerator IncarcaNivelul(string numeScena)
    {
        // 1. Aprindem ecranul de loading
        loadingPanel.SetActive(true);
        
        // GOLIM BARA fortat la inceput (0% = negru)
        loadingBar.fillAmount = 0f; 

        // Pauza ca sa apara poza pe ecran
        yield return new WaitForSeconds(0.1f);

        // 2. Incepem incarcarea zombilor, dar II SPUNEM SA NU INCEAPA JOCUL INCA
        AsyncOperation operatiune = SceneManager.LoadSceneAsync(numeScena);
        operatiune.allowSceneActivation = false; 

        float progresVizual = 0f;

        // 3. Cat timp nivelul se incarca in fundal...
        while (!operatiune.isDone)
        {
            // Tinta de incarcare a telefonului
            float progresReal = Mathf.Clamp01(operatiune.progress / 0.9f);

            // TRUCUL MAGIC: Umplem bara lin pe ecran, chiar daca se incarca rapid!
            progresVizual = Mathf.MoveTowards(progresVizual, progresReal, 0.8f * Time.deltaTime);
            loadingBar.fillAmount = progresVizual;

            // Daca animatia barei a ajuns la 100% (1)
            if (progresVizual >= 1f)
            {
                // Dam voie jocului sa inceapa!
                operatiune.allowSceneActivation = true;
            }

            yield return null; 
        }
    }
}