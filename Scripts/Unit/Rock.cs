using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rock : MonoBehaviour
{
    Trebuchet trebuchet;

    [SerializeField] GameObject rockEffect;
    [SerializeField] GameObject rockMesh;

    [SerializeField] GameObject explosionEffect;

    float duration = 1.0f;

    float maxHeight = 8.0f;

    void Start()
    {
        trebuchet = transform.parent.GetComponent<Trebuchet>();
    }

    public void Throw(Tile attackTile, Damage damage)
    {
        rockEffect.SetActive(true);
        rockMesh.SetActive(true);
        StartCoroutine(ThrowRock(attackTile, damage));
    }

    IEnumerator ThrowRock(Tile attackTile, Damage damage)
    {
        SFXManager.instance.PlaySFX(trebuchet.ProjectileSFX);

        Vector3 startPos = trebuchet.UnitMesh.transform.position;

        startPos += trebuchet.UnitMesh.transform.up * 1.7f;
        startPos += trebuchet.UnitMesh.transform.forward * 1f;

        Vector3 endPos = attackTile.transform.position;

        Vector3 dir = (endPos - startPos).normalized;

        if (dir != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(dir);
        }

        float timer = 0f;

        while (timer < duration)
        {
            float t = timer / duration;

            Vector3 nowPos = Vector3.Lerp(startPos, endPos, t);

            float height = 4.0f * maxHeight * t * (1.0f - t);

            transform.position = new Vector3(nowPos.x, nowPos.y + height, nowPos.z);

            transform.localRotation *= Quaternion.Euler(-2f, 0f, 0f);

            timer += Time.deltaTime;

            yield return null;
        }

        transform.position = endPos;
        rockMesh.SetActive(false);

        // 이펙트 생성
        Vector3 effectPos = attackTile.TileData.TilePos;
        effectPos.y -= 0.05f;
        Instantiate(explosionEffect, effectPos, Quaternion.identity);

        SFXManager.instance.PlaySFX(trebuchet.HitSFX);

        if (attackTile.Unit != null && trebuchet.IsAlly != attackTile.Unit.IsAlly)
        {
            // 피해 입힘
            attackTile.Unit.TakeDamage(damage);
        }

        // 범위 공격 타겟 리스트
        List<Unit> splashTargetList = new List<Unit>();

        // 공격 대상 주변 타일 순회
        foreach (var tile in trebuchet.M_Map.FindPath.NeighborTiles(attackTile.TileData.Coord))
        {
            // 타일에 적 유닛이 있는 경우
            if (tile.Unit != null && trebuchet.IsAlly != tile.Unit.IsAlly)
            {
                // 공격 대상에 추가
                splashTargetList.Add(tile.Unit);
            }
        }

        // 공격 대상이 1명 이상인 경우
        if (splashTargetList.Count > 0)
        {
            // 공격
            foreach (var targetUnit in splashTargetList)
            {
                // 공격력 절반으로 적용
                Damage splashDamage = trebuchet.CalculateDamage(targetUnit, true);

                // 데미지
                SkillValue<Damage> calculateDamage = new SkillValue<Damage>
                {
                    value = splashDamage
                };

                // 데미지 무효화 확인
                if (targetUnit.CheckDamageNegation != null && targetUnit.CheckDamageNegation.HasCalls)
                {
                    targetUnit.CheckDamageNegation.Invoke(calculateDamage);
                }

                if (trebuchet.IsAlly)
                {
                    // 공격
                    targetUnit.TakeDamage(calculateDamage.value);

                    // 동기화
                    GameManager.instance.UnitDamageRpc(targetUnit.UnitID, calculateDamage.value.value, calculateDamage.value.isMiss);
                }
            }
        }

        trebuchet.EndAttack = true;

        // 행동 종료
        trebuchet.M_Player.EndAction();

        trebuchet.NowAttacking = false;
    }
}
