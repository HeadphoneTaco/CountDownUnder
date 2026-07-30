using UnityEngine;

[CreateAssetMenu(fileName = "CountInfo", menuName = "Scriptable Objects/CountInfo")]
public class CountInfo : ScriptableObject
{
    public float WalkSpeed;
    public float DefaultGravity;
    public float FallMoveSpeed;
}
