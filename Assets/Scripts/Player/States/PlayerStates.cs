using UnityEngine;

// STATE PATTERN - every thing the player can be "doing" is its own class.
//
// Each state has three parts:
//   Enter() - runs once when we switch INTO this state (play its animation, etc.)
//   Tick()  - runs every frame while we're in it (decides when to switch out)
//   Exit()  - runs once when we leave it
//
// Name is also the name of the matching state in the Animator, so switching
// state automatically plays the right animation.
//
//   Idle <-> Run           (start / stop moving)
//   Idle/Run -> Jump       (leave the ground)
//   Jump -> Idle/Run       (land)
//   any -> Hurt -> Idle    (hit by spikes or a cannonball)
//   any -> Dead            (out of hearts)
//   any -> Victory         (reached the unlocked chest)
public abstract class PlayerState
{
    protected PlayerStateMachine machine;
    protected PlayerMovement movement;

    public abstract string Name { get; }

    protected PlayerState(PlayerStateMachine machine)
    {
        this.machine = machine;
        this.movement = machine.Movement;
    }

    public virtual void Enter() { }
    public virtual void Tick() { }
    public virtual void Exit() { }

    // Shared check used by the "normal" states.
    protected bool IsMoving => movement.HorizontalSpeed > 0.1f;

    // CharacterController.isGrounded flickers on bumpy sand, so only count it as
    // "in the air" if we've been off the ground for a moment or are going up.
    protected bool IsAirborne => movement.TimeSinceGrounded > 0.15f || movement.VerticalVelocity > 0.5f;
}

public class IdleState : PlayerState
{
    public IdleState(PlayerStateMachine machine) : base(machine) { }
    public override string Name => "Idle";

    public override void Enter() { movement.CanMove = true; }

    public override void Tick()
    {
        if (IsAirborne) machine.ChangeState(machine.Jump);
        else if (IsMoving) machine.ChangeState(machine.Run);
    }
}

public class RunState : PlayerState
{
    public RunState(PlayerStateMachine machine) : base(machine) { }
    public override string Name => "Run";

    public override void Enter() { movement.CanMove = true; }

    public override void Tick()
    {
        if (IsAirborne) machine.ChangeState(machine.Jump);
        else if (!IsMoving) machine.ChangeState(machine.Idle);
    }
}

public class JumpState : PlayerState
{
    public JumpState(PlayerStateMachine machine) : base(machine) { }
    public override string Name => "Jump";

    public override void Enter() { movement.CanMove = true; }   // you can steer in the air

    public override void Tick()
    {
        if (movement.IsGrounded && movement.VerticalVelocity <= 0f)
        {
            machine.ChangeState(IsMoving ? (PlayerState)machine.Run : machine.Idle);
        }
    }
}

public class HurtState : PlayerState
{
    private float timer;
    private Vector3 hitFrom;

    public HurtState(PlayerStateMachine machine) : base(machine) { }
    public override string Name => "Hurt";

    // The state machine tells us where the hit came from before switching in.
    public void SetHitSource(Vector3 source) { hitFrom = source; }

    public override void Enter()
    {
        movement.CanMove = false;                         // can't move while stunned
        movement.Knockback(hitFrom, machine.knockbackForce);
        timer = machine.hurtDuration;
    }

    public override void Tick()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f) machine.ChangeState(machine.Idle);
    }
}

public class DeadState : PlayerState
{
    public DeadState(PlayerStateMachine machine) : base(machine) { }
    public override string Name => "Death";

    public override void Enter() { movement.CanMove = false; }   // nothing leaves this state
}

public class VictoryState : PlayerState
{
    public VictoryState(PlayerStateMachine machine) : base(machine) { }
    public override string Name => "Victory";

    public override void Enter() { movement.CanMove = false; }   // nothing leaves this state
}
