using UnityEngine;

namespace Assets._Project.Code.Gameplay.Obstacles
{
    public abstract class BaseObstacle : MonoBehaviour
    {
        [SerializeField]protected ObstacleInfo _obstacleInfo;
        protected PlayerController _playerController;
        protected int _playerLayerIndex;
        private Vector2 _knockBackDirection;

        protected virtual void Awake()
        {
            _playerLayerIndex = LayerMask.GetMask(_obstacleInfo.PlayerLayerName);
        }
        protected void HurtPlayer()
        {
            if (_playerController != null)
            {
                _knockBackDirection = _playerController.transform.position - transform.position;
                _knockBackDirection.Normalize();
                _playerController.TakeDamage(_obstacleInfo.Damage, _obstacleInfo.KnockbackForce * _knockBackDirection, _obstacleInfo.StunTime);
            }
        }
    }
}
