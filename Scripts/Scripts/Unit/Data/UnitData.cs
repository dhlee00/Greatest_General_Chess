using UnityEngine;

// 유닛 종류
public enum UnitType
{
    Infantry,      // 보병
    Cavalry,       // 경기병
    HeavyCavalry,  // 중장기병
    Trebuchet,     // 트레뷰셋
    General,       // 장군
    King           // 킹
}

// 공격 방식
public enum AttackType
{
    CloseCombat,  // 일반 공격
    DirectFire,   // 직사 공격
    IndirectFire  // 곡사 공격
}

// 유닛 데이터
[CreateAssetMenu(fileName = "New Unit", menuName = "Game Data/Unit/Unit Data")]
public class UnitData : ScriptableObject
{
    [SerializeField] UnitType unitType;
    [SerializeField] int valuePoint;     // 가치 점수
    [SerializeField] AttackType attackType;
    [SerializeField] float attackPower;  // 공격력
    [SerializeField] int attackRange;    // 공격 사거리
    [SerializeField] float penetration;  // 방어 관통력
    [SerializeField] float defense;      // 방어력
    [SerializeField] float health;       // 체력
    [SerializeField] int movement;       // 기동력

    public UnitType UnitType => unitType;
    public int ValuePoint => valuePoint;
    public AttackType AttackType => attackType;
    public float AttackPower => attackPower;
    public int AttackRange => attackRange;
    public float Penetration => penetration;
    public float Defense => defense;
    public float Health => health;
    public int Movement => movement;
}
