using TMPro;
using UnityEngine;

public class DamageManager : MonoBehaviour
{
    // 카메라
    Transform camTransform;

    Unit unit;

    [SerializeField] TextMeshProUGUI hitDamageText;
    [SerializeField] TextMeshProUGUI MissDamageText;

    [SerializeField] GameObject damageVFX;

    void Start()
    {
        if (Camera.main != null)
        {
            camTransform = Camera.main.transform;
        }

        unit = transform.parent.GetComponent<Unit>();
    }

    void Update()
    {
        if (camTransform == null) return;

        transform.rotation = camTransform.rotation;
    }

    public void ShowDamage(Damage damage)
    {
        if (!damage.isMiss)
        {
            hitDamageText.text = damage.value.ToString();
            hitDamageText.gameObject.SetActive(true);
        }

        else
        {
            MissDamageText.text = damage.value.ToString();
            MissDamageText.gameObject.SetActive(true);
        }
    }

    public void ShowVFX(bool isNegation)
    {
        Vector3 pos = unit.transform.position;
        pos.y = 2;

        if (!isNegation)
        {
            Instantiate(damageVFX, pos, Quaternion.identity);
        }

        else
        {
            // 데미지 무효화 VFX 출력
            foreach (SkillData skill in unit.AppliedSkill)
            {
                if (skill.SkillID == "OO")
                {
                    Instantiate(skill.TargetVFXPrefab, pos, Quaternion.identity);
                }
            }
        }
    }
}
