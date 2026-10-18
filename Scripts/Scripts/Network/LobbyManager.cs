using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.CloudCode;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using UnityEngine;
using UnityEngine.SceneManagement;
using WebSocketSharp;

public class LobbyManager : MonoBehaviour
{
    // 싱글턴
    public static LobbyManager instance;

    // 플레이어 아이디
    [SerializeField] string playerId;
    public string PlayerId => playerId;

    // 메인 로비
    Lobby mainLobby;
    public Lobby MainLobby => mainLobby;

    // 선택한 장군
    [SerializeField] string myGeneral = "";
    public string MyGeneral { get => myGeneral; set => myGeneral = value; }

    [SerializeField] string opponentGeneral = "";
    public string OpponentGeneral { get => opponentGeneral; set => opponentGeneral = value; }

    // 로그인 완료
    bool isLogin = false;
    public bool IsLogin => isLogin;

    // 매칭 중
    bool isMatching = false;
    public bool IsMatching => isMatching;

    void Awake()
    {
        // 인스턴스에 값이 없으면 자기자신 할당
        if (instance == null)
        {
            instance = this;

            // 사라지지 않음
            DontDestroyOnLoad(gameObject);
        }

        // 인스턴스에 값이 있으면 삭제
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    // Unity 사용자 인증
    public async void InitializeServices()
    {
        // Unity Services 초기화
        await UnityServices.InitializeAsync();

        // 기존 로그인 세션 초기화 (테스트를 위한 코드)
        AuthenticationService.Instance.ClearSessionToken();

        // 익명 로그인
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        // 플레이어 아이디 가져오기
        playerId = AuthenticationService.Instance.PlayerId;

        if (playerId != null)
        {
            isLogin = true;
        }
    }

    // 로비 참여
    public async Task JoinLobby()
    {
        while (true)
        {
            try
            {
                // 메인 로비가 있으면 참여 없으면 생성
                Debug.LogError($"메인 로비 불러오기 시도");
                QueryLobbiesOptions queryOption = new QueryLobbiesOptions
                {
                    Filters = new List<QueryFilter> { new QueryFilter(QueryFilter.FieldOptions.Name, "MainLobby", QueryFilter.OpOptions.EQ) }
                };

                QueryResponse lobbyList = await LobbyService.Instance.QueryLobbiesAsync(queryOption);
                mainLobby = lobbyList.Results.FirstOrDefault();

                if (mainLobby == null)
                {
                    Debug.LogError("메인 로비 없음");
                    await Task.Delay(UnityEngine.Random.Range(100, 301));
                    mainLobby = await LobbyService.Instance.CreateLobbyAsync("MainLobby", 20);
                    Debug.LogError("메인 로비 생성 완료");
                }

                else
                {
                    Debug.LogError("메인 로비 있음");
                    // 로비에 이미 참여되어 있는 경우
                    var joinedLobby = await LobbyService.Instance.GetJoinedLobbiesAsync();
                    if (joinedLobby.Contains(mainLobby.Id))
                    {
                        Debug.LogError("로비에 이미 참여 중");
                    }

                    else
                    {
                        mainLobby = await LobbyService.Instance.JoinLobbyByIdAsync(mainLobby.Id);
                        Debug.LogError("메인 로비에 참여");
                    }
                }

                Dictionary<string, string> option = new Dictionary<string, string>
                {
                    { "Status", "InLobby" },
                };

                await UpdatePlayerData(option);

                // 로비의 호스트면 하트비트 전송 시작
                if (mainLobby.HostId == playerId)
                {
                    StartCoroutine(HeartbeatLobbyCoroutine(mainLobby.Id));
                }

                // 로비 씬으로 이동
                SceneManager.LoadScene("1.LobbyScene");
                BGMManager.instance.PlayBGM("Lobby");
                return;
            }
            catch (Exception e)
            {
                Debug.LogError($"접속 실패: {e.Message}");
            }

            await Task.Delay(UnityEngine.Random.Range(1000, 1201));
        }
    }

    // 하트비트 전송
    IEnumerator HeartbeatLobbyCoroutine(string lobbyId)
    {
        while (true)
        {
            LobbyService.Instance.SendHeartbeatPingAsync(lobbyId);
            yield return new WaitForSecondsRealtime(15.0f);
        }
    }

    // 매칭 시작
    public async void StartMatch()
    {
        isMatching = true;

        Dictionary<string, string> option = new Dictionary<string, string>
        {
            { "Status", "Matching" },
            { "MatchingTime", DateTime.Now.Ticks.ToString() },
            { "SelectedGeneral", myGeneral }
        };

        // 서버에 요청
        await UpdatePlayerData(option);

        // 유저 찾기
        while (isMatching)
        {
            // 로비 불러오기
            mainLobby = await LoadMainLobby();

            // 매칭 중인 상태인 유저 찾기
            var matchingUser = mainLobby.Players.FirstOrDefault(p => p.Id != playerId && p.Data != null && p.Data.ContainsKey("Status") && p.Data["Status"].Value == "Matching");

            // 매칭 상태인 유저가 있는 경우
            if (matchingUser != null)
            {
                Debug.LogError($"매칭 된 유저 ID : {matchingUser.Id}");

                // 내가 매칭을 시작한 시간
                long myMatchingTime = long.Parse(mainLobby.Players.First(p => p.Id == playerId).Data["MatchingTime"].Value);

                // 상대방이 매칭을 시작한 시간
                long opponentMatchingTime = long.Parse(matchingUser.Data["MatchingTime"].Value);

                // 상대방이 선택한 장군 저장
                opponentGeneral = matchingUser.Data["SelectedGeneral"].Value;

                // 먼저 매칭을 시작한 유저 (같은 경우 아이디가 빠른 유저)
                if (myMatchingTime < opponentMatchingTime || (myMatchingTime == opponentMatchingTime && string.Compare(playerId, matchingUser.Id) < 0))
                {
                    // 방 생성 후 참여코드 공유
                    var joinCode = await RelayNetwork.instance.StartRelay();
                    Debug.LogError($"참여 코드 {joinCode}");
                    await ShareCode(joinCode);
                    Debug.LogError($"참여 코드 공유");

                    isMatching = false;
                    return;
                }

                // 늦게 시작한 유저
                else
                {
                    // 방 참여
                    if (await JoinRoom(matchingUser.Id))
                    {
                        Debug.LogError($"방 참여");
                        isMatching = false;
                        return;
                    }
                }
            }

            else
            {
                Debug.LogError("매칭 중인 다른 유저 못찾음");
                await Task.Delay(500);
            }
        }
    }

    // 매칭 취소
    public async void CancelMatch()
    {
        isMatching = false;

        Dictionary<string, string> option = new Dictionary<string, string>
        {
            { "Status", "InLobby" },
            { "MatchingTime", "" }
        };

        // 서버에 요청
        await UpdatePlayerData(option);
    }

    // Relay 참여코드 공유
    async Task ShareCode(string joinCode)
    {
        Dictionary<string, string> option = new Dictionary<string, string>
        {
            { "RelayCode", joinCode },
            { "Status", "InGame" }
        };

        // 서버에 요청
        await UpdatePlayerData(option);
    }

    // Relay 참여
    async Task<bool> JoinRoom(string opponentId)
    {
        while (true)
        {
            try
            {
                // 최신 로비 불러오기
                mainLobby = await LoadMainLobby();

                // 상대방 찾기
                var opponent = mainLobby.Players.Find(p => p.Id == opponentId);

                // 상대방에게 Relay 참여 코드 있는지 확인
                if (opponent != null && opponent.Data.ContainsKey("RelayCode"))
                {
                    // 참여 코드를 통해 참여
                    string code = opponent.Data["RelayCode"].Value;

                    if (!code.IsNullOrEmpty())
                    {
                        await RelayNetwork.instance.JoinRelay(code);

                        // 인게임 상태로 전환
                        Dictionary<string, string> option = new Dictionary<string, string>                    
                        {
                            { "Status", "InGame" }
                        };

                        // 서버에 요청
                        await UpdatePlayerData(option);

                        return true;
                    }
                }
            }
            catch (LobbyServiceException e)
            {
                Debug.LogWarning($"재시도 : {e}");
            }
            catch (RelayServiceException e)
            {
                Debug.LogWarning($"재시도 : {e}");
            }

            await Task.Delay(500);
        }
    }

    // 메인 로비 불러오기
    public async Task<Lobby> LoadMainLobby()
    {
        while (true)
        {
            // 최신 로비 불러오기
            Lobby lobby = await CloudCodeService.Instance.CallModuleEndpointAsync<Lobby>
            (
                "ServerModule", "LoadLobby",
                new Dictionary<string, object>
                {
                    { "lobbyId", mainLobby.Id }
                }
            );

            if (lobby != null)
            {
                return lobby;
            }

            else
            {
                await Task.Delay(500);
            }
        }
    }

    // 플레이어 데이터 업데이트 요청
    public async Task UpdatePlayerData(Dictionary<string, string> option)
    {
        // 서버에 요청
        await CloudCodeService.Instance.CallModuleEndpointAsync
        (
            "ServerModule", "UpdatePlayerData",
            new Dictionary<string, object>
            {
                { "lobbyId", mainLobby.Id },
                { "option", option}
            }
        );
    }

    //void OnGUI()
    //{
    //    // 폰트 스타일 설정
    //    GUIStyle myStyle = new GUIStyle();
    //    myStyle.fontSize = 25; // 글자 크기
    //    myStyle.normal.textColor = Color.green; // 글자 색상
    //    myStyle.alignment = TextAnchor.LowerRight; // 우측 하단 정렬

    //    // 여백 설정 (우측 하단 끝에서 20픽셀씩 뗌)
    //    float padding = 20f;
    //    Rect position = new Rect(0, 0, Screen.width - padding, Screen.height - padding);

    //    string text = playerId;

    //    if (mainLobby != null && mainLobby.HostId == playerId)
    //    {
    //        text += "(메인 로비 호스트)";
    //    }

    //    // 화면에 문구 표시
    //    GUI.Label(position, text, myStyle);
    //}
}
