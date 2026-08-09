using UnityEngine;

namespace Assets._Project.Code.Gameplay.Obstacles.Strategies
{
    [CreateAssetMenu(fileName = "SingleRaySearchStrategy", menuName = "Scriptable Objects/SingleRaySearchStrategy")]
    public class SingleRaySearchStrategy : SearchInfo
    {
        [SerializeField] private Vector2 _rayOffset;
        public override void SearchForPlayer(Transform obstacleTransform, LayerMask playerLayer, out PlayerController playerController, bool FacingRight)
        {
            playerController = null;
            RaycastHit2D hit = Physics2D.Raycast((Vector2)obstacleTransform.position + new Vector2(FacingRight ? _rayOffset.x : -_rayOffset.x, _rayOffset.y), FacingRight ? obstacleTransform.right : -obstacleTransform.right, CastDistance, playerLayer);
            if (hit.collider != null)
            {
                playerController = hit.collider.GetComponent<PlayerController>();
            }
        }
    }
}