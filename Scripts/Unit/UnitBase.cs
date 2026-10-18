using UnityEngine;

public class UnitBase : MonoBehaviour
{
    MeshRenderer mr;
    MaterialPropertyBlock propertyBlock;

    [SerializeField] Color allyColor;
    [SerializeField] Color enemyColor;

    [SerializeField] Color allySelectColor;
    [SerializeField] Color enemySelectColor;

    [SerializeField] float fadeValue;

    void Awake()
    {
        mr = GetComponent<MeshRenderer>();
        propertyBlock = new MaterialPropertyBlock();
    }

    // 유닛 발판 메테리얼 설정
    public void SetMat(bool isSelect, bool isAlly, bool isConcealment)
    {
        mr.GetPropertyBlock(propertyBlock);

        Color color;

        if (isSelect)
        {
            color = isAlly ? allySelectColor : enemySelectColor;
        }

        else
        {
            color = isAlly ? allyColor : enemyColor;
        }

        float fade = isConcealment ? fadeValue : 100f;

        propertyBlock.SetColor("_Color", color);
        propertyBlock.SetFloat("_Alpha", fade);

        mr.SetPropertyBlock(propertyBlock);
    }

    // 유닛 발판 투명도 조절
    public void SetFade(float value)
    {
        mr.GetPropertyBlock(propertyBlock);

        propertyBlock.SetFloat("_Alpha", value);

        mr.SetPropertyBlock(propertyBlock);
    }
}
