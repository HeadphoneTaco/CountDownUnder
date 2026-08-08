using System.Collections;
using UnityEngine;

namespace Assets._Project.Code.Gameplay.Obstacles.Stratagies
{
    [CreateAssetMenu(fileName = "IdleStrategy", menuName = "Scriptable Objects/IdleStrategy")]
    public class IdleStrategy : IdleInfo
    {
        public override (bool, bool) Idle(Transform obstacleTransform, Rigidbody2D RB, Vector2 StartPosition, bool HasMadeItBack)
        {
            if (!HasMadeItBack)
            {
                if (Mathf.Abs(StartPosition.x - obstacleTransform.position.x) > 0.1f)
                {
                    return base.Idle(obstacleTransform, RB, StartPosition, HasMadeItBack);
                }
            }
            // if there then look back and forth every now and then
                RB.linearVelocityX = 0;
                if (Time.time % (2 * DecisionTime) + DecisionTimeOffset < DecisionTime)
                {
                    return (true, true);
                }
                else return (false, true);
        }
    }
}