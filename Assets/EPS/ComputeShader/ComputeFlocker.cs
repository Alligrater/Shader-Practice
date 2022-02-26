using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComputeFlocker : MonoBehaviour
{

    struct BoidData{
        public Vector3 position;
        public Vector3 velocity;
        public Vector3 color;
    };

    [SerializeField, Range(10, 200)] int resolution = 32;
    [SerializeField] int numFish = 1024; //this should be more than enough?
    [SerializeField] float maxSpeed = 0.3f;
    [SerializeField] ComputeShader computeShader;
    [SerializeField] Mesh mesh;
    [SerializeField] Material material;
    ComputeBuffer boidBuffer;
    ComputeBuffer outputDataBuffer;

    //public RenderTexture rt;
    // Start is called before the first frame update
    void OnEnable()
    {
        //                                                  float3 position, float3 vector, float3 accleration
        boidBuffer = new ComputeBuffer(resolution * resolution, sizeof(float) * 3 * 3);
        //                                                            float3 position
        outputDataBuffer = new ComputeBuffer(resolution * resolution, sizeof(float) * 3);
        InitializeBoids();
    }

    void UpdateFunctionOnGPU(){
        //draw stuff
        computeShader.SetBuffer(0, "_Boids", boidBuffer);
        computeShader.SetBuffer(0, "_Output", outputDataBuffer);
        computeShader.SetFloat("_TimeStep", Time.deltaTime);
        computeShader.SetVector("_Resolution", new Vector4(resolution, 0, 0, 0));
        int groups = Mathf.CeilToInt(resolution / 8f);
        computeShader.Dispatch(0, groups * groups, 1, 1);

        material.SetBuffer("_Boids", outputDataBuffer);
        var bounds = new Bounds(Vector3.zero, Vector3.one * 256);
        Graphics.DrawMeshInstancedProcedural(mesh, 0, material, bounds, numFish);
    }

    void OnDisable(){
        boidBuffer.Release();
        boidBuffer = null;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateFunctionOnGPU();
    }

    void InitializeBoids(){
        BoidData[] initValue = new BoidData[numFish];
        for(int i = 0; i < numFish; i++){
            initValue[i].color = new Vector3(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f));
            initValue[i].position = new Vector3(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f)) * 5;
            initValue[i].velocity = Random.insideUnitSphere * maxSpeed;
        }
        boidBuffer.SetData(initValue);
        //boidBuffer.SetCounterValue()
    }
}
