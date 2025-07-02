using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public event EventHandler<OnGridClickedEventArgs> OnGridClicked;
    public class OnGridClickedEventArgs : EventArgs
    {
        public int x;
        public int y;
    }

    private void Awake()
    {
        Instance = this;
    }

    public void OnClickedGrid(int x, int y)
    {
        Debug.Log($"grid Clicked and x : {x} and y : {y}");
        OnGridClicked?.Invoke(this,new OnGridClickedEventArgs
        {
            x = x,
            y = y,
        });
    }
}
