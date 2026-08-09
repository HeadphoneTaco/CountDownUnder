using UnityEngine;

namespace Assets._Project.Code.Gameplay.Obstacles.Strategies
{
    [CreateAssetMenu(fileName = "LungeBiteStrategy", menuName = "Scriptable Objects/LungeBiteStrategy")]
    public class LungeBiteStrategy : BiteInfo
    {
        [Tooltip("Keep the x value low because it is added every FixedUpdate, however the y value must be larger than the force of gravity")][SerializeField] private Vector2 LungeSpeed;
        public override bool BitePlayer(Transform obstacleTransform, LayerMask playerLayer, PlayerController playerController, bool facingRight, Rigidbody2D ObstacleRigidbody, float TimeSinceBiteStart)
        {
            ObstacleRigidbody.AddForce(Time.fixedDeltaTime * (BiteDuration - TimeSinceBiteStart)/BiteDuration * new Vector2(facingRight ? LungeSpeed.x : -LungeSpeed.x, LungeSpeed.y), ForceMode2D.Impulse);
            if(Physics2D.OverlapCircle((Vector2)obstacleTransform.position + new Vector2(facingRight ? BiteOffset.x : -BiteOffset.x, BiteOffset.y), BiteRadius, playerLayer) == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}