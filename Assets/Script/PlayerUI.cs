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
    }

    private void GameManager_OnShapeAssign(object sender, System.EventArgs e)
    {
        if(GameManager.Instance.GetShapeType() == "CIRCLE")
        {
            circleYouText.SetActive(true);
        }
        else if(GameManager.Instance.GetShapeType() == "CROSS")
        {
            crossYouText.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Shape Has not be assigned Yet");
        }
    }
}
