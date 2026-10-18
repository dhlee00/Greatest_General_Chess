using System;
using System.Collections;
using UltEvents;
using Unity.Collections;
using Unity.Netcode;
using Unity.Services.Relay;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public enum RunMethod
{
    UnitSettingComplete,
    ReadyComplete
}

public class GameManager : NetworkBehaviour
{
    // 싱글턴
    public static GameManager instance;

    #region 매니저
    // UI 매니저
    UIManager m_UI;
    public UIManager M_UI { get => m_UI; set => m_UI = value; }

    // 맵 매니저
    MapManager m_Map;
    public MapManager M_Map { get => m_Map; set => m_Map = value; }

    // 유닛 매니저
    UnitManager m_Unit;
    public UnitManager M_Unit { get => m_Unit; set => m_Unit = value; }

    // 플레이어 매니저
    PlayerManager m_Player;
    public PlayerManager M_Player { get => m_Player; set => m_Player = value; }

    // 게임 오버
    GameOverManager m_GameOver;
    public GameOverManager M_GameOver { get => m_GameOver; set => m_GameOver = value; }
    #endregion

    #region 상태 변수
    // 유닛 위치 설정 완료
    bool unitSettingComplete = false;
    public bool UnitSettingComplete => unitSettingComplete;

    // 게임 준비 완료
    [SerializeField] bool readyComplete = false;
    public bool ReadyComplete { get => readyComplete; set => readyComplete = value; }
    #endregion

    #region 게임 관리 변수
    // 턴 수
    int turnCount = 0;

    // 턴 시간
    [Header("")]
    [SerializeField] float turnTime;

    // 행동력
    [SerializeField] int actionPoint = 2;
    public int ActionPoint => actionPoint;

    // 추가 턴 확인 대리자
    [SerializeField] UltEvent<SkillValue<bool>> checkExtraTurn;
    public UltEvent<SkillValue<bool>> CheckExtraTurn { get => checkExtraTurn; set => checkExtraTurn = value; }
    #endregion

    #region 동기화 변수
    [Header("")]

    [SerializeField] string p1_GeneralCode = "";  // 플레이어 1의 장군 코드
    [SerializeField] string p2_GeneralCode = "";  // 플레이어 2의 장군 코드

    [SerializeField] SettingResult p1_settingResult = new SettingResult();  // 플레이어 1 위치 세팅 결과
    [SerializeField] SettingResult p2_settingResult = new SettingResult();  // 플레이어 2 위치 세팅 결과

    // 플레이어 1 완료 신호
    bool p1_Complete;
    public bool P1_Complete => p1_Complete;

    // 플레이어 2 완료 신호 
    bool p2_Complete;
    public bool P2_Complete => p2_Complete;

    // 현재 턴
    [SerializeField] NetworkVariable<bool> isP1Turn = new NetworkVariable<bool>();
    public bool IsP1Turn => isP1Turn.Value;

    // 턴 제한 시간
    [SerializeField] NetworkVariable<float> turnTimer = new NetworkVariable<float>();
    public float TurnTimer => turnTimer.Value;
    #endregion

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

    // 스폰 될 때 실행
    public override void OnNetworkSpawn()
    {
        m_UI = FindAnyObjectByType<UIManager>();
        m_Map = FindAnyObjectByType<MapManager>();
        m_Unit = FindAnyObjectByType<UnitManager>();
        m_Player = FindAnyObjectByType<PlayerManager>();
        m_GameOver = FindAnyObjectByType<GameOverManager>();

        // 맵 생성
        m_Map.CreateMap();

        // 호스트인 경우
        if (IsServer)
        {
            // 호스트 로딩 완료 신호 보냄
            using (var data = new FastBufferWriter(0, Allocator.Temp))
            {
                NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage("LoadGameScene", 1, data);
                Debug.Log("완료 신호 보내기");
            }

            // 클라이언트로 받을 메세지
            NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("CloneGameManager", CloneGameManager);

            // 시간 세팅
            turnTimer.Value = turnTime;

            // 선공 후공 결정
            PickFirstTurn();

            // 로비 매니저에서 장군 이름 불러오기
            p1_GeneralCode = LobbyManager.instance.MyGeneral;
            p2_GeneralCode = LobbyManager.instance.OpponentGeneral;

            // 내 유닛만 생성
            m_Unit.SetPlayerUnit(1, p1_GeneralCode);
        }

        // 클라이언트인 경우
        else
        {
            // 로비 매니저에서 장군 이름 불러오기
            p1_GeneralCode = LobbyManager.instance.OpponentGeneral;
            p2_GeneralCode = LobbyManager.instance.MyGeneral;

            // 내 유닛만 생성
            m_Unit.SetPlayerUnit(2, p2_GeneralCode);
        }

        // 선수 소개
        if (NetworkManager.Singleton.IsServer)
        {
            StartCoroutine(m_UI.PlayerIntroduce.Introduction(m_Unit.GeneralData[p1_GeneralCode], m_Unit.GeneralData[p2_GeneralCode]));
        }

        else
        {
            StartCoroutine(m_UI.PlayerIntroduce.Introduction(m_Unit.GeneralData[p2_GeneralCode], m_Unit.GeneralData[p1_GeneralCode]));
        }

        m_UI.AllyUserGeneralImg.sprite = m_Unit.GeneralData[LobbyManager.instance.MyGeneral].GeneralImage.profileImg;
        m_UI.EnemyUserGeneralImg.sprite = m_Unit.GeneralData[LobbyManager.instance.OpponentGeneral].GeneralImage.profileImg;
    }

    void Update()
    {
        // 모든 준비 끝나고 실행
        if (readyComplete)
        {
            if (IsServer)
            {
                if (turnTimer.Value > 0.0f)
                {
                    turnTimer.Value -= Time.deltaTime;
                }
            }

            if (turnTimer.Value <= 0.0f)
            {
                if (!m_Player.IsActing)
                {
                    TurnOver();
                }

                else
                {
                    Debug.Log("연출 중 타이머 0.5초로 적용");
                    if (IsServer)
                    {
                        turnTimer.Value = 0.5f;
                    }
                }
            }

            m_UI.TurnTimerTxt.text = $"{string.Format("{0:00}", turnTimer.Value)}초";
        }
    }

    // 선공 후공 결정
    void PickFirstTurn()
    {
        int randomNum = UnityEngine.Random.Range(0, 2);

        if (randomNum == 0)
        {
            isP1Turn.Value = true;
        }

        else
        {
            isP1Turn.Value = false;
        }
    }

    // 코인토스 진행
    public void StartCoinToss()
    {
        m_UI.ShowCoinToss(isP1Turn.Value);

        // isP1Turn의 값이 바뀔 때 호출할 콜백 등록
        isP1Turn.OnValueChanged += m_UI.ResetTurnUI;
    }

    // 턴 종료
    public void TurnOver()
    {
        SkillValue<bool> getExtraTurn = new SkillValue<bool>
        {
            value = false
        };

        if (checkExtraTurn != null && checkExtraTurn.HasCalls)
        {
            checkExtraTurn.Invoke(getExtraTurn);
        }

        if (m_Player.alwaysExtraTurn)
        {
            getExtraTurn.value = true;
        }

        ChangeTurnRpc(getExtraTurn.value);
    }

    // 턴 변경
    public void ChangeTurn(bool extraTurn, ulong senderId)
    {
        // 턴 수 증가
        turnCount++;

        // 턴 수 출력
        m_UI.PrintTurn(turnCount);

        // 사용한 스킬 턴 확인
        m_Player.CheckSkillTurn();

        // 스킬 쿨타임 감소
        m_Player.SkillCoolDown();

        // 다음 턴 때 예약한 스킬 사용
        m_Player.UseReserveSkill();

        // 플레이어 상태 초기화
        m_Player.ResetState();

        // 행동력 초기화
        actionPoint = 2;

        // 호스트가 초기화
        if (IsServer)
        {
            // 턴 제한 시간
            turnTimer.Value = turnTime;

            // 턴 반대로
            isP1Turn.Value = !isP1Turn.Value;
        }

        if (extraTurn)
        {
            StartCoroutine(TakeTurn(senderId));
        }
    }

    // 턴 뺏기
    IEnumerator TakeTurn(ulong senderId)
    {
        Debug.Log("기만술 발동");

        General general;

        if (checkExtraTurn != null && checkExtraTurn.HasCalls)
        {
            general = m_Player.Ally_General;
        }

        else
        {
            general = m_Player.Enemy_General;
        }

        if (general.Skill[0].SkillID == "MD")
        {
            // 이펙트 출력
            GameObject effect = Instantiate(general.Skill[0].SkillVFXPrefab, Camera.main.transform);
            effect.transform.localPosition = new Vector3(0, 0, 3);
            effect.transform.localRotation = Quaternion.Euler(0, 0, 0);

            // 효과음 재생
            SFXManager.instance.PlaySFX(general.Skill[0].SfxID);
        }

        yield return new WaitForSeconds(2.6f);

        if (NetworkManager.Singleton.LocalClientId != senderId)
        {
            TurnOver();
        }
    }

    // 행동력 소모
    public void SpendActionPoint(int add = 1)
    {
        if (turnTimer.Value > 0.0f)
        {
            // 행동력 감소
            actionPoint -= add;

            // 행동력이 0인 경우
            if (actionPoint <= 0)
            {
                // 턴 종료
                TurnOver();
            }
        }
    }

    // 게임 오버
    public void GameOver(string result)
    {
        m_GameOver.GameOverView.SetActive(true);
        m_GameOver.ShowGameResult(result);
    }

    #region Rpc
    // 세팅 완료 신호 보내기
    [Rpc(SendTo.Server)]
    public void SendUnitPosSettingRpc(bool state, bool resultLeft, bool resultRight, RpcParams rpcParams = default)
    {
        // 보낸 클라이언트의 id 확인
        ulong senderId = rpcParams.Receive.SenderClientId;

        // 플레이어 1인 경우
        if (senderId == 0)
        {
            p1_Complete = state;
            p1_settingResult.Left = resultLeft;
            p1_settingResult.Right = resultRight;
        }

        // 플레이어 2인 경우
        else if (senderId == 1)
        {
            p2_Complete = state;
            p2_settingResult.Left = resultLeft;
            p2_settingResult.Right = resultRight;
        }

        // 호스트만 확인
        if (IsHost)
        {
            // 둘 다 완료 됐는지 확인
            if (p1_Complete && p2_Complete)
            {
                SetOpponentUnitRpc(p1_settingResult.Left, p1_settingResult.Right, p2_settingResult.Left, p2_settingResult.Right);

                p1_Complete = false;
                p2_Complete = false;
            }
        }
    }

    // 모든 클라이언트에게 상대방 유닛 생성 명령
    [Rpc(SendTo.Everyone)]
    public void SetOpponentUnitRpc(bool p1_ResultLeft, bool p1_ResultRight, bool p2_ResultLeft, bool p2_ResultRight)
    {
        m_UI.UnitPosSetting.WaittingView.SetActive(false);

        unitSettingComplete = true;

        m_UI.PrintStep(GameReadyStep.CoinToss);

        p1_settingResult.Left = p1_ResultLeft;
        p1_settingResult.Right = p1_ResultRight;
        p2_settingResult.Left = p2_ResultLeft;
        p2_settingResult.Right = p2_ResultRight;

        if (IsServer)
        {
            if (p2_settingResult.Left)
            {
                m_Unit.UnitSetInfo[1].SetInfo[2].Coord[1].x = 9;
                m_Unit.UnitSetInfo[1].SetInfo[3].Coord[1].x = 11;
            }

            if (p2_settingResult.Right)
            {
                m_Unit.UnitSetInfo[1].SetInfo[2].Coord[0].x = 5;
                m_Unit.UnitSetInfo[1].SetInfo[3].Coord[0].x = 3;
            }

            m_Unit.SetPlayerUnit(2, p2_GeneralCode);
        }

        else
        {
            if (p1_settingResult.Left)
            {
                m_Unit.UnitSetInfo[0].SetInfo[2].Coord[0].x = 5;
                m_Unit.UnitSetInfo[0].SetInfo[3].Coord[0].x = 3;
            }

            if (p1_settingResult.Right)
            {
                m_Unit.UnitSetInfo[0].SetInfo[2].Coord[1].x = 9;
                m_Unit.UnitSetInfo[0].SetInfo[3].Coord[1].x = 11;
            }

            m_Unit.SetPlayerUnit(1, p1_GeneralCode);
        }
    }

    // 준비 완료 신호 보내기
    [Rpc(SendTo.Server)]
    public void SendReadyCompleteRpc(bool state, RpcParams rpcParams = default)
    {
        // 보낸 클라이언트의 id 확인
        ulong senderId = rpcParams.Receive.SenderClientId;

        // 플레이어 1인 경우
        if (senderId == 0)
        {
            p1_Complete = state;
        }

        // 플레이어 2인 경우
        else if (senderId == 1)
        {
            p2_Complete = state;
        }

        // 호스트만 확인
        if (IsHost)
        {
            // 둘 다 완료 됐는지 확인
            if (p1_Complete && p2_Complete)
            {
                SetUIAndReadyCompleteRpc();

                p1_Complete = false;
                p2_Complete = false;
            }
        }
    }

    // 모든 클라이언트 초기 UI 세팅 및 준비 완료 상태로 전환
    [Rpc(SendTo.Everyone)]
    public void SetUIAndReadyCompleteRpc()
    {
        if (IsServer)
        {
            // 턴 종료 버튼
            m_UI.TurnOverBtnObject.SetActive(isP1Turn.Value);

            // 턴 휘장 활성화
            StartCoroutine(m_UI.ShowTurnImg());
        }

        else
        {
            // 턴 종료 버튼
            m_UI.TurnOverBtnObject.SetActive(!isP1Turn.Value);

            // 턴 휘장 활성화
            StartCoroutine(m_UI.ShowTurnImg());
        }

        // 유닛 가치 점수 계산
        foreach (Unit unit in m_Unit.AllyUnitList.Values)
        {
            m_Player.AllyUnit_ValuePoint += unit.ValuePoint;
        }

        foreach (Unit unit in m_Unit.EnemyUnitList.Values)
        {
            m_Player.EnemyUnit_ValuePoint += unit.ValuePoint;
        }

        // 장군 유닛 게임 시작 시 발동 스킬 사용
        m_Player.ActivateGameStartSkill();

        // 발동 조건이 있는 스킬 발동 조건 확인 시작
        m_Player.StartCheckOutNumbered();

        // 준비 완료
        readyComplete = true;
    }

    // 모든 플레이어 턴 전환 명령
    [Rpc(SendTo.Everyone)]
    public void ChangeTurnRpc(bool getExtraTurn, RpcParams rpcParams = default)
    {
        ChangeTurn(getExtraTurn, rpcParams.Receive.SenderClientId);
    }

    // 상대방에게 유닛 이동 명령
    [Rpc(SendTo.NotMe)]
    public void UnitMoveRpc(string unitID, Vector2Int destinationCoord)
    {
        Unit unit = m_Unit.EnemyUnitList[unitID];

        if (m_Player.EnemyControlUnit != unit)
        {
            m_Player.EnemyControlUnit = unit;

            unit.Selected(true);
        }

        Tile destination = m_Map.TileList[destinationCoord];

        StartCoroutine(unit.Move(destination));
    }

    // 상대방에게 유닛 공격 명령
    [Rpc(SendTo.NotMe)]
    public void UnitAttackRpc(string unitID, Vector2Int tileCoord, float damageValue, bool _isMiss, bool _isNegation)
    {
        Unit attacker = m_Unit.EnemyUnitList[unitID];

        if (m_Player.EnemyControlUnit != attacker)
        {
            m_Player.EnemyControlUnit = attacker;
            attacker.Selected(true);
        }

        Tile targetTile = m_Map.TileList[tileCoord];

        // 공격자가 은폐 상태면 1초만 모습을 보임
        if (attacker.IsConcealment)
        {
            StartCoroutine(attacker.ShowUnit());
        }

        Damage damage = new Damage
        {
            value = damageValue,
            isMiss = _isMiss,
            isNegation = _isNegation
        };

        // 공격
        StartCoroutine(attacker.Attack(targetTile, damage));
    }

    // 상대방에게 유닛 데미지 명령
    [Rpc(SendTo.NotMe)]
    public void UnitDamageRpc(string unitID, float damageValue, bool isMiss)
    {
        Unit myUnit = m_Unit.AllyUnitList[unitID];

        Damage damage = new Damage
        {
            value = damageValue,
            isMiss = isMiss
        };

        myUnit.TakeDamage(damage);
    }

    // 상대방에게 액티브 스킬 사용 명령
    [Rpc(SendTo.NotMe)]
    public void ReserveActiveSkillRpc(int skillIndex)
    {
        if (m_Player.Enemy_General.Skill[skillIndex].ActivateTiming == ActivateTiming.Activate)
        {
            // 스킬 예약
            ReserveSkill reserveSkill = new ReserveSkill
            {
                General = m_Player.Enemy_General,
                SkillIndex = skillIndex
            };

            m_Player.ReserveSkillList.Add(reserveSkill);
        }
    }

    // 상대방에게 스킬 발동 명령
    [Rpc(SendTo.NotMe)]
    public void ActivateSkillRpc(string skillID, bool onoff)
    {
        Debug.Log("동기화 실행");

        foreach (SkillData skill in m_Player.Enemy_General.Skill)
        {
            if (skill.SkillID == skillID)
            {
                if (onoff)
                {
                    skill.Activate(m_Player.Enemy_General);
                    Debug.Log("스킬 발동");
                }

                else
                {
                    skill.Deactivate(m_Player.Enemy_General);
                    Debug.Log("스킬 해제");
                }
            }
        }
    }
    #endregion

    // 게임 매니저 클론 복제
    public void CloneGameManager(ulong clientId, FastBufferReader reader)
    {
        NetworkObject.NetworkShow(clientId);

        NetworkManager.CustomMessagingManager.UnregisterNamedMessageHandler("CloneGameManager");
    }

    // 게임 매니저 없애기
    public void DestroyGameManager()
    {
        if (IsServer)
        {
            if (NetworkObject != null && NetworkObject.IsSpawned)
            {
                NetworkObject.Despawn(true);
                Destroy(gameObject);
            }
        }

        instance = null;
    }
}
