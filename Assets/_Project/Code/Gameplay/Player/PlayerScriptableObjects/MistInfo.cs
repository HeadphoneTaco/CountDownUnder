using UnityEngine;

[CreateAssetMenu(fileName = "MistInfo", menuName = "Scriptable Objects/MistInfo")]
public class MistInfo : ScriptableObject
{
    public float TimeAfterBreakToTransform;
    public float TimeBetweenMist;
    public float MistSpeed;
    public float MistTime;
}
