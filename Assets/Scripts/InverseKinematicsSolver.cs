using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InverseKinematicsSolver : MonoBehaviour
{
    public Transform pos1;
    public Transform pos2;
    public Transform pos3;
    public Transform target;

    /// <summary>
    /// Length between pos1 and pos2
    /// </summary>
    public float lengthP1P2;
    /// <summary>
    /// Length between pos2 and pos3
    /// </summary>
    public float lengthP2P3;

    void Update()
    {
        lengthP1P2 = Vector3.Distance(pos1.position, pos2.position);
        lengthP2P3 = Vector3.Distance(pos2.position, pos3.position);

        Vector3 newPos3 = target.position * lengthP2P3;
        pos3.position = newPos3;
    }
}
