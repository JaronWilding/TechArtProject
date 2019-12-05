using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKScript : MonoBehaviour
{
    [SerializeField] private Transform _endLocator;
    [SerializeField] private Transform _targetLocator;
    [SerializeField] private List<GameObject> allObjs;
    [SerializeField] private Material lineMat;


    private Segment tentacle;
    void Start()
    {
        /*
        allObjs = new List<GameObject>();

        Transform currentObj = _endLocator;
        while (currentObj != transform)
        {
            allObjs.Add(currentObj.gameObject);
            currentObj = currentObj.parent;
        }
        allObjs.Add(gameObject);
        allObjs.Reverse();
        */

        tentacle = new Segment(0f, 0f, 0.2f, Mathf.Deg2Rad * 0f, newSeg("seg_1"));

        Segment current = tentacle;
        for (int ii = 0; ii < 20; ii++)
        {
            Segment next = new Segment(current, 0.2f, Mathf.Deg2Rad * 0f, newSeg(string.Format("seg_{0}", ii+1)));
            current.child = next;
            current = next;
        }

    }

    

    void Update()
    {
        //SetLineRenderer();
        //SetIK();
        Segment next = tentacle;
        while(next != null)
        {
            next.Wiggle();
            next.updateSegments();
            next.Show();
            next = next.child;
        }

        
    }


    private void SetIK()
    {
        for (int ii = 0; ii < allObjs.Count - 1; ii++)
        {
            GameObject currentObj = allObjs[ii];
            GameObject childObj = allObjs[ii + 1];

            Vector3 _startLoc = currentObj.transform.position;
            Vector3 _endLoc = childObj.transform.position;
            float _len;
            float _angle;

        }
    }

    private LineRenderer newSeg(string name)
    {
        GameObject newLine = new GameObject();
        newLine.name = name;
        LineRenderer _line = newLine.AddComponent<LineRenderer>();
        _line.material = new Material(lineMat);
        _line.startWidth = 0.02f;
        _line.endWidth = 0.02f;
        return _line;
    }

    private void SetLineRenderer()
    {
        for (int ii = 0; ii < allObjs.Count - 1; ii++)
        {
            GameObject currentObj = allObjs[ii];
            GameObject childObj = allObjs[ii + 1];

            Vector3[] pos = new[] { currentObj.transform.position, childObj.transform.position };
            currentObj.GetComponent<LineRenderer>().SetPositions(pos);
        }
    }


    public class Segment
    {
        Vector3 a;
        Vector3 b;
        float len;
        float angle;
        float selfAngle;

        float xoff = Random.Range(0f, 1000f);

        LineRenderer line;
        public Segment parent = null;
        public Segment child = null;



        public Segment(float _x, float _y, float _len, float _angle, LineRenderer _line)
        {
            a = new Vector3(_x, 0f, _y);
            len = _len;
            angle = _angle;
            line = _line;
            calculateB();
            parent = null;
        }

        public Segment(Segment _parent, float _len, float _angle, LineRenderer _line)
        {
            parent = _parent;
            a = parent.b;
            len = _len;
            angle = _angle;
            selfAngle = angle;
            line = _line;
            calculateB();
        }

        public void Wiggle()
        {
            float maxAngle =  1f;
            float minAngle =  -1f;

            selfAngle = Mathf.PerlinNoise(xoff, 0f).Range01(minAngle, maxAngle);
            xoff += 0.01f;
            //selfAngle = selfAngle +  1 * Mathf.Deg2Rad;
        }

        public void updateSegments()
        {
            angle = selfAngle;
            if (parent != null)
            {
                a = parent.b;
                angle += parent.angle;
            } else
            {
                angle += -Mathf.PI / 2;
            }
            calculateB();
        }

        public void calculateB()
        {
            float dx = len * Mathf.Cos(angle);
            float dy = len * Mathf.Sin(angle);

            b = new Vector3(a.x + dx, 0f, a.z + dy);
        }

        public void Show()
        {
            line.SetPositions( new[] { a, b });
        }

    }




}

public static class ExtensionMethods
{
    public static float Range(this float _input, float _inMin, float _inMax, float _outMin, float _outMax)
    {
        return (_input - _inMin) / (_inMax - _inMin) * (_outMin - _outMax) + _outMax;
    }
    public static float Range01(this float _input, float _outMin, float _outMax)
    {
        return (_input - 0f) / (1f - 0f) * (_outMin - _outMax) + _outMax;
    }
    public static float Range11(this float _input, float _outMin, float _outMax)
    {
        return (_input - -1f) / (1f - -1f) * (_outMin - _outMax) + _outMax;
    }
}
