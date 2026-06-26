using System.Collections.Generic;
using Singleton;
using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    private TMP_Text textMesh;
    private bool isCanvasUI;
    private RectTransform rectTransform;
    private Vector2 startAnchoredPosition;
    private float floatDistanceY;
    private float fadeStartNormalized;
    private bool usePingPongColor;
    private Color initialColor;
    private Color pingPongColorEnd;
    private float pingPongSpeed;
    private float floatSpeed;
    private float fadeDuration;
    private float elapsed;

    public void Setup(string textContent, FloatingTextSettings settings, Vector3 position, Color? colorOverride = null)
    {
        isCanvasUI = settings.isCanvasUI;
        floatDistanceY = settings.floatDistanceY;
        fadeStartNormalized = settings.fadeStartNormalized;
        usePingPongColor = settings.usePingPongColor;
        pingPongColorEnd = settings.pingPongColorEnd;
        pingPongSpeed = settings.pingPongSpeed;
        floatSpeed = settings.floatSpeed;
        fadeDuration = settings.fadeDuration;
        initialColor = colorOverride ?? settings.textColor;
        elapsed = 0f;

        if (isCanvasUI)
        {
            textMesh = GetComponent<TextMeshProUGUI>();
            if (textMesh == null) textMesh = gameObject.AddComponent<TextMeshProUGUI>();
            rectTransform = GetComponent<RectTransform>();
            if (rectTransform == null) rectTransform = gameObject.AddComponent<RectTransform>();

            rectTransform.anchorMin = settings.anchorMin;
            rectTransform.anchorMax = settings.anchorMax;
            rectTransform.pivot = settings.pivot;
            rectTransform.anchoredPosition = settings.anchoredPosition;
            startAnchoredPosition = settings.anchoredPosition;
        }
        else
        {
            textMesh = GetComponent<TextMeshPro>();
            if (textMesh == null) textMesh = gameObject.AddComponent<TextMeshPro>();
            rectTransform = GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.sizeDelta = new Vector2(5, 2); // Default size to prevent wrapping issues
            }

            transform.position = position + settings.spawnOffset;

            // Render on top of 2D sprites
            Renderer rend = GetComponent<Renderer>();
            if (rend != null)
            {
                rend.sortingLayerName = "Ui";
                rend.sortingOrder = 100;
            }
        }

        // Apply Text Case
        switch (settings.textCase)
        {
            case TextCase.Uppercase: textContent = textContent.ToUpper(); break;
            case TextCase.Lowercase: textContent = textContent.ToLower(); break;
        }

        textMesh.text = textContent;
        textMesh.color = initialColor;
        textMesh.fontSize = settings.fontSize;
        if (settings.font != null) textMesh.font = settings.font;
        textMesh.fontStyle = settings.fontStyle;
        textMesh.alignment = TextAlignmentOptions.Center;

        // Force sorting layer immediately in Setup
        if (!isCanvasUI)
        {
            Renderer rend = GetComponent<Renderer>();
            if (rend != null)
            {
                rend.sortingLayerName = "Ui";
                rend.sortingOrder = 100;
            }
        }
        else
        {
            gameObject.layer = LayerMask.NameToLayer("UI");
        }
    }

    void Start()
    {
        // Re-enforce sorting layer and object layer in Start to override any TMPro mesh regeneration resets
        if (!isCanvasUI)
        {
            Renderer rend = GetComponent<Renderer>();
            if (rend != null)
            {
                rend.sortingLayerName = "Ui";
                rend.sortingOrder = 100;
            }
        }
        else
        {
            gameObject.layer = LayerMask.NameToLayer("UI");
        }
    }

    void Update()
    {
        elapsed += Time.unscaledDeltaTime;
        float t = Mathf.Clamp01(elapsed / fadeDuration);

        // Ping-pong color
        if (usePingPongColor)
        {
            float pingPong = Mathf.PingPong(elapsed * pingPongSpeed, 1f);
            textMesh.color = Color.Lerp(initialColor, pingPongColorEnd, pingPong);
        }

        // Calculate fading alpha
        float alpha = 1f;
        if (t > fadeStartNormalized)
        {
            float fadeProgress = (t - fadeStartNormalized) / (1f - fadeStartNormalized);
            alpha = Mathf.Lerp(1f, 0f, fadeProgress);
        }

        Color curColor = textMesh.color;
        textMesh.color = new Color(curColor.r, curColor.g, curColor.b, alpha);

        if (isCanvasUI)
        {
            // Move on canvas using anchoredPosition
            rectTransform.anchoredPosition = startAnchoredPosition + new Vector2(0f, Mathf.Lerp(0f, floatDistanceY, t));
        }
        else
        {
            // Move in 3D world space
            transform.position += Vector3.up * floatSpeed * Time.unscaledDeltaTime;
        }

        // Self-destruction
        if (elapsed >= fadeDuration)
        {
            if (FloatingTextManager.HasInstance)
            {
                FloatingTextManager.Instance.ReturnToPool(gameObject, isCanvasUI);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}

public class FloatingTextManager : SingletonBase<FloatingTextManager>
{
    private Queue<GameObject> canvasTextPool = new Queue<GameObject>();
    private Queue<GameObject> worldTextPool = new Queue<GameObject>();

    public void Spawn(string text, Vector3 spawnPosition, FloatingTextSettings settings, Color? colorOverride = null)
    {
        if (settings == null)
        {
            Debug.LogWarning("Tried to spawn floating text without passing settings!");
            return;
        }

        GameObject textObj = null;
        Queue<GameObject> pool = settings.isCanvasUI ? canvasTextPool : worldTextPool;

        while (pool.Count > 0)
        {
            GameObject pooled = pool.Dequeue();
            if (pooled != null)
            {
                textObj = pooled;
                textObj.SetActive(true);
                break;
            }
        }

        if (textObj == null)
        {
            textObj = new GameObject("DynamicFloatingText");
        }

        if (settings.isCanvasUI)
        {
            GameObject canvas = null;
            if (!string.IsNullOrEmpty(settings.canvasName))
            {
                canvas = GameObject.Find(settings.canvasName);
            }
            if (canvas == null)
            {
                canvas = GameObject.Find("Canvas");
            }
            if (canvas != null)
            {
                textObj.transform.SetParent(canvas.transform, false);
            }
        }
        else
        {
            textObj.transform.position = spawnPosition;
        }

        FloatingText floatingText = textObj.GetComponent<FloatingText>();
        if (floatingText == null)
        {
            floatingText = textObj.AddComponent<FloatingText>();
        }
        floatingText.Setup(text, settings, spawnPosition, colorOverride);
    }

    public void ReturnToPool(GameObject obj, bool isCanvas)
    {
        if (obj == null) return;
        obj.SetActive(false);
        Queue<GameObject> pool = isCanvas ? canvasTextPool : worldTextPool;
        if (!pool.Contains(obj))
        {
            pool.Enqueue(obj);
        }
    }
}

