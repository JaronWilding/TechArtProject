using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class InverseKin : MonoBehaviour
{

    enum IKSystems
    {
        CustomIK,
        FabricIK
    }

    [Header("Solver Settings")]
    [SerializeField] public int iterations = 20;
    [SerializeField] public Transform Target;
    [SerializeField] public Transform Pole;
    //[SerializeField] public bool IKSystems = false;
    [SerializeField] IKSystems ikSystems;

    [Header("Joint Settings")]
    [SerializeField] public Transform A_Joint;
    [SerializeField] public Transform B_Joint;
    [SerializeField] public Transform C_Joint;

    
    void Update()
    {
        if (ikSystems == IKSystems.CustomIK)
            IKSystemCustom();
        else if(ikSystems == IKSystems.FabricIK)
            IKSystemFabrick();
    }

    private void IKSystemCustom()
    {
        for (int i = 0; i < iterations; i++)
        {
            float arm_length_a = Vector3.Distance(A_Joint.position, B_Joint.position);
            float arm_length_b = Vector3.Distance(B_Joint.position, C_Joint.position);

            Vector3 jnt_B_rot = FollowMouse(B_Joint.position, Target.position, arm_length_b);

            Vector3 jnt_B_pos = Target.position + (jnt_B_rot * -1);
            Vector3 jnt_A_rot = FollowMouse(A_Joint.position, jnt_B_pos, arm_length_a);

            Vector3 up = Vector3.up;
            Vector3 rotate = PoleVector(B_Joint.position, A_Joint.position, C_Joint.position);

            A_Joint.rotation = Quaternion.LookRotation((jnt_A_rot + rotate) * Mathf.Rad2Deg, up);
            B_Joint.rotation = Quaternion.LookRotation(jnt_B_rot * Mathf.Rad2Deg, up);
        }
    }

   

    private Vector3 PoleVector(Vector3 current, Vector3 last, Vector3 next)
    {
        Vector3 rot;
        Plane plane = new Plane(next - last, last);
        Vector3 projectedPole = plane.ClosestPointOnPlane(Pole.position);
        Vector3 projectedBone = plane.ClosestPointOnPlane(current);
        float angle = Vector3.SignedAngle(projectedBone - last, projectedPole - last, plane.normal);
        rot = Quaternion.AngleAxis(angle, plane.normal) * (current - last) + last;
        return rot;
    }

    private Vector3 FollowMouse(Vector3 joint_, Vector3 target_, float length_)
    {
        Vector3 dir = target_ - joint_;
        dir = dir.normalized * length_;
        return dir;
    }

    //------------------------------------

    private void IKSystemFabrick()
    {
        IKSystem_PartA();
    }

    protected static float[] BonesLength;
    protected static float CompleteLength;
    protected static Transform[] Bones;
    protected static Vector3[] Positions;
    protected static int ChainLength = 3;

    //protected Vector3[] StartDirectionSucc;
    //protected Quaternion[] StartRotationBone;
    //protected Quaternion StartRotationTarget;
    protected Quaternion StartRotationRoot;

    private void IKSystem_PartA()
    {

        Vector3 Pos_A = A_Joint.position;
        Vector3 Pos_B = B_Joint.position;
        Vector3 Pos_C = C_Joint.position;




        CompleteLength = Vector3.Distance(Pos_B, Pos_C) + Vector3.Distance(Pos_A, Pos_B);

        if ((Target.position - Pos_A).sqrMagnitude >= CompleteLength * CompleteLength)
        {
            Vector3 direction = (Target.position - Pos_A).normalized;
            Pos_B = Pos_A + direction * 1;
            Pos_C = Pos_B + direction * 1;
        }
        else
        {

            for (int iteration = 0; iteration < iterations; iteration++)
            {
                
                Pos_C = Target.position;
                Pos_B = Pos_C + ((Pos_B - Pos_C).normalized * 1);

                Pos_B = Pos_A + (Pos_B - Pos_A).normalized * 1;
                Pos_C = Pos_B + (Pos_C - Pos_B).normalized * 1;


                if ((Pos_C - Target.position).sqrMagnitude < 0.001f * 0.001f)
                    break;
            }
        }

        Pos_B = PoleVector(Pos_B, Pos_A, Pos_C);
        
        A_Joint.LookAt(B_Joint);
        B_Joint.LookAt(C_Joint);
        C_Joint.rotation = Target.rotation;

        B_Joint.position = Pos_B;
        C_Joint.position = Pos_C; 
        
    }
    

}
