using Managers;
using Placeables;
using Ui;
using Weapons;
using Nodes;
using EventTypes;
using EventTypes.InventoryEvents;
using EventTypes.InputEvents;
using UnityEngine;

[CreateAssetMenu(fileName = "AudioClipSo", menuName = "Scriptable Objects/AudioClipSo")]
public class AudioClipSo : ScriptableObject
{
    public float Volume;
    public float Duration;
    public AudioClip Clip;
    public AudioChannel Channel;
}

