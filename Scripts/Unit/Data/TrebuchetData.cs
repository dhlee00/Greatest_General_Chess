using UnityEngine;

[CreateAssetMenu(fileName = "New Trebuchet", menuName = "Game Data/Unit/Trebuchet Data")]
public class TrebuchetData : UnitData
{
    [SerializeField] int minAttackRange;  // 최소 공격 사거리
    public int MinAttackRange => minAttackRange;
}
