using UnityEngine;

namespace Assets._Project.Code.Gameplay.Obstacles.Strategies
{
    [CreateAssetMenu(fileName = "ChaseStrategy", menuName = "Scriptable Objects/ChaseStrategy")]
    public class ChaseStrategy : ChaseInfo
    {
        public override bool Chase(Transform obstacleTransform, Rigidbody2D RB, Vector2 playerPosition)
        {
            if(playerPosition.x - obstacleTransform.position.x > 0)
            {

                RB.linearVelocityX = ChaseSpeed;
                return true;
            }
            else
            {
                RB.linearVelocityX = -ChaseSpeed;
                return false;
            }
        }
    }
}