using EventSystems;
using UnityEngine;

public class AudioClipEvent : IEvent
{
    public AudioClip Clip;
    public AudioChannel Channel;
    public float Volume;
    public float Duration;
}


public enum AudioChannel
{
    Music,
    Sfx,
    Ambient,
    Null
}
