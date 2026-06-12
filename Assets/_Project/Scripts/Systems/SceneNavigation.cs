using UnityEngine;

public class SceneNavigation : MonoBehaviour
{
    public void EnterDungeon()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.EnterDungeon();
        }
    }

    public void ReturnToShop()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ReturnToShop();
        }
    }
}
