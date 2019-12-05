using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class IKShape : MonoBehaviour
{
    public Transform child;
    public Transform parent;
    public Transform target;

    public List<Transform> bones;

    private void Update()
    {
        Transform current = child;
        bones = new List<Transform>();
        int number = 0;
        do 
        {
            bones.Add(current);
            current = current.parent;
            number++;
        }
        while (current != null && current != parent) ;

        Vector2 tar = new Vector2(target.position.x, target.position.z);

        for (int i = 1; i < bones.Count - 1; i++)
        {
            Vector2 posBone = new Vector2(bones[i].position.x, bones[i].position.z);
            Vector2 goalBone = new Vector2(bones[i-1].position.x, bones[i-1].position.z);

            Quaternion pos = ikSolve(posBone, goalBone);
            bones[i].rotation = pos;
            
        }
    }

    Quaternion ikSolve(Vector2 bone, Vector2 goal)
    {
        float angle = Mathf.Atan2(bone.x, bone.y);
        Vector3 rot = new Vector3(0f, angle * Mathf.Rad2Deg, 0f);

        Vector3 target = bone - goal;
        Matrix4x4 matrix = Matrix4x4.TRS(bone, Quaternion.LookRotation(target, rot), Vector3.one);

        Quaternion quat = Quaternion.LookRotation(target, rot);

        return quat;
    }
    

    private void OnDrawGizmos()
    {
        //Ray r = new Ray(Vector3.zero, Vector3.forward);
        //Gizmos.DrawRay(r);
    }



    private void IKSolver()
    {
        for(int i = 1; i < bones.Count-1; i++)
        {
            Transform current = bones[i];
            Transform next = bones[i + 1];
            Transform next2 = bones[i + 2];

            float l1 = Vector3.Distance(current.position, next.position);
            if(i == 1)
                l1 = Vector3.Distance(target.position, current.position);
            float l2 = Vector3.Distance(next.position, next2.position);

            float theta1 = Mathf.PI / 2;
            float theta2 = Mathf.PI;

            Vector2 THETA = new Vector2(theta1, theta2);

            float X = Mathf.Cos(THETA.x) + l2 + Mathf.Cos(THETA.x + THETA.y);
            float Y = Mathf.Sin(THETA.x) + l2 + Mathf.Sin(THETA.x + THETA.y);

            Debug.Log(X + " " + Y);

        }
    }

    Vector2 updateIK(Vector2 target)
    {
        // convert from parent to local coordinates
        Vector2 localTarget = rotatePoint(translatePoint(target, -transform.position.x, -transform.position.z), transform.rotation.y);

        Vector2 endPoint;
        if (transform.childCount >= 1)
        {
            Vector2 end = new Vector2(transform.GetChild(0).position.x, transform.GetChild(0).position.z);
            endPoint = updateIK(new Vector2(transform.GetChild(0).position.x, transform.GetChild(0).position.y));
        }
        else
        {
            // base case:  the end point is the end of the current bone
            endPoint = new Vector2(target.x, target.y);
        }

        // point towards the endpoint
        float shiftAngle = angle(localTarget) - angle(endPoint);
        transform.rotation *= Quaternion.Euler(new Vector3(0f, shiftAngle,0f));

        // convert back to parent coordinate space
        return translatePoint(rotatePoint(endPoint, 1f), transform.position.x, transform.position.z);
    }

    Vector2  rotatePoint(Vector2 point, float angle)
    {
        float x = point.x;
        float y = point.y;
        return new Vector2(x * Mathf.Cos(angle) - y * Mathf.Sin(angle), x * Mathf.Sin(angle) + y * Mathf.Cos(angle));
    }
    Vector2 translatePoint(Vector2 point, float h, float v)
    {
        return new Vector2(point.x + h, point.y + v);
    }

    float angle(Vector2 point)
    {
        return Mathf.Atan2(point.y, point.x);
    }
}
public static class MatrixExtensions
{
    public static Quaternion ExtractRotation(this Matrix4x4 matrix)
    {
        Vector3 forward;
        forward.x = matrix.m02;
        forward.y = matrix.m12;
        forward.z = matrix.m22;

        Vector3 upwards;
        upwards.x = matrix.m01;
        upwards.y = matrix.m11;
        upwards.z = matrix.m21;

        return Quaternion.LookRotation(forward, upwards);
    }

    public static Vector3 ExtractPosition(this Matrix4x4 matrix)
    {
        Vector3 position;
        position.x = matrix.m03;
        position.y = matrix.m13;
        position.z = matrix.m23;
        return position;
    }

    public static Vector3 ExtractScale(this Matrix4x4 matrix)
    {
        Vector3 scale;
        scale.x = new Vector4(matrix.m00, matrix.m10, matrix.m20, matrix.m30).magnitude;
        scale.y = new Vector4(matrix.m01, matrix.m11, matrix.m21, matrix.m31).magnitude;
        scale.z = new Vector4(matrix.m02, matrix.m12, matrix.m22, matrix.m32).magnitude;
        return scale;
    }
}