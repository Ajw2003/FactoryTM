using Managers;
using Placeables;
using Ui;
using Weapons;
using Nodes;
using EventTypes;
using EventTypes.InventoryEvents;
using EventTypes.InputEvents;
namespace Weapons
{
    using UnityEngine;
    
    public class ExplosionVisual : MonoBehaviour
    {
        private LineRenderer lineRenderer;
        private float maxRadius;
        private float currentRadius = 0f;
        private float expansionSpeed;
        private float fadeSpeed;
        private float alpha = 1f;
        
        public void Initialize(float radius, Color color)
        {
            maxRadius = radius;
            expansionSpeed = radius / 0.15f; // Expands fully in 0.15 seconds
            fadeSpeed = 1f / 0.3f; // Fades out in 0.3 seconds after expanding
            
            lineRenderer = gameObject.AddComponent<LineRenderer>();
            
            // Find a basic sprite shader so it works in 2D without lighting issues
            Shader spriteShader = Shader.Find("Sprites/Default");
            if (spriteShader != null)
            {
                lineRenderer.material = new Material(spriteShader);
            }
            
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;
            lineRenderer.startWidth = 0.15f;
            lineRenderer.endWidth = 0.15f;
            lineRenderer.useWorldSpace = false;
            
            int segments = 36;
            lineRenderer.positionCount = segments + 1;
            
            UpdateCircle(0.1f);
        }
        
        private void Update()
        {
            if (currentRadius < maxRadius)
            {
                currentRadius += expansionSpeed * Time.deltaTime;
                if (currentRadius > maxRadius) currentRadius = maxRadius;
                UpdateCircle(currentRadius);
            }
            else
            {
                alpha -= fadeSpeed * Time.deltaTime;
                if (alpha <= 0f)
                {
                    Destroy(gameObject);
                }
                else
                {
                    Color c = lineRenderer.startColor;
                    c.a = alpha;
                    lineRenderer.startColor = c;
                    lineRenderer.endColor = c;
                }
            }
        }
        
        private void UpdateCircle(float radius)
        {
            int segments = lineRenderer.positionCount - 1;
            float angle = 0f;
            for (int i = 0; i < (segments + 1); i++)
            {
                float x = Mathf.Sin(Mathf.Deg2Rad * angle) * radius;
                float y = Mathf.Cos(Mathf.Deg2Rad * angle) * radius;
                
                lineRenderer.SetPosition(i, new Vector3(x, y, 0));
                
                angle += (360f / segments);
            }
        }
    }
    
}


