using UnityEngine;
using TMPro;

public class NewMonoBehaviourScript : MonoBehaviour
{
    public TMP_Text killText;

    void Update()
    {
        killText.text = "Kills: " + GameManager.Instance.killCount;
    }
}
