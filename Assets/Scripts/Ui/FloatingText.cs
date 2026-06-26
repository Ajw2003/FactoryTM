using Singleton;
using UnityEngine;
using TMPro;

public enum TextCase { Normal, Uppercase, Lowercase }

[CreateAssetMenu(fileName = "NewFloatingTextSettings", menuName = "UI/Floating Text Settings")]
public class FloatingTextSettings : ScriptableObject
{
    [Header("Appearance")]
    public Color textColor = Color.white;
    public float fontSize = 5f;
    public TMP_FontAsset font;
    public FontStyles fontStyle = FontStyles.Normal;
    public TextCase textCase = TextCase.Normal;

    [Header("Behavior")]
    public Vector3 spawnOffset = new Vector3(0, 1f, 0);
    public float floatSpeed = 2f;
    public float fadeDuration = 1.5f;
}

[RequireComponent(typeof(TextMeshPro))]
public class FloatingText : MonoBehaviour
{
    private TextMeshPro textMesh;
    private float floatSpeed;
    private float fadeDuration;
    private float fadeTimer;
    private Color initialColor;

    public void Setup(string textContent, FloatingTextSettings settings, Vector3 position)
    {
        textMesh = GetComponent<TextMeshPro>();

        // Apply Text Case
        switch (settings.textCase)
        {
            case TextCase.Uppercase: textContent = textContent.ToUpper(); break;
            case TextCase.Lowercase: textContent = textContent.ToLower(); break;
        }

        // Apply Appearance
        textMesh.text = textContent;
        textMesh.color = settings.textColor;
        textMesh.fontSize = settings.fontSize;
        if (settings.font != null) textMesh.font = settings.font;
        textMesh.fontStyle = settings.fontStyle;

        // Apply Behavior Data
        transform.position = position + settings.spawnOffset;
        floatSpeed = settings.floatSpeed;
        fadeDuration = settings.fadeDuration;
        
        initialColor = settings.textColor;
        fadeTimer = fadeDuration;

        // Render on top of 2D sprites
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer.sortingLayerName = "UI";
            meshRenderer.sortingOrder = 100;
        }
    }

    void Update()
    {
        // Move the text upwards
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;

        // Calculate fade
        fadeTimer -= Time.deltaTime;
        float alpha = Mathf.Clamp01(fadeTimer / fadeDuration);
        
        // Apply fading alpha
        textMesh.color = new Color(initialColor.r, initialColor.g, initialColor.b, alpha);

        // Delete when invisible
        if (fadeTimer <= 0)
        {
            Destroy(gameObject);
        }
    }
}

public class FloatingTextManager : SingletonBase<FloatingTextManager>
{
    public void Spawn(string text, Vector3 spawnPosition, FloatingTextSettings settings)
    {
        if (settings == null)
        {
            Debug.LogWarning("Tried to spawn floating text without passing settings!");
            return;
        }

        // 1. Create a brand new empty GameObject
        GameObject textObj = new GameObject("DynamicFloatingText");
        
        // 2. Set its initial position
        textObj.transform.position = spawnPosition;

        // 3. Add the FloatingText script. 
        // Note: Because of the [RequireComponent] attribute in FloatingText.cs, 
        // Unity will automatically add the TextMeshPro and RectTransform components for us!
        FloatingText floatingText = textObj.AddComponent<FloatingText>();
        
        // 4. (Optional) Force the RectTransform to center alignment if needed by your setup
        RectTransform rectTransform = textObj.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.sizeDelta = new Vector2(5, 2); // Default size to prevent wrapping issues
        }

        // 5. Initialize the behavior and appearance
        floatingText.Setup(text, settings, spawnPosition);
    }
}

