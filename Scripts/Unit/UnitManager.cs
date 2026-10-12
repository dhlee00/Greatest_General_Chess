using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[System.Serializable]
public struct UnitSetInfoCopy
{
    [SerializeField] SetData[] setInfo;

    public SetData[] SetInfo { get => setInfo; set => setInfo = value; }
}

public class UnitManager : MonoBehaviour
{
    // 맵 매니저
    [SerializeField] MapManager m_Map;

    // 플레이어 매니저
    [SerializeField] PlayerManager m_Player;

    // 유닛 프리팹
    [SerializeField] public GameObject[] unitPrefab;

    // 유닛 외형 메테리얼
    [SerializeField] Material[] unitMeshMat;

    // 유닛 외곽선 메테리얼
    [SerializeField] Material[] unitOutlineMat;

    // 유닛 은폐 메테리얼
    [SerializeField] Material[] unitConcealmentMat;

    // 유닛 배치 정보
    [SerializeField] UnitSetInfo[] unitSetInfo;

    [SerializeField] UnitSetInfoCopy[] unitSetInfoCopy;
    public UnitSetInfoCopy[] UnitSetInfo => unitSetInfoCopy;

    // 장군 데이터
    Dictionary<string, GeneralData> generalData = new Dictionary<string, GeneralData>();
    public Dictionary<string, GeneralData> GeneralData => generalData;

    // 플레이어 1 유닛 리스트
    Dictionary<string, Unit> allyUnitList = new Dictionary<string, Unit>();
    public Dictionary<string, Unit> AllyUnitList { get => allyUnitList; set => allyUnitList = value; }

    // 플레이어 2 유닛 리스트
    Dictionary<string, Unit> enemyUnitList = new Dictionary<string, Unit>();
    public Dictionary<string, Unit> EnemyUnitList { get => enemyUnitList; set => enemyUnitList = value; }

    void Awake()
    {
        // Resources 폴더에서 모든 장군 데이터 불러오기
        GeneralData[] allGeneral = Resources.LoadAll<GeneralData>("GeneralData");

        // 장군의 이름을 키로 딕셔너리에 저장
        foreach (GeneralData general in allGeneral)
        {
            if (general == null) continue;

            if (!generalData.ContainsKey(general.GeneralCode))
            {
                generalData.Add(general.GeneralCode, general);
            }
        }

        CopySetInfo();
    }

    // 유닛 배치 정보 복사
    void CopySetInfo()
    {
        unitSetInfoCopy = new UnitSetInfoCopy[unitSetInfo.Length];

        for (int i = 0; i < unitSetInfo.Length; i++)
        {
            unitSetInfoCopy[i].SetInfo = new SetData[unitSetInfo[i].SetData.Length];

            for (int j = 0; j < unitSetInfo[i].SetData.Length; j++)
            {
                unitSetInfoCopy[i].SetInfo[j].UnitData = unitSetInfo[i].SetData[j].UnitData;

                unitSetInfoCopy[i].SetInfo[j].Coord = new Vector2Int[unitSetInfo[i].SetData[j].Coord.Length];

                for (int k = 0; k < unitSetInfo[i].SetData[j].Coord.Length; k++)
                {
                    unitSetInfoCopy[i].SetInfo[j].Coord[k] = unitSetInfo[i].SetData[j].Coord[k];
                }
            }
        }
    }

    // 플레이어 유닛 세팅
    public void SetPlayerUnit(int playerNum, string generalCode)
    {
        // 장군 할당
        unitSetInfoCopy[playerNum - 1].SetInfo[4].UnitData = generalData[generalCode];

        SetUnit(unitSetInfoCopy[playerNum - 1], playerNum);
    }

    // 유닛 배치
    void SetUnit(UnitSetInfoCopy unitSet, int playerNum)
    {
        // 유닛 배치 정보 순회
        foreach (SetData setData in unitSet.SetInfo)
        {
            int num = 0;

            // 좌표 순회
            foreach (Vector2Int coord in setData.Coord)
            {
                // 유닛 데이터, 좌표, 진영 설정
                GenerateUnit(setData.UnitData, coord, playerNum, num++);
            }
        }
    }

    // 유닛 생성
    void GenerateUnit(UnitData unitData, Vector2Int coord, int playerNum, int unitNum)
    {
        // 나 자신인지 여부
        bool isMe = NetworkManager.Singleton.IsServer == (playerNum == 1);

        // 유닛 데이터 종류 별로 유닛 생성 + 유닛 정보 불러오기
        GameObject unit = null;
        Unit unitInfo = null;
        switch (unitData.UnitType)
        {
            case UnitType.Infantry:
                unit = Instantiate(unitPrefab[0]);
                break;

            case UnitType.Cavalry:
                unit = Instantiate(unitPrefab[1]);
                break;

            case UnitType.HeavyCavalry:
                unit = Instantiate(unitPrefab[2]);
                break;

            case UnitType.Trebuchet:
                unit = Instantiate(unitPrefab[3]);
                unitInfo = unit.GetComponent<Trebuchet>();
                break;

            case UnitType.General:
                unit = Instantiate(unitPrefab[4]);
                unitInfo = unit.GetComponent<General>();

                if (isMe)
                {
                    m_Player.Ally_General = unitInfo as General;
                }

                else
                {
                    m_Player.Enemy_General = unitInfo as General;
                }
                break;

            case UnitType.King:
                unit = Instantiate(unitPrefab[5]);
                break;
        }

        if (unitInfo == null)
        {
            unitInfo = unit.GetComponent<Unit>();
        }

        // 유닛 매니저 전달
        unitInfo.GetUnitManager(this);

        // 맵 매니저 전달
        unitInfo.GetMapManager(m_Map);

        // 플레이어 매니저 전달
        unitInfo.GetPlayerManager(m_Player);

        // 유닛 데이터 바탕으로 스탯 복사
        unitInfo.SetStat(unitData);

        // 고유 ID 부여
        unitInfo.UnitID = $"P{playerNum}_{unitInfo.UnitType}_{unitNum}";

        // 배치
        unitInfo.Tile = m_Map.TileList[coord];

        // 오브젝트 이름, 태그, 부모, 외형 번호 설정
        unit.name = unitData.name;

        // 메테리얼 번호
        int matNum = 0;

        // 아군 및 레이어 설정
        unit.layer = isMe ? 7 : 8;
        unitInfo.IsAlly = isMe ? true : false;

        // 머티리얼 번호 결정
        matNum = isMe ? 0 : 1;

        // 외형 적용
        unitInfo.MeshMat = unitMeshMat[matNum];
        unitInfo.OutlineMat = unitOutlineMat[matNum];
        unitInfo.ConcealmentMat = unitConcealmentMat[matNum];
        unitInfo.SetMat(SetMatMode.Default);

        Dictionary<string, Unit> unitList;

        // 딕셔너리에 추가
        unitList = isMe ? allyUnitList : enemyUnitList;

        unitList.Add(unitInfo.UnitID, unitInfo);
    }

    public void ChangeUnitPos(int playerNum, string side)
    {
        SettingResult result = new SettingResult
        {
            Left = (side == "Left"),
            Right = (side == "Right")
        };

        ChangeUnitPos(playerNum, result);
    }

    public void ChangeUnitPos(int playerNum, SettingResult settingResult)
    {
        int yPos = (playerNum == 1) ? 13 : 0;

        int left_1 = (playerNum == 1) ? 3 : 9;
        int left_2 = (playerNum == 1) ? 5 : 11;

        int right_1 = (playerNum == 1) ? 9 : 3;
        int right_2 = (playerNum == 1) ? 11 : 5;

        if (settingResult.Left)
        {
            SwapUnit(new Vector2Int(left_1, yPos), new Vector2Int(left_2, yPos));
        }

        if (settingResult.Right)
        {
            SwapUnit(new Vector2Int(right_1, yPos), new Vector2Int(right_2, yPos));
        }
    }

    private void SwapUnit(Vector2Int pos1, Vector2Int pos2)
    {
        Tile tile1 = m_Map.TileList[pos1];
        Tile tile2 = m_Map.TileList[pos2];

        Unit unit1 = tile1.Unit;
        Unit unit2 = tile2.Unit;

        unit1.Tile = tile2;
        unit2.Tile = tile1;

        unit1.SetPos();
        unit2.SetPos();
    }
}
