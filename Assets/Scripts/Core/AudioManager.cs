using UnityEngine;

// Plays every sound in the game. SINGLETON (one per scene) and OBSERVER:
// nothing calls "play the coin sound" directly - the AudioManager listens to
// GameEvents and picks the sound itself. Adding a new sound never means
// editing the coin, cannon or GameManager scripts.
//
// Drag sound files into the slots in the Inspector. Any empty slot is skipped.
public class AudioManager : Singleton<AudioManager>
{
    [Header("Music")]
    public AudioClip music;
    [Range(0f, 1f)] public float musicVolume = 0.35f;

    [Header("Sound effects")]
    public AudioClip coinSound;
    public AudioClip gemSound;
    public AudioClip powerUpSound;
    public AudioClip hurtSound;
    public AudioClip deathSound;
    public AudioClip winSound;
    public AudioClip chestUnlockSound;
    public AudioClip achievementSound;
    public AudioClip cannonSound;
    [Range(0f, 1f)] public float effectsVolume = 0.8f;

    private AudioSource effectsSource;
    private AudioSource musicSource;

    protected override void Awake()
    {
        base.Awake();
        effectsSource = gameObject.AddComponent<AudioSource>();
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
    }

    void Start()
    {
        if (music != null)
        {
            musicSource.clip = music;
            musicSource.volume = musicVolume;
            musicSource.Play();
        }
    }

    void OnEnable()
    {
        GameEvents.PickupCollected += OnPickupCollected;
        GameEvents.PlayerDamaged += OnPlayerDamaged;
        GameEvents.PlayerDied += OnPlayerDied;
        GameEvents.GameWon += OnGameWon;
        GameEvents.TreasureGoalReached += OnTreasureGoalReached;
        GameEvents.AchievementUnlocked += OnAchievementUnlocked;
        GameEvents.CannonFired += OnCannonFired;
    }

    void OnDisable()
    {
        GameEvents.PickupCollected -= OnPickupCollected;
        GameEvents.PlayerDamaged -= OnPlayerDamaged;
        GameEvents.PlayerDied -= OnPlayerDied;
        GameEvents.GameWon -= OnGameWon;
        GameEvents.TreasureGoalReached -= OnTreasureGoalReached;
        GameEvents.AchievementUnlocked -= OnAchievementUnlocked;
        GameEvents.CannonFired -= OnCannonFired;
    }

    void Play(AudioClip clip, float volumeScale = 1f)
    {
        if (clip != null && effectsSource != null)
        {
            effectsSource.PlayOneShot(clip, effectsVolume * volumeScale);
        }
    }

    void OnPickupCollected(PickupType type, Vector3 position)
    {
        if (type == PickupType.Coin) Play(coinSound);
        else if (type == PickupType.Gem) Play(gemSound);
        else Play(powerUpSound);
    }

    void OnPlayerDamaged(Vector3 hitFrom) { Play(hurtSound); }
    void OnPlayerDied() { musicSource.Stop(); Play(deathSound); }
    void OnGameWon() { musicSource.Stop(); Play(winSound); }
    void OnTreasureGoalReached() { Play(chestUnlockSound); }
    void OnAchievementUnlocked(string title) { Play(achievementSound, 0.8f); }
    void OnCannonFired(Vector3 position) { Play(cannonSound, 0.6f); }
}
