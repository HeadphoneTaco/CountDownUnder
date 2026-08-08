using UnityEngine;

namespace Assets._Project.Code.Gameplay.Obstacles.Stratagies
{
    [CreateAssetMenu(fileName = "BoxSearchStrategy", menuName = "Scriptable Objects/BoxSearchStrategy")]
    public class BoxSearchStrategy : SearchInfo
    {
        [SerializeField] private Vector2 BoxSize;
        public override void SearchForPlayer(Transform obstacleTransform, LayerMask playerLayer, out PlayerController playerController, bool FacingRight)
        {
            playerController = null;
            RaycastHit2D[] hits = Physics2D.BoxCastAll(obstacleTransform.position, BoxSize, 0f, FacingRight ? obstacleTransform.right : -obstacleTransform.right, CastDistance, playerLayer);
            if (hits.Length > 0)
            {
                playerController = hits[0].collider.GetComponent<PlayerController>();
            }
            else playerController = null;
        }
    }
}