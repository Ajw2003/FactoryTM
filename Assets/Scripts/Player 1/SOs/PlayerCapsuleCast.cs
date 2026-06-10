using UnityEngine;

[CreateAssetMenu(fileName = "PlayerCapsuleCast", menuName = "Player Scriptable Objects/Create Custom Values for the Player's Capsule Cast")]
public class PlayerCapsuleCast : ScriptableObject
{
    public float OffsetOfSpheres;
    public float RadiusOfSpheres;
    public float MaxDistance; //Max distance in how far you want the capsule cast to shoot out
}

//A capsule cast is a raycast, but instead of a laser/ray, it's shooting a capsule shaped ray
//A capsule cast, the way it's created, is that you make two spheres to position the top/bottom of the capsule
//"OffsetOfSpheres" is the offset from the object origin the capsule cast will be positioned.

//For the "PlayerController script", I have it like this:
//Vector3 firstSpherePosition = transform.position + (transform.up * OffsetOfSpheres);
//Vector3 secondSpherePosition = transform.position + (transform.up * -OffsetOfSpheres); 

//This means the capsule cast will be from the player's position, moving up from the transform by the offset
