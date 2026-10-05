using UnityEngine;

public class PlayerStardust : MonoBehaviour
{
    public int stardust = 0;
    public int stardustPerUpgrade = 1;

    private bool banked;

    public void Add(int amount)
    {
        stardust += amount + SaveSystem.Data.stardustLevel * stardustPerUpgrade;
    }

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
