using UnityEngine;

[CreateAssetMenu(fileName = "BatInfo", menuName = "Scriptable Objects/BatInfo")]
public class BatInfo : ScriptableObject
{
    public float FlySpeed;
    public float MaxBatTime;
    public float BatTimeDrainRate;
    public float BatTimeFillRate;
}
