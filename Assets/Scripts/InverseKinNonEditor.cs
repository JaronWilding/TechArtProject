using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InverseKinNonEditor : MonoBehaviour
{

    enum IKSystems
    {
        CustomIK,
        FabricIK,
        CCD_IK
    }

    [Header("Solver Settings")]
    [SerializeField] public int iterations = 20;
    [SerializeField] public Transform Target;
    [SerializeField] public Transform Pole;
    [SerializeField] public float DistError = 0.01f;
    [Range(0f, 1f)][SerializeField] public float weight = 1f;
    //[SerializeField] public bool IKSystems = false;
    [SerializeField] IKSystems ikSystems;
    [SerializeField] public Vector3 Trans;

    [Header("Joint Settings")]
    [SerializeField] public Transform A_Joint;
    [SerializeField] public Transform B_Joint;
    [SerializeField] public Transform C_Joint;

    List<Transform> ccd_Bones;

    private void Start()
    {
        ccd_Bones = new List<Transform>();
        Transform current = C_Joint;

        while(current != null && current != A_Joint.parent)
        {
            ccd_Bones.Add(current);
            current = current.parent;
        }
        if (current == null)
            throw new UnityException("IK failing");
        
    }

    void LateUpdate()
    {
        if (ikSystems == IKSystems.CustomIK)
            IKSystemCustom();
        else if (ikSystems == IKSystems.FabricIK)
            IKSystemFabrick();
        else if (ikSystems == IKSystems.CCD_IK)
            CCD_IK();
    }

    private void IKSystemCustom()
    {
        for (int i = 0; i < iterations; i++)
        {
            float arm_length_a = Vector3.Distance(A_Joint.position, B_Joint.position);
            float arm_length_b = Vector3.Distance(B_Joint.position, C_Joint.position);

            Vector3 goal = Vector3.Lerp(C_Joint.position, Target.position, weight);

            Vector3 jnt_B_rot = FollowMouse(B_Joint.position, goal, arm_length_b);

            Vector3 jnt_B_pos = goal + (jnt_B_rot * -1);
            Vector3 jnt_A_rot = FollowMouse(A_Joint.position, jnt_B_pos, arm_length_a);

            Vector3 up = Vector3.up;
            Vector3 rotate = new Vector3(0f, 0f, 0f);

            rotate = PoleVector(B_Joint.position, A_Joint.position, C_Joint.position);
            
            A_Joint.rotation = Quaternion.LookRotation((jnt_A_rot + rotate) * Mathf.Rad2Deg, up);
            B_Joint.rotation = Quaternion.LookRotation((jnt_B_rot) * Mathf.Rad2Deg, up);
            
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

    private void CCD_IK()
    {
        Vector3 goalPos = Target.position;
        Vector3 EffPos = ccd_Bones[0].position;

        Vector3 targetPos = Vector3.Lerp(EffPos, goalPos, weight);
        float sqrDist;

        int interationCount = 0;
        do
        {
            for (int i = 0; i < ccd_Bones.Count - 2; i++)
            {
                for (int j = 1; j < i + 3 && j < ccd_Bones.Count; j++)
                {
                    RotateBone(ccd_Bones[0], ccd_Bones[j], targetPos);

                    sqrDist = (ccd_Bones[0].position - targetPos).sqrMagnitude;
                    if (sqrDist <= DistError)
                        return;
                }
            }



            sqrDist = (ccd_Bones[0].position - targetPos).sqrMagnitude;
            interationCount++;
        }
        while (sqrDist > DistError && interationCount <= iterations);

    }
   

    private void RotateBone(Transform effector, Transform bone, Vector3 goalPos)
    {
        Vector3 effPos = effector.position;
        Vector3 bonePos = bone.position;
        Quaternion boneRot = bone.rotation;

        Vector3 boneToEff = effPos - bonePos;
        Vector3 boneToGoal = goalPos - bonePos;
        

        Quaternion fromToRot = Quaternion.FromToRotation(boneToEff, boneToGoal);
        Quaternion newRot = fromToRot * boneRot;
        

        bone.rotation = newRot;

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
