using System.Collections;
using TMPro;
using UnityEngine;

public class FloatingDamageText : MonoBehaviour
{
    public void Initialize(string text, Color color, Vector3 startPosition)
    {
        transform.position = startPosition;
        
        TextMeshPro tmp = gameObject.AddComponent<TextMeshPro>();
        tmp.text = text;
        tmp.color = color;
        tmp.fontSize = 4.5f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;
        
        // Render on top of 2D sprites
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer.sortingLayerName = "UI";
            meshRenderer.sortingOrder = 100;
        }

        StartCoroutine(Animate());
    }

    private IEnumerator Animate()
    {
        float duration = 0.5f;
        float elapsed = 0f;
        Vector3 startPos = transform.position;
        // Float slightly upwards and randomize horizontal drift slightly
        Vector3 endPos = startPos + new Vector3(Random.Range(-0.3f, 0.3f), 0.8f, 0f);
        
        TextMeshPro tmp = GetComponent<TextMeshPro>();

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // Ease-out position
            float tEase = t * (2f - t);
            transform.position = Vector3.Lerp(startPos, endPos, tEase);
            
            // Fade out
            if (tmp != null)
            {
                Color c = tmp.color;
                c.a = Mathf.Lerp(1f, 0f, t);
                tmp.color = c;
            }
            
            // Quick scale pop
            float scale = 1f;
            if (t < 0.2f)
            {
                scale = Mathf.Lerp(1f, 1.4f, t / 0.2f);
            }
            else
            {
                scale = Mathf.Lerp(1.4f, 0.8f, (t - 0.2f) / 0.8f);
            }
            transform.localScale = Vector3.one * scale;

            yield return null;
        }
        
        Destroy(gameObject);
    }
}
