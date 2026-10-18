using Unity.Netcode;
using UnityEngine;

public class LobbyScene : MonoBehaviour
{
    [SerializeField] GameObject networkManagerPrefeb;

    void Start()
    {
        GameObject networkManager = Instantiate(networkManagerPrefeb);
    }
}
