using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using TMPro;

public class DayController : MonoBehaviour
{
    public int currentDay = 1;
    public int maxDays = 3;

    public Transform playerSpawnPoint; // Only one spawn point needed
    public GameObject player;
    public GameObject[] wives; // Assign WifeDay1, WifeDay2, WifeDay3
    public TextMeshProUGUI dayText;

    public CanvasGroup caughtPanel; // Assign a CanvasGroup component for "You Got Caught" panel
    public PlayableDirector startTimeline;

    public MonoBehaviour playerController; // Reference to player controller script
    public GameObject escapeItem; // Will be null if player didn't escape

    private bool isGameOver = false;

    void Start()
    {
        UpdateDayDisplay();
        ActivateCurrentWife();
    }

    public void PlayerCaught()
    {
        if (isGameOver) return;
        StartCoroutine(HandlePlayerCaught());
    }

    IEnumerator HandlePlayerCaught()
    {
        playerController.enabled = false;
        yield return new WaitForSeconds(2f);

        // Fade in "You Got Caught" panel
        caughtPanel.gameObject.SetActive(true);
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime;
            caughtPanel.alpha = Mathf.Lerp(0, 1, t);
            yield return null;
        }

        yield return new WaitForSeconds(1.5f);
        currentDay++;

        if (currentDay > maxDays)
        {
            GameOver();
        }
        else
        {
            // Begin timeline, then delay before showing player again
            yield return new WaitForSeconds(2f);
            ReplayStartTimeline();

            RespawnPlayer();

            caughtPanel.gameObject.SetActive(false);
            caughtPanel.alpha = 0;
        }
    }

    void RespawnPlayer()
    {
        player.SetActive(true);
        player.transform.position = playerSpawnPoint.position;
        player.transform.rotation = playerSpawnPoint.rotation;

        playerController.enabled = true;

        UpdateDayDisplay();
        ActivateCurrentWife();

        Debug.Log("✅ Player respawned at Day " + currentDay);
    }

    void UpdateDayDisplay()
    {
        dayText.text = "DAY " + currentDay;
    }

    void ActivateCurrentWife()
    {
        for (int i = 0; i < wives.Length; i++)
        {
            wives[i].SetActive(i == currentDay - 1);
        }
    }

    void ReplayStartTimeline()
    {
        if (startTimeline != null)
        {
            startTimeline.time = 0;
            startTimeline.Play();
            Debug.Log("▶️ Start timeline replayed.");
        }
    }

    public void EscapeTriggered()
    {
        isGameOver = true;
        Debug.Log("🎉 Player Escaped!");
        // Add win screen or end-game logic here
    }

    void GameOver()
    {
        Debug.Log("💀 Game Over. Player failed to escape.");
        // Implement game over logic (load scene, show screen, etc.)
    }
}
