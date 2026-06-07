using System;
using UnityEngine;

/// <summary>
/// Simple attribute to mark a method as a button in the Unity Inspector.
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
public class ButtonAttribute : Attribute
{
    public string ButtonName { get; }
    public ButtonAttribute(string buttonName = null)
    {
        ButtonName = buttonName;
    }
}
