using Managers;
using Placeables;
using Ui;
using Weapons;
using Nodes;
using EventTypes;
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

    [Header("UI Canvas Settings")]
    public bool isCanvasUI = false;
    public string canvasName = "Canvas";
    public Vector2 anchorMin = new Vector2(0.5f, 0.5f);
    public Vector2 anchorMax = new Vector2(0.5f, 0.5f);
    public Vector2 pivot = new Vector2(0.5f, 0.5f);
    public Vector2 anchoredPosition = Vector2.zero;
    
    [Header("UI Animation Settings")]
    public bool usePingPongColor = false;
    public Color pingPongColorEnd = new Color(1f, 0.5f, 0f);
    public float pingPongSpeed = 5f;
    public float floatDistanceY = 0f;
    public float fadeStartNormalized = 0.5f;
}

