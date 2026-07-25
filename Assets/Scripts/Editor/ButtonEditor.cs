using Managers;
using Placeables;
using Ui;
using Weapons;
using Nodes;
using EventTypes;
using EventTypes.InventoryEvents;
using EventTypes.InputEvents;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Linq;

/// <summary>
/// Custom editor that finds methods marked with [Button] and renders them as buttons in the inspector.
/// </summary>
[CustomEditor(typeof(MonoBehaviour), true)]
[CanEditMultipleObjects]
public class ButtonEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        // Get all methods with the ButtonAttribute
        var methods = target.GetType()
            .GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(m => m.GetCustomAttributes(typeof(ButtonAttribute), true).Length > 0);

        foreach (var method in methods)
        {
            var attr = (ButtonAttribute)method.GetCustomAttributes(typeof(ButtonAttribute), true)[0];
            string buttonName = string.IsNullOrEmpty(attr.ButtonName) ? method.Name : attr.ButtonName;

            if (GUILayout.Button(buttonName))
            {
                foreach (var obj in targets)
                {
                    method.Invoke(obj, null);
                }
            }
        }
    }
}

