using UnityEngine;

public class StatsMob : MonoBehaviour
{
    public float BaseHP;
    public float BaseATK;
    public float BaseSPE;

    public static float HpMultiplier = 1f;
    public static float AtkMultiplier = 1f;
    public static float SpeMultiplier = 1f;

    public static void ResetMultipliers()
    {
        HpMultiplier = 1f;
        AtkMultiplier = 1f;
        SpeMultiplier = 1f;
    }

    void Start()
    {
        BaseHP = 50f * HpMultiplier;
        BaseATK = 15f * AtkMultiplier;
        BaseSPE = 3f * SpeMultiplier;

        if (SettingGame.Instance != null && SettingGame.Instance.mobPowerUp)
        {
            BaseATK *= 1.5f;
            BaseSPE *= 1.2f;
        }
    }

    public void LvUp()    
    {
        Debug.Log("MOB LV UP");
        HpMultiplier *= 1.3f;
        AtkMultiplier *= 1.15f;
        SpeMultiplier *= 1.02f;
    }
}
