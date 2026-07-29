using UnityEngine;

public class PSEating : IState
{
    private PlayerController _player;
    
    public PSEating(PlayerController player)
    {
        _player = player;
    }
    // play the eating animation
    // exit into Idle
    // 
    public void Enter()
    {
        _player.CanTransform = false;
    }

    public void Execute()
    {
        _player.IncreaseBatTime();
    }

    public void Exit()
    {
        if (_player != null)
        {
            _player.CanTransform = true;
        }
    }
}
