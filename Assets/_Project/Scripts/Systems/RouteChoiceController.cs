using TMPro;
using UnityEngine;

public class RouteChoiceController : MonoBehaviour
{
    public DungeonDoor combatRouteDoor;
    public DungeonDoor rewardRouteDoor;
    public RoomEncounter combatEncounter;
    public DungeonExitPortal rewardExitPortal;
    public TMP_Text statusText;

    private bool routeChosen;

    public void ChooseCombatRoute()
    {
        if (routeChosen)
        {
            return;
        }

        routeChosen = true;
        rewardRouteDoor.SetLocked(true);
        combatRouteDoor.SetLocked(false);
        combatEncounter.BeginEncounter();

        if (statusText != null)
        {
            statusText.text = "Combat route chosen";
        }
    }

    public void ChooseRewardRoute()
    {
        if (routeChosen)
        {
            return;
        }

        routeChosen = true;
        combatRouteDoor.SetLocked(true);
        rewardRouteDoor.SetLocked(false);
        rewardExitPortal.SetUnlocked(true);

        if (statusText != null)
        {
            statusText.text = "Safe reward route chosen";
        }
    }
}
