using System.Collections;
using UnityEngine;

public class Trebuchet : Unit
{
    [SerializeField] string hitSFX;
    public string HitSFX => hitSFX;

    [SerializeField] string projectileSFX;
    public override string ProjectileSFX => projectileSFX;

    [Header("")]
    [SerializeField] int minAttackRange;  // 최소 공격 사거리
    public override int MinAttackRange => minAttackRange;

    [SerializeField] Rock rock;
    public override Rock Rock => rock;

    // 스탯 + 최소 사거리 설정
    public override void SetStat(UnitData unitData)
    {
        base.SetStat(unitData);

        TrebuchetData trebuchetData = unitData as TrebuchetData;
        minAttackRange = trebuchetData.MinAttackRange;
    }

    // 이동 이펙트 활성화
    public override IEnumerator GenerateMoveVFX(int pathTilesCount)
    {
        if (IsAlly || !IsConcealment)
        {
            MoveVFX.SetActive(true);
        }

        if (!IsAlly)
        {
            while (NowMoving)
            {
                MoveVFX.SetActive(!IsConcealment);
                yield return new WaitForSeconds(0.1f);
            }
        }
    }
}
