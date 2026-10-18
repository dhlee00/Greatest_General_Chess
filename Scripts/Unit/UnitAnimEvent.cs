using System.Collections;
using UnityEngine;

public class UnitAnimEvent : MonoBehaviour
{
    // 유닛 클래스
    Unit unit;

    Tile attackTile;
    Damage damage;

    void Start()
    {
        unit = transform.parent.GetComponent<Unit>();
    }

    public void SetTargetAndDamage(Tile _attackTile, Damage _damage)
    {
        attackTile = _attackTile;
        damage = _damage;
    }

    void StartAttack()
    {
        switch (unit.AttackType)
        {
            case AttackType.CloseCombat:
                SFXManager.instance.PlaySFX(unit.AttackSFX);
                unit.CloseCombatVFX.SetActive(true);
                break;

            case AttackType.DirectFire:
                unit.Arrow.Shoot(attackTile.Unit, damage);
                break;

            case AttackType.IndirectFire:
                unit.Rock.Throw(attackTile, damage);
                break;
        }
    }

    void GiveDamage()
    {
        attackTile.Unit.TakeDamage(damage);
    }

    void EndAttack()
    {
        StartCoroutine(EndAttackCo());
    }

    IEnumerator EndAttackCo()
    {
        // 정면 바라보기
        Quaternion nowRot = unit.UnitMesh.transform.localRotation;
        float time = 0;

        while (time < 0.1f)
        {
            time += Time.deltaTime;
            float timer = time / 0.1f;

            unit.UnitMesh.transform.localRotation = Quaternion.Slerp(nowRot, Quaternion.Euler(0, 0, 0), timer);
            yield return null;
        }

        if (unit.AttackType == AttackType.CloseCombat)
        {
            // 공격 한 상태
            unit.EndAttack = true;

            // 행동 종료
            unit.M_Player.EndAction();

            unit.NowAttacking = false;
        }
    }

    void Die()
    {
        if (unit.UnitType == UnitType.King)
        {
            string result = "Victory";

            if (unit.IsAlly)
            {
                result = "Defeat";
            }

            GameManager.instance.GameOver(result);
        }

        unit.M_Player.AllUnitCheckCanConcealment();
        transform.parent.gameObject.SetActive(false);
    }
}
