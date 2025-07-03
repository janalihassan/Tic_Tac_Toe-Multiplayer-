using TMPro;
using UnityEngine;
using Playroom;
using Unity.VisualScripting;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI gameOverText;
    [SerializeField] private Color winColor;
    [SerializeField] private Color loseColor;
    private void Start()
    {
        GameManager.Instance.OnGameOver += GameManager_OnGameOver;
        gameObject.SetActive(false);
    }

    private void GameManager_OnGameOver(string winnerId)
    {
        Debug.LogWarning("Game Over Event Hit");
        gameObject.SetActive(true);
        string player = GameManager.Instance.GetMyPlayerId();

        if (winnerId == player)
        {
            gameOverText.text = "You Win!";
            gameOverText.color = winColor;
        }
        else
        {
            gameOverText.text = "You Lose!";
            gameOverText.color = loseColor;
        }
    }
}
