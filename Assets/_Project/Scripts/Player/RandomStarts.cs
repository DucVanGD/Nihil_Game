using UnityEngine;

public class RandomStats : MonoBehaviour
{
    [Header("New Random Stats")]
    public float BasePoint = 300;
    public float NewHp;
    public float NewAtk;
    public float NewMp;

    void Start()
    {
        Debug.Log("Tao Random Stats Player");
    }

    public void GenerateRandomStats()
    {
        while (true)
        {
            NewHp = Random.Range(125, 176);  
            NewAtk = Random.Range(25, 36);
            NewMp = BasePoint - NewHp - NewAtk;

            if (NewMp >= 0)
                break;
        }
    }
}
