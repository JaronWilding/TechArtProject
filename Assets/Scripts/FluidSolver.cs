using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityScript.Lang;

public class FluidSolver : MonoBehaviour
{
    private static int N = 256;
    private static int scale = 4;
    private static int iter = 4;
    
    private FluidCube fluid;
    public RenderTexture rt;
    public Texture2D texture;
    Renderer rend;
    private Grid grid;

    private Vector2 mousePos = new Vector2();
    private Vector2 mousePos0 = new Vector2();

    private void Start()
    {
        fluid = FluidCubeCreate(0, 0, 0.1f);
        rt = new RenderTexture(N, N, 1);
        rt.enableRandomWrite = true;
        rt.Create();

        rend = GetComponent<Renderer>();
        rend.enabled = true;

        texture = new Texture2D(rt.width, rt.height);
        rend.material.mainTexture = texture;
        grid = GetComponent<Grid>();
    }

    private void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        // get the collision point of the ray with the z = 0 plane
        Vector3 worldPoint = ray.GetPoint(-ray.origin.z / ray.direction.z);

        mousePos.x = Remap(-worldPoint.x, -5f, 5f, 0f, 255f);
        mousePos.y = Remap(-worldPoint.y, -5f, 5f, 0f, 255f);

        if (Input.GetMouseButton(0))
        {

            
                AddDensity((int)mousePos.x, (int)mousePos.y, 1f);
                float amtX = mousePos.x - mousePos0.x;
                float amtY = mousePos.y - mousePos0.y;
                AddVelocity((int)mousePos.x, (int)mousePos.y, amtX, amtY);


            
        }
        
        FluidCubeStep(fluid);
        Render();
        //rend.material.SetTexture("_MainTex", rt);
        mousePos0 = mousePos;
    }

    public float Remap(float value, float initialMin, float initialMax, float endMin, float endMax)
    {
        return (value - initialMin) / (initialMax - initialMin) * (endMax - endMin) + initialMax;
    }

    public void Render()
    {
        RenderTexture.active = rt;
        texture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        for (int ii = 0; ii < rt.width; ii++)
        {
            for (int jj = 0; jj < rt.height; jj++)
            {
                float d = fluid.density[ArrayIndex(ii, jj)];
                texture.SetPixel(ii, jj, new Color(d, d, d));
            }
        }
        texture.Apply();
    }


    public void AddDensity(int x, int y, float amount)
    {
        int index = ArrayIndex(x, y);
        fluid.density[index] += amount;
    }

    public void AddVelocity(int x, int y, float amountX, float amountY)
    {
        int index = ArrayIndex(x, y);
        fluid.Vx[index] += amountX;
        fluid.Vy[index] += amountY;
    }


    public struct FluidCube
        {
            public int size;
            public float dt;
            public float diff;
            public float visc;

            public float[] s;
            public float[] density;

            public float[] Vx;
            public float[] Vy;

            public float[] Vx0;
            public float[] Vy0;
        };




        public FluidCube FluidCubeCreate(int diffusion, int viscosity, float dt)
        {
            FluidCube cube = new FluidCube();

            cube.size = N;
            cube.dt = dt;
            cube.diff = diffusion;
            cube.visc = viscosity;

            cube.s = new float[N * N];
            cube.density = new float[N * N];

            cube.Vx = new float[N * N];
            cube.Vy = new float[N * N];

            cube.Vx0 = new float[N * N];
            cube.Vy0 = new float[N * N];

            return cube;
        }

        void FluidCubeStep(FluidCube cube)
        {
            float visc = cube.visc;
            float diff = cube.diff;
            float dt = cube.dt;
            float[] Vx = cube.Vx;
            float[] Vy = cube.Vy;
            float[] Vx0 = cube.Vx0;
            float[] Vy0 = cube.Vy0;
            float[] s = cube.s;
            float[] density = cube.density;

            Diffuse(1, Vx0, Vx, visc, dt);
            Diffuse(2, Vy0, Vy, visc, dt);

            Projection(Vx0, Vy0, Vx, Vy);

            Advect(1, Vx, Vx0, Vx0, Vy0, dt);
            Advect(2, Vy, Vy0, Vx0, Vy0, dt);

            Projection(Vx, Vy, Vx0, Vy0);

            Diffuse(0, s, density, diff, dt);
            Advect(0, density, s, Vx, Vy, dt);
        }

        void FluidCubeAddDensity(FluidCube cube, int x, int y, float amount)
        {
            N = cube.size;
            cube.density[ArrayIndex(x, y)] += amount;
        }

        public static int ArrayIndex(int x, int y)
        {
            x = Mathf.Clamp(x, 0, N - 1);
            y = Mathf.Clamp(y, 0, N - 1);
            return x + (y * N);
        }

        void FluidCubeAddVelocity(FluidCube cube, int x, int y, float amountX, float amountY)
        {
            int index = ArrayIndex(x, y);

            cube.Vx[index] += amountX;
            cube.Vy[index] += amountY;
        }

        void Diffuse(int b, float[] x, float[] x0, float diff, float dt)
        {
            float a = dt * diff * (N - 2) * (N - 2);
            LinearSolve(b, x, x0, a, 1 + 6 * a);
        }

        static void LinearSolve(int b, float[] x, float[] x0, float a, float c)
        {
            float cRecip = 1.0f / c;
            for (int k = 0; k < iter; k++)
            {

                for (int j = 1; j < N - 1; j++)
                {
                    for (int i = 1; i < N - 1; i++)
                    {
                        x[ArrayIndex(i, j)] =
                            (x0[ArrayIndex(i, j)]
                             + a * (x[ArrayIndex(i + 1, j)]
                                    + x[ArrayIndex(i - 1, j)]
                                    + x[ArrayIndex(i, j + 1)]
                                    + x[ArrayIndex(i, j - 1)]
                             )) * cRecip;
                    }
                }

                SetBound(b, x);
            }
        }

        static void Projection(float[] velocX, float[] velocY, float[] p, float[] div)
        {

            for (int j = 1; j < N - 1; j++)
            {
                for (int i = 1; i < N - 1; i++)
                {
                    div[ArrayIndex(i, j)] = -0.5f * (
                                           velocX[ArrayIndex(i + 1, j)]
                                           - velocX[ArrayIndex(i - 1, j)]
                                           + velocY[ArrayIndex(i, j + 1)]
                                           - velocY[ArrayIndex(i, j - 1)]
                                       ) / N;
                    p[ArrayIndex(i, j)] = 0;
                }
            }
            SetBound(0, div);
            SetBound(0, p);
            LinearSolve(0, p, div, 1, 6);

            for (int j = 1; j < N - 1; j++)
            {
                for (int i = 1; i < N - 1; i++)
                {
                    velocX[ArrayIndex(i, j)] -= 0.5f * (p[ArrayIndex(i + 1, j)]
                                                   - p[ArrayIndex(i - 1, j)]) * N;
                    velocY[ArrayIndex(i, j)] -= 0.5f * (p[ArrayIndex(i, j + 1)]
                                                   - p[ArrayIndex(i, j - 1)]) * N;
                }
            }
            SetBound(1, velocX);
            SetBound(2, velocY);
        }


        static void Advect(int b, float[] d, float[] d0, float[] velocX, float[] velocY, float dt)
        {
            float i0, i1, j0, j1;

            float dtx = dt * (N - 2);
            float dty = dt * (N - 2);

            float s0, s1, t0, t1;
            float tmp1, tmp2, x, y;

            float Nfloat = N;
            float ifloat, jfloat;
            int i, j;

            for (j = 1, jfloat = 1; j < N - 1; j++, jfloat++)
            {
                for (i = 1, ifloat = 1; i < N - 1; i++, ifloat++)
                {
                    tmp1 = dtx * velocX[ArrayIndex(i, j)];
                    tmp2 = dty * velocY[ArrayIndex(i, j)];
                    x = ifloat - tmp1;
                    y = jfloat - tmp2;

                    if (x < 0.5f) x = 0.5f;
                    if (x > Nfloat + 0.5f) x = Nfloat + 0.5f;
                    i0 = Mathf.Floor(x);
                    i1 = i0 + 1.0f;
                    if (y < 0.5f) y = 0.5f;
                    if (y > Nfloat + 0.5f) y = Nfloat + 0.5f;
                    j0 = Mathf.Floor(y);
                    j1 = j0 + 1.0f;

                    s1 = x - i0;
                    s0 = 1.0f - s1;
                    t1 = y - j0;
                    t0 = 1.0f - t1;

                    int i0i = (int)i0;
                    int i1i = (int)i1;
                    int j0i = (int)j0;
                    int j1i = (int)j1;


                    // TO-DO

                    d[ArrayIndex(i, j)] =
                        s0 * (t0 * d0[ArrayIndex(i0i, j0i)] + t1 * d0[ArrayIndex(i0i, j1i)]) +
                        s1 * (t0 * d0[ArrayIndex(i1i, j0i)] + t1 * d0[ArrayIndex(i1i, j1i)]);
                }
            }
            SetBound(b, d);
        }



        static void SetBound(int b, float[] x)
        {
            for (int i = 1; i < N - 1; i++)
            {
                x[ArrayIndex(i, 0)] = b == 2 ? -x[ArrayIndex(i, 1)] : x[ArrayIndex(i, 1)];
                x[ArrayIndex(i, N - 1)] = b == 2 ? -x[ArrayIndex(i, N - 2)] : x[ArrayIndex(i, N - 2)];
            }
            for (int j = 1; j < N - 1; j++)
            {
                x[ArrayIndex(0, j)] = b == 1 ? -x[ArrayIndex(1, j)] : x[ArrayIndex(1, j)];
                x[ArrayIndex(N - 1, j)] = b == 1 ? -x[ArrayIndex(N - 2, j)] : x[ArrayIndex(N - 2, j)];
            }


            x[ArrayIndex(0, 0)] = 0.33f * (x[ArrayIndex(1, 0)]
                                          + x[ArrayIndex(0, 1)]);
            x[ArrayIndex(0, N - 1)] = 0.33f * (x[ArrayIndex(1, N - 1)]
                                          + x[ArrayIndex(0, N - 2)]);
            x[ArrayIndex(N - 1, 0)] = 0.33f * (x[ArrayIndex(N - 2, 0)]
                                          + x[ArrayIndex(N - 1, 1)]);
            x[ArrayIndex(N - 1, N - 1)] = 0.33f * (x[ArrayIndex(N - 2, N - 1)]
                                          + x[ArrayIndex(N - 1, N - 2)]);
        }
    

}
