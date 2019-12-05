using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class GizmoDraw : MonoBehaviour
{
    [SerializeField] public Color colour = Color.blue;
    [SerializeField] public Color rayColour = Color.red;
    [SerializeField] public float radius = 0.2f;
    [SerializeField] private float boxRadius = 0.1f;
    [SerializeField] private Vector3 Trans;
    private void OnDrawGizmos()
    {
        Gizmos.color = colour;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireSphere(Vector3.zero, radius);
        
       if(transform.childCount > 0) {
            Transform child = transform.GetChild(0);
            float dist = Vector3.Distance(transform.position, child.position);
            Ray r = new Ray(Vector3.zero, Vector3.forward * dist);
            
            float angle = Mathf.Atan2(child.position.x, child.position.z);
            Vector3 rot = new Vector3(0f, angle * Mathf.Rad2Deg , 0f);

            Vector3 target = child.position - transform.position;
            
            //transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0.0f, Mathf.Rad2Deg * angle, 0.0f), desiredRotationSpeed);

            Gizmos.matrix = Matrix4x4.TRS(transform.position, Quaternion.LookRotation(target, rot), Vector3.one);
            Gizmos.color = rayColour;
            Gizmos.DrawRay(Vector3.zero, Vector3.forward * dist);
        }
    }
}
