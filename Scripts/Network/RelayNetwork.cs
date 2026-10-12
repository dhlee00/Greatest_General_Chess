using System.Threading.Tasks;
using Unity.Collections;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RelayNetwork : MonoBehaviour
{
    // 싱글톤
    public static RelayNetwork instance;

    // 게임 매니저 프리팹
    [SerializeField] GameObject gameManagerPrefab;

    [SerializeField] bool isHostLoadComplete = false;
    public bool IsHostLoadComplete => isHostLoadComplete;

    void Awake()
    {
        instance = this;
    }

    // 호스트로 시작
    public async Task<string> StartRelay()
    {
        // Relay에 새 연결 슬롯을 만들고 2명까지 접속할 수 있도록 요청
        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(2);
        
        // Relay 참여 코드 생성
        string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

        // NetworkManager에서 UnityTransport 컴포넌트 참조
        var unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        // 네트워크 접속 설정
        unityTransport.SetHostRelayData
        (
            allocation.RelayServer.IpV4,          // 접속에 사용할 Relay 서버의 IP 주소
            (ushort)allocation.RelayServer.Port,  // 접속 포트 번호(형변환해서 전달)
            allocation.AllocationIdBytes,         // 이 할당(방)을 식별하는 바이너리 ID
            allocation.Key,                       // 통신을 보호하는 암호화 키
            allocation.ConnectionData             // 호스트 측 핸드셰이크에 필요한 데이터
        );

        // Relay를 호스트 모드로 시작
        NetworkManager.Singleton.StartHost();

        // 씬 로드 콜백 함수 등록
        SceneManager.sceneLoaded += GenerateGameManager;

        // 로딩씬으로 전환
        SceneManager.LoadScene("LoadingScene");

        return joinCode;
    }

    // 클라이언트로 참여
    public async Task JoinRelay(string joinCode)
    {
        Debug.LogError($"받은 참여 코드 {joinCode}");
        // 전달받은 코드로 Relay 접속
        JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

        // NetworkManager에서 UnityTransport 컴포넌트 참조
        var unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        // 네트워크 접속 설정
        unityTransport.SetClientRelayData
        (
            joinAllocation.RelayServer.IpV4,          // 접속에 사용할 Relay 서버의 IP 주소
            (ushort)joinAllocation.RelayServer.Port,  // 접속 포트 번호(형변환해서 전달)
            joinAllocation.AllocationIdBytes,         // 이 방(할당)을 식별하는 바이너리 ID
            joinAllocation.Key,                       // 통신을 보호하는 암호화 키
            joinAllocation.ConnectionData,            // 클라이언트 핸드셰이크에 필요한 데이터
            joinAllocation.HostConnectionData         // 호스트 연결 데이터
        );

        // Relay를 클라이언트 모드로 시작
        NetworkManager.Singleton.StartClient();

        // 씬 로드 콜백 함수 등록
        SceneManager.sceneLoaded += CloneGameManager;

        isHostLoadComplete = false;

        NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("LoadGameScene", (sender, message) =>
        {
            isHostLoadComplete = true;

            Debug.Log("호스트 로딩 완료");

            NetworkManager.Singleton.CustomMessagingManager.UnregisterNamedMessageHandler("LoadGameScene");
        });

        // 로딩 씬으로 전환
        SceneManager.LoadScene("LoadingScene");
    }

    // Relay 종료
    public void EndRelay()
    {
        // 현재 네트워크가 실행 중인지 확인
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            // 네트워크 연결 종료
            NetworkManager.Singleton.Shutdown();
        }
    }

    // 게임 매니저 생성
    void GenerateGameManager(Scene scene, LoadSceneMode mode)
    {
        // 게임씬인 경우
        if (scene.name == "2.GameScene")
        {
            // 게임 매니저 생성
            GameObject gm = Instantiate(gameManagerPrefab);
            gm.GetComponent<NetworkObject>().Spawn();

            // 생성 후 콜백 해제
            SceneManager.sceneLoaded -= GenerateGameManager;
        }
    }

    // 게임 매니저 복제
    void CloneGameManager(Scene scene, LoadSceneMode mode)
    {
        // 게임씬인 경우
        if (scene.name == "2.GameScene")
        {
            // 게임 매니저 복제 요청
            using (var data = new FastBufferWriter(0, Allocator.Temp))
            {
                NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage("CloneGameManager", 0, data);
            }

            // 복제 후 콜백 해제
            SceneManager.sceneLoaded -= CloneGameManager;
        }
    }
}
