using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    public GameObject loadingPanel; 
    public Image loadingBar;        
    private bool isLoading;
    public RectTransform menuSafeArea, menuArtwork, playButton, shopButton, lockerButton, settingsButton;
    private Vector2 menuSize;
    private Rect lastSafeArea;

    void Update()
    {
        if (menuSafeArea == null) return;
        if (menuSize == new Vector2(Screen.width, Screen.height) && lastSafeArea == Screen.safeArea) return;
        menuSize = new Vector2(Screen.width, Screen.height);
        var scaler = GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = Screen.width > Screen.height * 1.15f ? new Vector2(1440, 810) : new Vector2(720, 1280);
            scaler.matchWidthOrHeight = 0;
        }
        lastSafeArea = Screen.safeArea;
        menuSafeArea.anchorMin = new Vector2(lastSafeArea.xMin / Screen.width, lastSafeArea.yMin / Screen.height);
        menuSafeArea.anchorMax = new Vector2(lastSafeArea.xMax / Screen.width, lastSafeArea.yMax / Screen.height);
        menuSafeArea.offsetMin = menuSafeArea.offsetMax = Vector2.zero;
        bool wide = Screen.width > Screen.height * 1.15f;
        Place(menuArtwork, 0, 0, wide ? .6f : 1, 1);
        if (wide)
        {
            Place(playButton, .64f, .66f, .94f, .84f);
            Place(shopButton, .64f, .46f, .94f, .62f);
            Place(lockerButton, .64f, .26f, .94f, .42f);
            Place(settingsButton, .64f, .08f, .94f, .22f);
        }
        else
        {
            Place(playButton, .18f, .255f, .82f, .33f);
            Place(shopButton, .18f, .165f, .49f, .235f);
            Place(lockerButton, .51f, .165f, .82f, .235f);
            Place(settingsButton, .33f, .075f, .67f, .145f);
        }
    }

    static void Place(RectTransform rect, float x0, float y0, float x1, float y1)
    {
        rect.anchorMin = new Vector2(x0, y0); rect.anchorMax = new Vector2(x1, y1);
        rect.offsetMin = rect.offsetMax = Vector2.zero; rect.localScale = Vector3.one;
    }

    public void StartGame()
    {
        if (isLoading) return;
        isLoading = true;
        Time.timeScale = 1f;
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
