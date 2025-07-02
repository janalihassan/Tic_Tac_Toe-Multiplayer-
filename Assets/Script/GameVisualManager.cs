using UnityEngine;

public class GameVisualManager : MonoBehaviour
{
    private const float GRID_SIZE = 3.1f;

    [SerializeField] private GameObject cross;
    [SerializeField] private GameObject cricle;


    private void Start()
    {
        GameManager.Instance.OnGridClicked += GameManager_OnGridClicked;
    }

    private void GameManager_OnGridClicked(object sender, GameManager.OnGridClickedEventArgs e)
    {
        Instantiate(cross,GetGridWorldPosition(e.x,e.y),Quaternion.identity);
    }

    private Vector2 GetGridWorldPosition(int x, int y)
    {
        return new Vector2(-GRID_SIZE + x * GRID_SIZE, -GRID_SIZE + y * GRID_SIZE);
    }
}
