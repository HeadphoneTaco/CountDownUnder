using UnityEngine;

public class PSEating : IState
{
    private PlayerController _player;
    private bool _targetDead;
    private float _bloodSucked;
    private bool _eating;
    
    public PSEating(PlayerController player)
    {
        _player = player;
    }
    // play the eating animation
    // exit into Idle
    // 
    public void Enter()
    {
        // The null check has to come first. Reading Food.transform.position above it
        // threw before the guard could ever run.
        if (_player.Food == null || _player.Food.GetBit())
        {
            Debug.Log("food is missing or dead");
            _player.MyStateMachine.ChangeState(_player.MyStateMachine.StateIdle);
            return;
        }

        _player.MyAnimator.ChangeState(PlayerAnimationState.EATING);
        _player.CanTransform = false;
        _player.transform.position = _player.Food.transform.position;
        _player.RB.linearVelocity = Vector2.zero;

        _eating = true;
        EventManager.PlayerEatStarted?.Invoke();
    }

    public void Execute()
    {
        if (_player.Food == null)
        {
            Debug.Log("food is missing");
            _player.MyStateMachine.ChangeState(_player.MyStateMachine.StateIdle);
            return;
        }
        _player.IncreaseBatTime(1);
        _player.RB.linearVelocity = Vector2.zero;
        DrainBlood();
    }

    public void Exit()
    {
        if (_player != null)
        {
            _player.CanTransform = true;
            _player.Food = null;
        }

        // Guarded, because Enter can bail out before eating ever begins. Firing the
        // end event without a matching start would leave the eating loop running.
        if (!_eating) return;
        _eating = false;
        EventManager.PlayerEatEnded?.Invoke();
    }
    public void FixedUpdate()
    {

    }
    private void DrainBlood()
    {
        (_targetDead, _bloodSucked) =_player.Food.DrainBlood(_player.HitInfo.BloodDrainRate * Time.deltaTime);

        // The victim was losing blood but none of it was reaching the player. The amount
        // came back in _bloodSucked and was simply never spent.
        // Guarded on > 0 because a drained victim returns zero, and ChangeBloodPoints
        // logs a warning for a change of nothing, which would spam every frame.
        if (_bloodSucked > 0f) _player.ChangeBloodPoints(_bloodSucked);

        if (_targetDead)
        {
            _player.MyStateMachine.ChangeState(_player.MyStateMachine.StateIdle);
        }
    }
}
