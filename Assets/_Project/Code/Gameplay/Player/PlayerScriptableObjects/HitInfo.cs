using UnityEngine;

[CreateAssetMenu(fileName = "HitInfo", menuName = "Scriptable Objects/HitInfo")]
public class HitInfo : ScriptableObject
{
    public Vector2 BoxCastHalf;
    public float InvincibilityTime;
    public float KnockBackTime;
    [Tooltip("How fast blood is pulled OUT of a victim while eating.")]
    public float BloodDrainRate;

    [Tooltip("Blood points the player loses every second just by being undead. This is what " +
             "forces you to keep hunting. At 0.6 a full bar of 110 empties in about three minutes.")]
    public float PassiveBloodDrainRate = 0.6f;

    public float MaxBloodPoints;
}
