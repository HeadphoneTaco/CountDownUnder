using UnityEngine;
namespace Assets._Project.Code.Gameplay.Obstacles
{
    public class StaticObstacle : BaseObstacle
    {
        private void OnTriggerEnter2D(Collider2D collision)
        {
            _playerController = collision.GetComponent<PlayerController>();
            if (_playerController != null)
            {
                HurtPlayer();
            }
        }
    }
}