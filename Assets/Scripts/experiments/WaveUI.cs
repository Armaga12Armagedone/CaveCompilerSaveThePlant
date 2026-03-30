using UnityEngine;
using TMPro;

public class WaveUI : MonoBehaviour
{
    [Header("References")]
    public WaveManager waveManager;
    public TMP_Text waveNumberText;
    public TMP_Text timerText;
    public TMP_Text enemiesCountText;

    private void Start()
    {
        if (waveManager == null)
            waveManager = FindObjectOfType<WaveManager>();
    }

    private void Update()
    {
        if (waveManager == null) return;

        if (waveNumberText != null)
            waveNumberText.text = $"Wave: {waveManager.GetCurrentWave()}";

        float timeLeft = waveManager.GetTimeToNextWave();
        if (timerText != null)
            timerText.text = $"Next wave: {timeLeft:F1}s";

        int nextEnemies = waveManager.GetNextWaveEnemyCount();
        if (enemiesCountText != null)
            enemiesCountText.text = $"Enemies: {nextEnemies}";
    }
}