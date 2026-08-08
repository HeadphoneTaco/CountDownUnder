using Assets._Project.Code.Gameplay.Obstacles.Stratagies;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Assets._Project.Code.Gameplay.Obstacles
{
    public class MovingObstacle : BaseObstacle
    {
        // idle, chase, bite, search, forget
        // do a ray cast to look for the player
        // if close to the player do a bite
        // if not close to the player move towards the player
        [SerializeField] private BiteInfo _biteInfo;
        private BaseBiteStratagy _biteStratagy;
        [SerializeField] private SearchInfo _searchInfo;
        private BaseSearchStratagy _searchStratagy;
        [SerializeField] private IdleInfo _idleInfo;
        [SerializeField] private ChaseInfo _chaseInfo;

        public Dictionary<SearchStratagies, BaseSearchStratagy> _searchStratagies = new Dictionary<SearchStratagies, BaseSearchStratagy>()
    {
        { SearchStratagies.SingleRay, new SingleRaySearchStratagy() },
        { SearchStratagies.Box, new BoxSearchStratagy() }
    };


        private void Start()
        {
            _searchStratagy = _searchStratagies.GetValueOrDefault(_searchInfo.SearchStratagy);
            _searchStratagy.SearchForPlayer(transform, _searchInfo.RaycastDistance, _playerLayerIndex, out _playerController);
        }

    }
}