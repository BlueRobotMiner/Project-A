using UnityEngine;

public class PlayerStardust : MonoBehaviour
{
    public int stardust = 0;

    private bool banked;

    public void Add(int amount)
    {
        stardust += amount;
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
