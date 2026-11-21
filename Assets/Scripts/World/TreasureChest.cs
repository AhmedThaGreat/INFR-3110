using UnityEngine;

// The treasure chest at the end of the level. An OBSERVER (environment reaction):
// it listens for TreasureGoalReached and GameWon and changes on its own.
//
//   Locked    -> collect enough treasure -> Unlocked (golden light + beacon)
//   Unlocked  -> player touches it       -> Opened   (swap to the open model)
//
// Put this on the chest. The Goal trigger (tag "Goal") is what the player touches.
public class TreasureChest : MonoBehaviour
{
    private enum ChestState { Locked, Unlocked, Opened }

    [Tooltip("Optional: the closed chest model (hidden when the chest opens).")]
    public GameObject closedModel;
    [Tooltip("Optional: the open chest model (shown when the chest opens).")]
    public GameObject openModel;

    public Color glowColor = new Color(1f, 0.8f, 0.3f);
    public float glowRange = 8f;
    public float glowIntensity = 4f;

    private ChestState state = ChestState.Locked;
    private Light glow;

    void Start()
    {
        if (closedModel != null) closedModel.SetActive(true);
        if (openModel != null) openModel.SetActive(false);

        // A golden light that switches on when the chest unlocks.
        GameObject lightObject = new GameObject("Chest Glow");
        lightObject.transform.SetParent(transform, false);
        lightObject.transform.localPosition = Vector3.up * 1.5f;
        glow = lightObject.AddComponent<Light>();
        glow.type = LightType.Point;
        glow.color = glowColor;
        glow.range = glowRange;
        glow.intensity = 0f;
    }

    void OnEnable()
    {
        GameEvents.TreasureGoalReached += Unlock;
        GameEvents.GameWon += Open;
    }

    void OnDisable()
    {
        GameEvents.TreasureGoalReached -= Unlock;
        GameEvents.GameWon -= Open;
    }

    void Update()
    {
        // While unlocked, the glow pulses so the player can find the chest.
        if (state == ChestState.Unlocked && glow != null)
        {
            glow.intensity = glowIntensity * (0.75f + 0.25f * Mathf.Sin(Time.time * 4f));
        }
    }

    void Unlock()
    {
        if (state != ChestState.Locked) return;
        state = ChestState.Unlocked;
    }

    void Open()
    {
        state = ChestState.Opened;
        if (glow != null) glow.intensity = glowIntensity * 1.5f;
        if (closedModel != null) closedModel.SetActive(false);
        if (openModel != null) openModel.SetActive(true);
    }
}
