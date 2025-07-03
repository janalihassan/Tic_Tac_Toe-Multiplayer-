using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private GameObject crossYouText;
    [SerializeField] private GameObject circleYouText;
    [SerializeField] private GameObject crossArrow;
    [SerializeField] private GameObject circleArrow;


    private void Awake()
    {
        crossYouText.SetActive(false);
        circleYouText.SetActive(false);
        crossArrow.SetActive(false);
        circleArrow.SetActive(false);
    }

    private void Start()
    {
        GameManager.Instance.OnShapeAssign += GameManager_OnShapeAssign;
        GameManager.Instance.OnTurnchanged += GameManager_OnTurnchanged;
    }

    private void GameManager_OnTurnchanged(string TurnId)
    {
        var myId = GameManager.Instance.GetMyPlayerId();
        var myShape = GameManager.Instance.GetShapeType();

        if (TurnId == myId)
        {
            if (myShape == "CROSS")
            {
                crossArrow.SetActive(true);
                circleArrow.SetActive(false);
            }
            else if (myShape == "CIRCLE")
            {
                circleArrow.SetActive(true);
                crossArrow.SetActive(false);
            }
        }
        else
        {
            crossArrow.SetActive(false);
            circleArrow.SetActive(false);
        }
    }

    private void GameManager_OnShapeAssign(object sender, System.EventArgs e)
    {
        if (GameManager.Instance.GetShapeType() == "CIRCLE")
        {
            circleYouText.SetActive(true);
        }
        else if (GameManager.Instance.GetShapeType() == "CROSS")
        {
            crossYouText.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Shape Has not be assigned Yet");
        }
    }
}
