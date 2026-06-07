using System;

[AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
public class ButtonAttribute : Attribute
{
    public string ButtonName { get; }

    public ButtonAttribute(string buttonName = null)
    {
        ButtonName = buttonName;
    }
}
