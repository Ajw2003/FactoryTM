using System;
using System.Collections.Generic;
using SOs;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterProfiles", menuName = "Scriptable Objects/CharacterProfiles")]
public class CharacterProfiles : ScriptableObject
{
    public List<Character> allCharacters;

    public Character GetCharacter(Speaker current) => allCharacters.Find(x => x.charSpeaker == current);

}

[Serializable]
public struct Character
{
    public Sprite sprite;
    public Color dialogueColor;
    public Speaker charSpeaker;
    public float charPitch;
}
