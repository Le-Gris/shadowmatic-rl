using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowObject : MonoBehaviour
{
    public void ResetRotation(Quaternion rotation)
    {
        this.transform.rotation = rotation;
    }
}
