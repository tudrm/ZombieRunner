using UnityEngine;

// Attach to future dynamically spawned sound/music prefabs, before they play.
[DefaultExecutionOrder(-1000), RequireComponent(typeof(AudioSource))]
public class AudioChannel : MonoBehaviour
{
    public bool music;
    void Awake() { foreach (var source in GetComponents<AudioSource>()) GameAudio.Route(source, music); }
}
