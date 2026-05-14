using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class ChangeScenes : NetworkBehaviour
{
    public void EnterGame()
    {
        SceneManager.LoadScene("SampleScene");
    }
}
