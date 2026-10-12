using UnityEngine;

public class UnitMesh : MonoBehaviour
{
    // À¯´Ö ¿ÜÇü
    [SerializeField] MeshRenderer[] mr;
    public MeshRenderer[] Mr => mr;

    [SerializeField] SkinnedMeshRenderer[] smr;
    public SkinnedMeshRenderer[] Smr => smr;

    public void SetMat(Material mat)
    {
        foreach (MeshRenderer mr in mr)
        {
            mr.material = mat;
        }

        foreach (SkinnedMeshRenderer smr in smr)
        {
            smr.material = mat;
        }
    }

    public void HideMesh(bool hide)
    {
        foreach (MeshRenderer mr in mr)
        {
            mr.enabled = !hide;
        }

        foreach (SkinnedMeshRenderer smr in smr)
        {
            smr.enabled = !hide;
        }
    }
}
