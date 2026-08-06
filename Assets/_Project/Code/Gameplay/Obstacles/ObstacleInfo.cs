using Assets._Project.Code.Gameplay.Obstacles.Stratagies;
using UnityEngine;

namespace Assets._Project.Code.Gameplay.Obstacles
{
    [CreateAssetMenu(fileName = "ObstacleInfo", menuName = "Scriptable Objects/ObstacleInfo")]
    public class ObstacleInfo : ScriptableObject
    {
        public float Damage;
        public float KnockbackForce;
        public float StunTime;
        public string PlayerLayerName;
    }

    #region Moving Obstacle Info Set
    [CreateAssetMenu(fileName = "BiteInfo", menuName = "Scriptable Objects/BiteInfo")]
    public class  BiteInfo : ScriptableObject
    {
        public Vector2 BiteOffset;
        public Vector2 BiteHalfExtent;
        public float BiteDuration;
    }
    [CreateAssetMenu(fileName = "SearchInfo", menuName = "Scriptable Objects/SearchInfo")]
    public class SearchInfo : ScriptableObject
    {
        public SearchStratagies SearchStratagy;
        public float RaycastDistance;
    }
    [CreateAssetMenu(fileName = "IdleInfo", menuName = "Scriptable Objects/IdleInfo")]
    public class IdleInfo : ScriptableObject
    {

    }
    [CreateAssetMenu(fileName = "ChaseInfo", menuName = "Scriptable Objects/ChaseInfo")]
    public class ChaseInfo : ScriptableObject
    {

    }
    #endregion
}