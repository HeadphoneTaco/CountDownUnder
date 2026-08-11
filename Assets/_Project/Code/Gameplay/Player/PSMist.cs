using UnityEngine;

public class PSMist : IState
{
    private PlayerController _player;
    private float _mystStep;
    private Vector2 _dashDirection;
    private Collider2D _victimLayerFind;
    private Victim _possibleVictim;
    // _wasBat is gone. Its only reader was the old particle branch in Enter, which picked
    // between a bat and a person puff. One SmokeBurst covers both now.

    public PSMist(PlayerController player)
    {
        _player = player;
    }
    // the inbetween dashing and transforming state
    // give the player a bunch of speed in the direction they were going before (or not if from idle state)
    // do a boxcast to check for victims and if there are go into blood sucking state
    // if the player is still holding space by the end, put the player into bat state
    // if the player stops holding space put them back into falling state
    // if the player alternatively enters from the bat state the player should be put into falling (this may just happen anyway because they are not still holding space)
    // there may need to be a timer to prevent the player from spamming this ability
    public void Enter()
    {
        // Opening poof, then the trail runs for the length of the dash. The closing poof
        // is fired from Exit, giving poof-trail-poof.
        if (_player.SmokeBurst != null) _player.SmokeBurst.Play();
        if (_player.SmokeTrail != null) _player.SmokeTrail.Play();

        _player.MyAnimator.ChangeState(PlayerAnimationState.MIST);
        _player.CanTransform = false;
        EventManager.DIEvent += ChangeDI;
        _mystStep = 0;
        _dashDirection = _player.DirectionalInput;
        Debug.Log("State Entered: Mist");
    }

    public void Execute()
    {
        _mystStep += Time.deltaTime;
        _player.RB.linearVelocity = _dashDirection * _player.MistInfo.MistSpeed;
        if (_mystStep > _player.MistInfo.MistTime)
        {
            // No particle calls here. Exit fires the closing smoke puff on the way out of
            // this state, whichever form the player lands in, so doing it here as well
            // would double the poof.
            if (_player.BatInputHeld && Time.time - _player.LastBatBreakTime > _player.MistInfo.TimeAfterBreakToTransform)
            {
                // turn player into bat
                _player.MyStateMachine.ChangeState(_player.MyStateMachine.StateBat);
            }
            else
            {
                // turn player into vamp form
                _player.MyStateMachine.ChangeState(_player.MyStateMachine.StateFalling);
            }
        }
    }

    public void Exit()
    {
        // Was "+=" rather than "-=". Every mist added a second subscription and removed
        // none, so ChangeDI was called once more per dash for the rest of the run.
        EventManager.DIEvent -= ChangeDI;
        if (_player == null) return;

        _player.CanTransform = true;

        // Closing poof at whatever form the player arrives in, so a bat/human swap always
        // reads as a puff of smoke rather than a silent pop.
        if (_player.SmokeTrail != null)
        {
            _player.SmokeTrail.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        if (_player.SmokeBurst != null) _player.SmokeBurst.Play();
    }
    public void ChangeDI(Vector2 direction)
    {
        _player.ChangeDI(direction);
    }
    public void FixedUpdate()
    {
        if (VictimCheck())
        {
            // Blood is PSEating's business, and it fires the splatter on the bite itself.
            _player.MyStateMachine.ChangeState(_player.MyStateMachine.StateEating);
        }
    }
    public bool VictimCheck()
    {
        _victimLayerFind = Physics2D.OverlapBox(_player.transform.position, _player.HitInfo.BoxCastHalf, 0, _player.VictimLayerIndex);
        if ( _victimLayerFind != null)
        {
            _possibleVictim = _victimLayerFind.GetComponent<Victim>();
            if (_possibleVictim != null)
            {
                Debug.Log("Found");
                _player.Food = _possibleVictim;
                return true;
            }
        }
        return false;
    }
}
