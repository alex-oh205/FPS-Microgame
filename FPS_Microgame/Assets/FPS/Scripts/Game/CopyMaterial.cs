using UnityEngine;

public class CopyMaterial : MonoBehaviour
{
    // Attach this to a gameObject that has a renderer.
    // Copies any property mat has and assigns it to this transform material

    public Material mat;

    void Start()
    {
        mat.CopyPropertiesFromMaterial(GetComponent<MeshRenderer>().material);
    }
}