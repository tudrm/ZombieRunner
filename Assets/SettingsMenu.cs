using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    public GameObject panel;
    public RectTransform safeArea, sheet;
    public Slider effectsSlider, musicSlider;
    public TextMeshProUGUI effectsValue, musicValue;
    void Start()
    {
        effectsSlider.onValueChanged.AddListener(SetEffects);
        musicSlider.onValueChanged.AddListener(SetMusic);
    }
    public void Open()
    {
        panel.SetActive(true);
        effectsSlider.SetValueWithoutNotify(GameAudio.Effects);
        musicSlider.SetValueWithoutNotify(GameAudio.Music);
        RefreshLabels(); Layout();
    }
    public void Close() { PlayerPrefs.Save(); panel.SetActive(false); }
    public void SetEffects(float value) { GameAudio.SetEffects(value); RefreshLabels(); }
    public void SetMusic(float value) { GameAudio.SetMusic(value); RefreshLabels(); }
    void RefreshLabels()
    {
        effectsValue.text = Mathf.RoundToInt(GameAudio.Effects * 100) + "%";
        musicValue.text = Mathf.RoundToInt(GameAudio.Music * 100) + "%";
    }
    void Update()
    {
        if (!panel.activeSelf) return;
        Layout();
        if (Input.GetKeyDown(KeyCode.Escape)) Close();
    }
    void Layout()
    {
        var area = Screen.safeArea;
        safeArea.anchorMin = new Vector2(area.xMin / Screen.width, area.yMin / Screen.height);
        safeArea.anchorMax = new Vector2(area.xMax / Screen.width, area.yMax / Screen.height);
        safeArea.offsetMin = safeArea.offsetMax = Vector2.zero;
        float width = Mathf.Min(safeArea.rect.width - 48, 660);
        sheet.sizeDelta = new Vector2(width, Mathf.Min(safeArea.rect.height - 32, 570));
    }
}
