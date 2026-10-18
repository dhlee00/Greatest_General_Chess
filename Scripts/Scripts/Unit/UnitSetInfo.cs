using UnityEngine;

// 배치 정보
[System.Serializable]
public struct SetData
{
    [SerializeField] UnitData unitData;   // 유닛 데이터
    [SerializeField] Vector2Int[] coord;  // 배치 좌표

    public UnitData UnitData { get => unitData; set => unitData = value; }
    public Vector2Int[] Coord { get => coord; set => coord = value; }
}

[CreateAssetMenu(fileName = "New UnitSet", menuName = "UnitSet")]
public class UnitSetInfo : ScriptableObject
{
    [SerializeField] SetData[] setData;

    public SetData[] SetData => setData;
}
