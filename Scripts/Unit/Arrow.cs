using System.Collections;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    Unit unit;

    [SerializeField] GameObject arrowVFX;
    [SerializeField] GameObject arrowMesh;
    [SerializeField] GameObject arrowVFXHead;

    float duration = 0.4f;

    void Start()
    {
        unit = transform.parent.GetComponent<Unit>();
    }

    public void Shoot(Unit target, Damage damage)
    {
        SFXManager.instance.PlaySFX(unit.AttackSFX);
        arrowVFX.SetActive(true);
        arrowMesh.SetActive(true);
        StartCoroutine(ShootArrow(target, damage));
    }

    IEnumerator ShootArrow(Unit target, Damage damage)
    {
        SFXManager.instance.PlaySFX(unit.ProjectileSFX);

        Vector3 startPos = unit.UnitMesh.transform.position;

        startPos += unit.UnitMesh.transform.up * 1.6f;
        startPos += unit.UnitMesh.transform.forward * 1.6f;

        Vector3 endPos = target.transform.position;
        endPos.y = startPos.y;

        Vector3 dir = (endPos - startPos).normalized;

        if (dir != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(dir);
        }

        // 거리에 따른 보정
        float distance = unit.M_Map.FindPath.GetHexDis(unit.Tile.Coord, target.Tile.Coord);
        duration = Mathf.Min(0.1f * distance, 0.4f);

        float timer = 0f;

        while (timer < duration)
        {
            float t = timer / duration;

            transform.position = Vector3.Lerp(startPos, endPos, t);

            timer += Time.deltaTime;

            yield return null;
        }

        transform.position = endPos;
        arrowMesh.SetActive(false);
        arrowVFXHead.SetActive(false);

        target.TakeDamage(damage);

        unit.EndAttack = true;

        // 행동 종료
        unit.M_Player.EndAction();

        unit.NowAttacking = false;
    }
}
