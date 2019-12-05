using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSys_S : MonoBehaviour
{
    public Material _mat;
    public int _width = 512;
    public int _height = 512;
    public int _count = 100;

    public RenderTexture _result;
    public ComputeShader _compute;

    struct Point
    {
        public Vector3 pos;
        public Vector3 vel;
    }

    public void Start()
    {
        _result = new RenderTexture(_width, _height, 24);
        _result.enableRandomWrite = true;
        _result.Create();
    }

    public void Update()
    {
        if (_height < 1 || _width < 1) return;

        int kernel = _compute.FindKernel("StartSim");

        
        _compute.SetFloat("Width", _width);
        _compute.SetFloat("Height", _height);

        for(int ii = 0; ii < _count; ii++)
        {

        }

        _result = new RenderTexture(_width, _height, 24);
        _result.wrapMode = TextureWrapMode.Repeat;
        _result.enableRandomWrite = true;
        _result.filterMode = FilterMode.Point;
        _result.useMipMap = false;
        _result.Create();

        _compute.SetTexture(kernel, "Result", _result);
        _compute.Dispatch(kernel, _width / 8, _height / 8, 1);
        

    }
}
