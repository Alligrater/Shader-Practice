using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SHCompute : MonoBehaviour
{

    //I actually have 0 idea what to do
    //however, I do know that I need a cube map of some sort.
    //I need to know how to create a cube map.(this specific line is generated by copilot)

    [SerializeField] private Texture2D cubeMap;
    [SerializeField] private Texture2D tex;

    //And then, I need to have a way of sampling a hemisphere.
    //I need to know how to sample a hemisphere.(this specific line is generated by copilot)
    /*
    private void SampleHemisphere(){
        //two random numbers:
        //one for theta, one for phi
        float theta = Random.Range(0, Mathf.PI * 2);
        float phi = Random.Range(0, Mathf.PI);
    }*/

    //and I have the tool
    private Vector3 SampleSphere(){
        //this one is not uniform
        Vector3 direction = Random.onUnitSphere;
        //using the direction, compute theta and phi.
        return direction;
    }

    private Vector3 SampleHemisphere(Vector3 up){
        Vector3 direction = Random.onUnitSphere;
        if(Vector3.Dot(direction, up) < 0.0f){
            direction = -direction;
        }
        return direction;
    }

    //private Vector3 UniformSampleSphere(){

    //}

    // Start is called before the first frame update
    void Start()
    {
        
    }

    [ContextMenu("Sample Cube Map with Random Points")]
    public void SampleCubeMap(){
        Random.InitState(0);
        tex = new Texture2D(cubeMap.width, cubeMap.height, TextureFormat.ARGB32, false);
        //for each pixel up there, 
        for(int x = 0; x < tex.width; x++){
            for(int y = 0; y < tex.height; y++){
                //compute theta and phi from texture's uv coordinate:
                float theta = Mathf.PI * 2 * (x / (float)tex.width - 0.5f);
                float phi = Mathf.PI * ((y / (float)tex.height) - 0.5f);
                //convert this to a vector:
                float cosTheta = Mathf.Cos(theta);
                float sinTheta = Mathf.Sin(theta);

                float cosPhi = Mathf.Cos(phi);
                float sinPhi = Mathf.Sin(phi);

                Vector3 direction = new Vector3(
                    cosTheta * cosPhi,//for x, its cos theta
                    sinPhi,
                    sinTheta * cosPhi
                );

                Color c = SampleColorsOnCubemap(direction);
                //set the color to this average:
                tex.SetPixel(x, y, c);
            }
        }
        tex.Apply();

    }

    public Vector2 DirToUV(Vector3 dir){
        float u = Mathf.Atan2(dir.z, dir.x); // this has a range of -pi to pi
        
        u = u / Mathf.PI; //this remaps to -1 to 1
        u = u * 0.5f + 0.5f; //and that maps back to 0 to 1.

        float v = dir.y;//this has a range of -1 to 1 
        v = v * 0.5f + 0.5f; //remaps to 0 to 1
        //distortions happen near polar points
        //Debug.Log(u);
        Debug.Log(v);
        return new Vector2(u, v);
    }

    //Just start with the dumbest of the dumbest method, and then we can be smart later on.
    public Color SampleColorsOnCubemap(Vector3 invec){
        Color averageColor = new Color(0, 0, 0);
        //for(int i = 0; i < 1000; i++){
            //Vector3 dir = SampleHemisphere(invec);
            Vector2 uv = DirToUV(invec);
            int x = Mathf.FloorToInt(uv.x * cubeMap.width);
            int y = Mathf.FloorToInt(uv.y * cubeMap.height);
            //Debug.Log(x);
            //sample the texture
            averageColor += cubeMap.GetPixel(x, y);// * 0.001f;
        //}
        //Debug.Log(averageColor);
        return averageColor;
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
