using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;

// A single isolated camera renders only the selected model, not twenty cameras/cards.
public class WeaponPreview : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    public RawImage display;
    [Header("Shop presentation (does not affect gameplay cameras)")]
    public bool transparentBackground;
    public Color rimColor = new Color(.6f, .78f, 1);
    Camera previewCamera;
    GameObject stage, model;
    Transform pivot;
    RenderTexture texture;
    bool dragging;
    float idleDelay;
    float yaw = -65;
    const int PreviewLayer = 31;

    public void Show(WeaponDefinition weapon)
    {
        EnsureStage();
        if (model != null) { model.SetActive(false); Destroy(model); }
        if (weapon == null || weapon.modelPrefab == null) return;
        pivot.localRotation = Quaternion.identity;
        model = Instantiate(weapon.modelPrefab, pivot);
        model.transform.localPosition = Vector3.zero;
        model.transform.localRotation = Quaternion.identity;
        model.transform.localScale = Vector3.one;
        foreach (var t in model.GetComponentsInChildren<Transform>(true)) t.gameObject.layer = PreviewLayer;
        foreach (var c in model.GetComponentsInChildren<Collider>(true)) c.enabled = false;
        foreach (var b in model.GetComponentsInChildren<Rigidbody>(true)) b.isKinematic = true;
        var renderers = model.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0) return;
        Bounds bounds = renderers[0].bounds;
        foreach (var r in renderers) bounds.Encapsulate(r.bounds);
        float scale = 3.4f / Mathf.Max(.01f, bounds.size.magnitude);
        model.transform.localScale = Vector3.one * scale;
        model.transform.localPosition = -(bounds.center - pivot.position) * scale;
        yaw = -65;
        pivot.localRotation = Quaternion.Euler(-12, yaw, 0);
        idleDelay = 2f;
    }

    void EnsureStage()
    {
        if (stage != null) return;
        stage = new GameObject("Shop weapon preview stage");
        stage.transform.position = new Vector3(10000, 10000, 10000);
        pivot = new GameObject("Model pivot").transform;
        pivot.SetParent(stage.transform, false);
        previewCamera = new GameObject("Weapon preview camera").AddComponent<Camera>();
        previewCamera.transform.SetParent(stage.transform, false);
        previewCamera.transform.localPosition = new Vector3(0, 0, -6);
        previewCamera.orthographic = true;
        previewCamera.orthographicSize = 1.35f;
        previewCamera.nearClipPlane = .1f;
        previewCamera.farClipPlane = 15;
        previewCamera.clearFlags = CameraClearFlags.SolidColor;
        previewCamera.backgroundColor = transparentBackground ? Color.clear : new Color(.09f, .105f, .092f, 1);
        previewCamera.cullingMask = 1 << PreviewLayer;
        previewCamera.allowHDR = false;
        var data = previewCamera.GetUniversalAdditionalCameraData();
        data.renderPostProcessing = false;
        data.volumeLayerMask = 0;
        data.renderShadows = false;
        texture = new RenderTexture(768, 512, 16, RenderTextureFormat.ARGB32) { name = "Shop selected weapon", antiAliasing = 2 };
        previewCamera.targetTexture = texture;
        display.texture = texture;
        AddLight("Key", new Vector3(35, -35, 0), Color.white, 2.4f);
        AddLight("Rim", new Vector3(20, 145, 0), rimColor, 1.7f);
        stage.SetActive(isActiveAndEnabled);
    }

    void AddLight(string label, Vector3 rotation, Color color, float intensity)
    {
        var light = new GameObject(label).AddComponent<Light>();
        light.transform.SetParent(stage.transform, false);
        light.transform.localRotation = Quaternion.Euler(rotation);
        light.type = LightType.Directional;
        light.cullingMask = 1 << PreviewLayer;
        light.color = color;
        light.intensity = intensity;
        light.shadows = LightShadows.None;
    }

    void Update()
    {
        if (previewCamera == null) return;
        var size = display.rectTransform.rect.size;
        float aspect = size.x / Mathf.Max(1, size.y);
        previewCamera.aspect = aspect;
        previewCamera.orthographicSize = Mathf.Max(1.15f, 1.85f / Mathf.Max(.3f, aspect));
        idleDelay -= Time.unscaledDeltaTime;
        if (!dragging && idleDelay <= 0) pivot.localRotation = Quaternion.Euler(-12, yaw + Mathf.Sin(Time.unscaledTime * .45f) * 12, 0);
    }

    public void OnDrag(PointerEventData e) { yaw -= e.delta.x * .35f; if (pivot != null) pivot.localRotation = Quaternion.Euler(-12, yaw, 0); }
    public void OnPointerDown(PointerEventData e) { dragging = true; }
    public void OnPointerUp(PointerEventData e) { dragging = false; idleDelay = 2; }
    void OnEnable() { if (stage != null) stage.SetActive(true); }
    void OnDisable() { dragging = false; if (stage != null) stage.SetActive(false); }
    void OnDestroy()
    {
        if (previewCamera != null) previewCamera.targetTexture = null;
        if (texture != null) { texture.Release(); Destroy(texture); }
        if (stage != null) Destroy(stage);
    }
}
