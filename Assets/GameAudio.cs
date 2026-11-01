using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

// Mixer gains preserve each weapon/source's original relative volume.
public class GameAudio : MonoBehaviour
{
    public const string EffectsKey = "ZR_EffectsVolume", MusicKey = "ZR_MusicVolume";
    static GameAudio instance;
    static AudioMixer mixer;
    public static float Effects => Mathf.Clamp01(PlayerPrefs.GetFloat(EffectsKey, 1));
    public static float Music => Mathf.Clamp01(PlayerPrefs.GetFloat(MusicKey, .7f));
    public static AudioMixer Mixer => mixer != null ? mixer : mixer = Resources.Load<AudioMixer>("GameAudio");

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() { instance = null; mixer = null; }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Boot()
    {
        if (instance != null) return;
        instance = new GameObject("Game Audio").AddComponent<GameAudio>();
        DontDestroyOnLoad(instance.gameObject);
    }
    void OnEnable() { SceneManager.sceneLoaded += SceneLoaded; }
    void OnDisable() { SceneManager.sceneLoaded -= SceneLoaded; }
    void Start() { Apply(); }
    void SceneLoaded(Scene scene, LoadSceneMode mode)
    {
        foreach (var root in scene.GetRootGameObjects())
            foreach (var source in root.GetComponentsInChildren<AudioSource>(true))
                if (source.outputAudioMixerGroup == null) Route(source);
        Apply();
    }
    public static void Route(AudioSource source, bool music = false)
    {
        if (source == null || Mixer == null) return;
        var groups = Mixer.FindMatchingGroups(music ? "Music" : "Effects");
        if (groups.Length > 0) source.outputAudioMixerGroup = groups[0];
    }
    public static void SetEffects(float value) { PlayerPrefs.SetFloat(EffectsKey, Mathf.Clamp01(value)); Apply(); }
    public static void SetMusic(float value) { PlayerPrefs.SetFloat(MusicKey, Mathf.Clamp01(value)); Apply(); }
    public static void Apply()
    {
        if (Mixer == null) return;
        Mixer.SetFloat("EffectsVolume", Decibels(Effects));
        Mixer.SetFloat("MusicVolume", Decibels(Music));
    }
    public static float Decibels(float value) => value <= .0001f ? -80 : Mathf.Log10(value) * 20;
    void OnApplicationPause(bool paused) { if (paused) PlayerPrefs.Save(); }
    void OnApplicationQuit() { PlayerPrefs.Save(); }
}
