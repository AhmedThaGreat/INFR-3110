using UnityEngine;

// STATE PATTERN - the "context" that holds the current state and switches
// between them. Put this on the Player next to PlayerMovement.
//
// It is also an OBSERVER: it listens for PlayerDamaged, PlayerDied and GameWon,
// so the GameManager never has to know the player has states at all.
[RequireComponent(typeof(PlayerMovement))]
public class PlayerStateMachine : MonoBehaviour
{
    [Tooltip("The character model's Animator. Leave empty to find it on a child.")]
    public Animator animator;
    public float crossFadeTime = 0.15f;   // blend time between animations

    [Header("Hurt")]
    public float hurtDuration = 0.6f;
    public float knockbackForce = 7f;

    [Header("Live (just look)")]
    [SerializeField] private string currentStateName;

    public PlayerMovement Movement { get; private set; }
    public PlayerState Current { get; private set; }

    // One object per state, made once and reused.
    public IdleState Idle { get; private set; }
    public RunState Run { get; private set; }
    public JumpState Jump { get; private set; }
    public HurtState Hurt { get; private set; }
    public DeadState Dead { get; private set; }
    public VictoryState Victory { get; private set; }

    void Awake()
    {
        Movement = GetComponent<PlayerMovement>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (animator != null) animator.applyRootMotion = false;   // the code moves the player, not the animation

        Idle = new IdleState(this);
        Run = new RunState(this);
        Jump = new JumpState(this);
        Hurt = new HurtState(this);
        Dead = new DeadState(this);
        Victory = new VictoryState(this);
    }

    void OnEnable()
    {
        GameEvents.PlayerDamaged += OnPlayerDamaged;
        GameEvents.PlayerDied += OnPlayerDied;
        GameEvents.GameWon += OnGameWon;
    }

    void OnDisable()
    {
        GameEvents.PlayerDamaged -= OnPlayerDamaged;
        GameEvents.PlayerDied -= OnPlayerDied;
        GameEvents.GameWon -= OnGameWon;
    }

    void Start()
    {
        ChangeState(Idle);
    }

    void Update()
    {
        if (Current != null) Current.Tick();
    }

    public void ChangeState(PlayerState next)
    {
        if (next == null || next == Current) return;

        if (Current != null) Current.Exit();
        Current = next;
        currentStateName = next.Name;
        next.Enter();

        PlayAnimation(next.Name);
        GameEvents.RaisePlayerStateChanged(next.Name);
    }

    // Plays the Animator state with the same name, if the model has one.
    void PlayAnimation(string stateName)
    {
        if (animator == null || animator.runtimeAnimatorController == null) return;

        int hash = Animator.StringToHash(stateName);
        if (animator.HasState(0, hash))
        {
            animator.CrossFadeInFixedTime(hash, crossFadeTime);
        }
    }

    // ---- Observer reactions ----

    void OnPlayerDamaged(Vector3 hitFrom)
    {
        if (Current == Dead || Current == Victory) return;
        Hurt.SetHitSource(hitFrom);
        if (Current == Hurt) Hurt.Enter();   // hit again while hurt: restart the stun
        else ChangeState(Hurt);
    }

    void OnPlayerDied()
    {
        ChangeState(Dead);
    }

    void OnGameWon()
    {
        ChangeState(Victory);
    }
}
