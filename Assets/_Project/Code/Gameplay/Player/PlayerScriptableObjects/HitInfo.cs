using UnityEngine;

[CreateAssetMenu(fileName = "HitInfo", menuName = "Scriptable Objects/HitInfo")]
public class HitInfo : ScriptableObject
{
    public Vector2 BoxCastHalf;
    public float InvincibilityTime;
    public float KnockBackTime;
    public float BloodDrainRate;
    public float MaxBloodPoints;
}
