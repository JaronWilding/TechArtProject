using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class IK_Solver : MonoBehaviour
{
    [SerializeField] private Transform Target;
    [SerializeField] private Transform EndEffector;
    [SerializeField] private Transform BaseEffector;

    [SerializeField] private List<Transform> bones;
    [SerializeField] private List<float> boneLength;
    // Start is called before the first frame update
    void Start()
    {
        bones = new List<Transform>();
        boneLength = new List<float>();
        Transform current = EndEffector;
        while(current != null && current != BaseEffector.parent)
        {
            bones.Add(current);
            boneLength.Add(Vector3.Distance(current.position, current.parent.position));
            current = current.parent;
            
        }
    }

    // Update is called once per frame
    void Update()
    {
        Solve();
    }
    private void Solve()
    {
        Vector3 movePos = new Vector3(0, 0, 0);
        for(int i = 0; i < bones.Count; i++)
        {
            Transform currentTrans = bones[i];
            Vector3 goalTrans = bones[i].position;
            if (i == 0)
                goalTrans = Target.position;
            else
                goalTrans = movePos;//bones[i - 1].position;

            
            Quaternion rot = angleBetween(goalTrans, currentTrans.position);
            currentTrans.rotation = rot;
            movePos =  goalTrans + (currentTrans.rotation.eulerAngles * -1f);
            
            
        }
    }

    Quaternion angleBetween(Vector3 v1, Vector3 v2)
    {
        float d = Vector3.Dot(v1, v2);
        Vector3 axis = v1;
        axis = Vector3.Cross(axis, v2);
        float qw = Mathf.Sqrt(v1.sqrMagnitude * v2.sqrMagnitude) + d;
        if (qw < 0.0001)
        { // vectors are 180 degrees apart
            return (new Quaternion(0, -v1.z, v1.y, v1.x)).normalized;
        }
        Quaternion q = new Quaternion(qw, axis.x, axis.y, axis.z);
        return q.normalized;
    }
}
