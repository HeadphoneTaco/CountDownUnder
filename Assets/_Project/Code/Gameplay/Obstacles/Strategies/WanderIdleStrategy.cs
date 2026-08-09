using System.Collections;
using UnityEngine;

namespace Assets._Project.Code.Gameplay.Obstacles.Strategies
{
    [CreateAssetMenu(fileName = "WanderIdleStrategy", menuName = "Scriptable Objects/WanderIdleStrategy")]
    public class WanderStrategy : IdleInfo
    {
        /// <summary>
        /// wanders back and forth based on time instead of loading anything into memory
        /// this way the obstacle does not need to have special parameters for this strategy
        /// </summary>
        public override (bool, bool) Idle(Transform obstacleTransform, Rigidbody2D RB, Vector2 StartPosition, bool HasMadeItBack)
        {
            if (!HasMadeItBack)
            {
                if (Mathf.Abs(StartPosition.x - obstacleTransform.position.x) > 0.1f)
                {
                    return base.Idle(obstacleTransform, RB, StartPosition, HasMadeItBack);
                }
                if (Time.time % (4 * DecisionTime) + DecisionTimeOffset > DecisionTime - Time.fixedDeltaTime)
                {
                    RB.linearVelocityX = 0;
                    return (true, false);
                }
            }
            // then wander around that area
            if (Time.time % (4 * DecisionTime) + DecisionTimeOffset < DecisionTime)
            {
                RB.linearVelocityX = 0;
                return (false, true);
            }
            else if (Time.time % (4 * DecisionTime) + DecisionTimeOffset < 2 * DecisionTime)
            {
                RB.linearVelocityX = MoveSpeed;
                return (true, true);
            }
            else if (Time.time % (4 * DecisionTime) + DecisionTimeOffset < 3 * DecisionTime)
            {
                RB.linearVelocityX = 0;
                return (true, true);
            }
            else
            {
                RB.linearVelocityX = - MoveSpeed;
                return (false, true);
            }
        }
    }
}