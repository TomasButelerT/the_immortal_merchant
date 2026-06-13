using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class RouteChoiceTrigger : MonoBehaviour
{
    public RouteChoiceController routeChoice;
    public bool choosesCombatRoute;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || routeChoice == null)
        {
            return;
        }

        if (choosesCombatRoute)
        {
            routeChoice.ChooseCombatRoute();
        }
        else
        {
            routeChoice.ChooseRewardRoute();
        }
    }
}
