using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

[ExecuteInEditMode]
public class MultiInverse : MonoBehaviour
{

    [Header("Solver Parameters")]
    public int Iterations = 20;
    public float Delta = 0.001f;
    [Range(0, 1)] public float SnapBackStrenght = 1f;
    public Transform Target;
    public Transform Pole;
    public int ChainLength = 4;


    protected static float[] BonesLength;
    protected static float CompleteLength;
    protected static Transform[] Bones;
    protected static Vector3[] Positions;

    private Vector3[] StartDirectionSucc;
    private  Quaternion[] StartRotationBone;
    private Quaternion StartRotationTarget;
    private Quaternion StartRotationRoot;


    private void Update()
    {
        Init();
        
    }
    void Init()
    {
        Bones = new Transform[ChainLength + 1];
        Positions = new Vector3[ChainLength + 1];
        BonesLength = new float[ChainLength];
        StartDirectionSucc = new Vector3[ChainLength + 1];
        StartRotationBone = new Quaternion[ChainLength + 1];

        StartRotationTarget = Target.rotation;
        CompleteLength = 0;

        Transform current = this.transform;
        for( int i = Bones.Length-1; i>= 0; i--)
        {
            Bones[i] = current;
            StartRotationBone[i] = current.rotation;

            if(i == Bones.Length -1)
            {
                StartDirectionSucc[i] = Target.position - current.position;
            }
            else
            {
                StartDirectionSucc[i] = Bones[i + 1].position - current.position;
                BonesLength[i] = StartDirectionSucc[i].magnitude;
                CompleteLength += BonesLength[i];
            }

            current = current.parent;
        }
    }

    private void LateUpdate()
    {
        ResolveIK();
    }

    private void ResolveIK()
    {
        if (Target == null)
            return;

        if (BonesLength.Length != ChainLength)
            Init();


        //Get positions
        for (int i = 0; i < Bones.Length; i++)
           Positions[i] = Bones[i].position;

        var RootRot = (Bones[0].parent != null) ? Bones[0].parent.rotation : Quaternion.identity;
        var RootRotDiff = RootRot * Quaternion.Inverse(StartRotationRoot);

        //Calculation
        //Check if target is far away
        if((Target.position - Bones[0].position).sqrMagnitude >= CompleteLength * CompleteLength)
        {
            //Strecthc
            Vector3 direction = (Target.position - Positions[0]).normalized;

            for (int i = 1; i < Positions.Length; i++)
                Positions[i] = Positions[i - 1] + direction * BonesLength[i - 1];
        }
        else
        {
            for (int iteration = 0; iteration < Iterations; iteration++)
            {
                //Back
                for (int i = Positions.Length-1; i > 0; i--)
                {
                    if (i == Positions.Length - 1)
                        Positions[i] = Target.position; //Set to target
                    else
                        Positions[i] = Positions[i + 1] + ((Positions[i] - Positions[i + 1]).normalized * BonesLength[i]); //Set in line on Distance
                }
                //Forward
                for (int i = 1; i < Positions.Length; i++)
                {
                    Positions[i] = Positions[i - 1] + ((Positions[i] - Positions[i - 1]).normalized * BonesLength[i - 1]); //Set in line on Distance
                }
                

                if ((Positions[Positions.Length - 1] - Target.position).sqrMagnitude < Delta * Delta)
                    break;
            }
        }

        if (Pole != null)
        {
            for (int i = 1; i < Positions.Length - 1; i++)
            {
                Plane plane = new Plane(Positions[i + 1] - Positions[i - 1], Positions[i - 1]);
                Vector3 projectedPole = plane.ClosestPointOnPlane(Pole.position);
                Vector3 projectedBone = plane.ClosestPointOnPlane(Positions[i]);
                float angle = Vector3.SignedAngle(projectedBone - Positions[i - 1], projectedPole - Positions[i - 1], plane.normal);
                Positions[i] = Quaternion.AngleAxis(angle, plane.normal) * (Positions[i] - Positions[i - 1]) + Positions[i - 1];
            }
        }

        //Set positions
        for (int i = 0; i < Positions.Length; i++)
        {
            if (i == Positions.Length - 1)
                Bones[i].rotation = Target.rotation * Quaternion.Inverse(StartRotationTarget) * StartRotationBone[i];
            else
                Bones[i].rotation = Quaternion.FromToRotation(StartDirectionSucc[i], Positions[i + 1] - Positions[i]) * StartRotationBone[i];

            Bones[i].position = Positions[i];
        }
    }

}