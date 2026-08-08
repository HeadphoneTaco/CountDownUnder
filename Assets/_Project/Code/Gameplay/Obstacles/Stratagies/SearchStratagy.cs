using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets._Project.Code.Gameplay.Obstacles.Stratagies
{
    public enum SearchStratagies
    {
        SingleRay,
        Box
    }
    
    public interface BaseSearchStratagy
    {
        public abstract void SearchForPlayer(Transform obstacleTransform, float searchDistance, LayerMask playerLayer, out PlayerController playerController);
    }
    public class SingleRaySearchStratagy : BaseSearchStratagy
    {
        public void SearchForPlayer(Transform obstacleTransform, float searchDistance, LayerMask playerLayer, out PlayerController playerController)
        {
            Debug.Log("Raycasting for player");
            playerController = null;
            RaycastHit2D hit = Physics2D.Raycast(obstacleTransform.position, obstacleTransform.right, searchDistance, playerLayer);
            if (hit.collider != null)
            {
                playerController = hit.collider.GetComponent<PlayerController>();
            }
        }
    }
    public class BoxSearchStratagy : BaseSearchStratagy
    {
        public void SearchForPlayer(Transform obstacleTransform, float searchDistance, LayerMask playerLayer, out PlayerController playerController)
        {
            Debug.Log("Boxcasting for player");
            playerController = null;
            RaycastHit2D hit = Physics2D.BoxCast(obstacleTransform.position, new Vector2(searchDistance, 1f), 0f, obstacleTransform.right, searchDistance, playerLayer);
            if (hit.collider != null)
            {
                playerController = hit.collider.GetComponent<PlayerController>();
            }
        }
    }
}