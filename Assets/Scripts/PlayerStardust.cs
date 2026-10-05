// PlayerStardust
// Tracks the stardust collected during the current run. When the run ends (player dies,
// portal is used, or the object is disabled) the run total is banked into the saved total.
// The Stardust shop upgrade adds bonus stardust to every pickup.
using UnityEngine;

public class PlayerStardust : MonoBehaviour
{
    public int stardust = 0;
    public int stardustPerUpgrade = 1;

    private bool banked;

    // Adds a pickup amount plus the bonus from the Stardust upgrade level.
    public void Add(int amount)
    {
        stardust += amount + SaveSystem.Data.stardustLevel * stardustPerUpgrade;
    }

    // Moves the run total into the saved grand total. Runs once per run.
    public void BankRun()
    {
        if (banked) return;
        banked = true;
        SaveSystem.Data.totalStardust += stardust;
        SaveSystem.Save();
    }

    void OnDisable()
    {
        BankRun();
    }

    void OnApplicationQuit()
    {
        BankRun();
    }
}
