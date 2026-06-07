using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Object), true)]
[CanEditMultipleObjects]
public class ButtonEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var methods = target.GetType()
            .GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(m => m.GetCustomAttribute<ButtonAttribute>() != null && m.GetParameters().Length == 0);

        if (!methods.Any()) return;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Editor Actions", EditorStyles.boldLabel);

        foreach (var method in methods)
        {
            var attr = method.GetCustomAttribute<ButtonAttribute>();
            string buttonName = string.IsNullOrEmpty(attr.ButtonName) ? ObjectNames.NicifyVariableName(method.Name) : attr.ButtonName;

            if (GUILayout.Button(buttonName))
            {
                foreach (var t in targets)
                {
                    method.Invoke(t, null);
                }
            }
        }
    }
}
