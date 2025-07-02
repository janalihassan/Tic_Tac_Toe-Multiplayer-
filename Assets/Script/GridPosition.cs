using UnityEngine;

public class GridPosition : MonoBehaviour
{

    [SerializeField] private int X;
    [SerializeField] private int Y;

    private void OnMouseDown()
    {
        Debug.Log("Click!" + X + Y);
        GameManager.Instance.OnClickedGrid(X, Y);
    }
}
