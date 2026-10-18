using UnityEngine;

public class Cavalry : Unit
{
    [SerializeField] string projectileSFX;
    public override string ProjectileSFX => projectileSFX;

    [Header("")]
    [SerializeField] Arrow arrow;
    public override Arrow Arrow => arrow;
}
