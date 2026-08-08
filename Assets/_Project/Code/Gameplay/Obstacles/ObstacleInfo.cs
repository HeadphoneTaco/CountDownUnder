using UnityEngine;

namespace Assets._Project.Code.Gameplay.Obstacles
{
    [CreateAssetMenu(fileName = "ObstacleInfo", menuName = "Scriptable Objects/ObstacleInfo")]
    public class ObstacleInfo : ScriptableObject
    {
        public float Damage;
        public float KnockbackForce;
        public float StunTime;
        [Tooltip("should almost certainly be \"Player\" unless you change it for some reason")]public string PlayerLayerName = "Player";
    }

    #region Moving Obstacle Info Set
    public abstract class  BiteInfo : ScriptableObject
    {
        [Tooltip("The offset from the obstacle's position where the bite starts, if the enemy is facing left or right is already accounted for so the x value is more like the distance from the center of the transform")][SerializeField] protected Vector2 BiteOffset = new Vector2(1,0);
        [Tooltip("the bite is an overlap circle, this is how big that circle is")][SerializeField] protected float BiteRadius = 1;
        [Tooltip("this is how many seconds the bite lasts for")]public float BiteDuration = 0.5f;
        [Tooltip("this is not how far the bite reaches, it is how close the player needs to be for it to start")]public float BiteDistance = 1;
        [Tooltip("this is how many seconds after a bite finishes before the next one can start")]public float BiteCooldown = 0.5f;
        public abstract bool BitePlayer(Transform obstacleTransform, LayerMask playerLayer, PlayerController playerController, bool facingRight, Rigidbody2D ObstacleRigidbody, float TimeSinceBiteStart);
        public bool IsPlayerInBiteRange(Vector2 obstaclePosition, Vector2 playerPosition)
        {
            return Vector2.Distance(obstaclePosition, playerPosition) <= BiteDistance;
        }
    }
    public abstract class SearchInfo : ScriptableObject
    {
        public abstract void SearchForPlayer(Transform obstacleTransform, LayerMask playerLayer, out PlayerController playerController, bool FacingRight);
        public float CastDistance;
    }
    public abstract class IdleInfo : ScriptableObject
    {
        [Tooltip("this is how fast the enemy will wander or return to its starting position")][SerializeField] protected float MoveSpeed;
        [Tooltip("this is how long the enemy will wait before making a new decision")][SerializeField] protected float DecisionTime;
        [Tooltip("this is an offset for the decision time, to add some randomness")][SerializeField] protected float DecisionTimeOffset;
        public virtual (bool, bool) Idle(Transform obstacleTransform, Rigidbody2D RB, Vector2 StartPosition, bool HasMadeItBack)
        {
            // return to original position
            
                RB.linearVelocityX = MoveSpeed * (StartPosition.x - obstacleTransform.position.x > 0 ? 1 : -1);
                return (StartPosition.x - obstacleTransform.position.x > 0, false);
        }
    }
    public abstract class ChaseInfo : ScriptableObject
    {
        [Tooltip("this is how fast the enemy will chase the player")][SerializeField] protected float ChaseSpeed;
        [Tooltip("this is how far the enemy will chase the player before forgetting them")]public float ForgetDistance;
        public abstract bool Chase(Transform obstacleTransform, Rigidbody2D RB, Vector2 playerPosition);
    }
    #endregion
}