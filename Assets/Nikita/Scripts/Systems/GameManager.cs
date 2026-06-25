using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int killCount;

    private void Awake()
    {
        Instance = this;
    }

    public void AddKill()
    {
        killCount++;
    }
}