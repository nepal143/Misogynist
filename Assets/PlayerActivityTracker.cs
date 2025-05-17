using UnityEngine;
using System;

public class PlayerActivityTracker : MonoBehaviour
{
    public static PlayerActivityTracker Instance;

    [Header("Activity Settings")]
    [Tooltip("Current activity value.")]
    public float currentActivity = 0f;

    [Tooltip("Maximum activity value.")]
    public float maxActivity = 100f;

    [Tooltip("Activity decay rate per second.")]
    public float decayRate = 5f;

    [Tooltip("Threshold at which the AI is alerted.")]
    public float alertThreshold = 60f;

    [Tooltip("Cooldown time after alerting to avoid repeated alerts.")]
    public float alertCooldown = 5f;

    public event Action<Vector3> OnActivityAlert;

    private float alertTimer = 0f;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        HandleActivityDecay();
        HandleAlertCooldown();
    }

    private void HandleActivityDecay()
    {
        if (currentActivity > 0f)
        {
            currentActivity -= decayRate * Time.deltaTime;
            currentActivity = Mathf.Max(currentActivity, 0f);
        }
    }

    private void HandleAlertCooldown()
    {
        if (alertTimer > 0f)
        {
            alertTimer -= Time.deltaTime;
        }
    }

    public void IncreaseActivity(float amount, Vector3 sourcePosition)
    {
        currentActivity += amount;
        currentActivity = Mathf.Min(currentActivity, maxActivity);

        if (currentActivity >= alertThreshold && alertTimer <= 0f)
        {
            Debug.Log("[ActivityTracker] Activity threshold reached! Notifying AI.");
            OnActivityAlert?.Invoke(sourcePosition);
            alertTimer = alertCooldown;
        }
    }

    public void IncreaseActivity(float amount)
    {
        IncreaseActivity(amount, transform.position);
    }

    public float GetActivityLevel()
    {
        return currentActivity;
    }
}
