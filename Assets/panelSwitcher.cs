using UnityEngine;

public class PanelSwitcher : MonoBehaviour
{
    public GameObject panelToEnable;
    public GameObject panelToDisable;

    // Call this method to switch panels
    public void SwitchPanels()
    {
        if (panelToEnable != null)
            panelToEnable.SetActive(true);

        if (panelToDisable != null)
            panelToDisable.SetActive(false);
    }
}
