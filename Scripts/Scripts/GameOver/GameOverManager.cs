using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    [SerializeField] GameObject gameOverView;
    public GameObject GameOverView => gameOverView;

    [SerializeField] GameObject victory;
    public GameObject Victory => victory;

    [SerializeField] GameObject defeat;
    public GameObject Defeat => defeat;

    public void ShowGameResult(string result)
    {
        if (result == "Victory")
        {
            victory.SetActive(true);
        }

        else if (result == "Defeat")
        {
            defeat.SetActive(true);
        }
    }

    public async void GoLobbyBtnAsync(GameObject btn)
    {
        btn.SetActive(false);

        Dictionary<string, string> option = new Dictionary<string, string>
        {
            { "Status", "InLobby" },
            { "MatchingTime", "" },
            { "SelectedGeneral", "" }
        };

        await LobbyManager.instance.UpdatePlayerData(option);

        LobbyManager.instance.MyGeneral = "";
        LobbyManager.instance.OpponentGeneral = "";

        if (NetworkManager.Singleton.IsServer)
        {
            Dictionary<string, string> relayReset = new Dictionary<string, string>
            {
                { "RelayCode", "" }
            };

            await LobbyManager.instance.UpdatePlayerData(relayReset);
        }

        if (GameManager.instance != null)
        {
            GameManager.instance.DestroyGameManager();
        }

        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
        }

        while (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            await Task.Yield();
        }

        Destroy(NetworkManager.Singleton.gameObject);

        await Task.Delay(1000);
        
        SceneManager.LoadScene("1.LobbyScene");
        BGMManager.instance.PlayBGM("Lobby");
    }
}
