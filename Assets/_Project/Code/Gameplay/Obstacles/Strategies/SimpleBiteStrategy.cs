using UnityEngine;

namespace Assets._Project.Code.Gameplay.Obstacles.Stratagies
{
    
    [CreateAssetMenu(fileName = "SimpleBiteStrategy", menuName = "Scriptable Objects/SimpleBiteStrategy")]
    public class SimpleBiteStrategy : BiteInfo
    {
        public override bool BitePlayer(Transform obstacleTransform, LayerMask playerLayer, PlayerController playerController, bool facingRight, Rigidbody2D ObstacleRigidbody, float TimeSinceBiteStart)
        {
            return playerController == Physics2D.OverlapCircle((Vector2)obstacleTransform.position + new Vector2(facingRight ? BiteOffset.x : -BiteOffset.x, BiteOffset.y), BiteRadius, playerLayer).GetComponent<PlayerController>();
        }
    }
    
}