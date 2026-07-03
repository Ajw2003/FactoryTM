using Managers;
using Placeables;
using Ui;
using Weapons;
using Nodes;
using EventTypes;
using EventTypes.InventoryEvents;
using EventTypes.InputEvents;
using UnityEngine;

public class TwoDCollision : MonoBehaviour
{
    /// Creates an Axis-Aligned Bounding Box (AABB) that encapsulates a rotated rectangle.
    /// Assumes 'x' and 'y' represent the center point of the original rectangle.
    public static Rectangle2D CreateFromRotated(float x, float y, float width, float height, float angleRadians)
    {
        // Use absolute values because dimensions cannot be negative
        float cos = Mathf.Abs(Mathf.Cos(angleRadians));
        float sin = Mathf.Abs(Mathf.Sin(angleRadians));
        
        // Project the original width and height onto the X and Y axes
        float newWidth = (width * cos) + (height * sin);
        float newHeight = (width * sin) + (height * cos);
        
        return new Rectangle2D
        {
            // Calculate the bottom-left (or top-left depending on your coordinate system) corner
            X = x - (newWidth / 2f),
            Y = y - (newHeight / 2f),
            Width = newWidth,
            Height = newHeight,
        };
    }
}

public class Rectangle2D
{
    public float X;
    public float Y;
    public float Width;
    public float Height;
    
    /// Standard AABB (Axis-Aligned Bounding Box) intersection test.
    public static bool CheckCollision(Rectangle2D r1, Rectangle2D r2)
    {
        return r1.X < r2.X + r2.Width && 
               r1.X + r1.Width > r2.X && 
               r1.Y < r2.Y + r2.Height && 
               r1.Y + r1.Height > r2.Y;
    }
}
