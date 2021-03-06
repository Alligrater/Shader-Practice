using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class DepthReconstruct : MonoBehaviour
{
    [SerializeField] private Material postProcess;
    private Camera targetCamera;

    private Transform m_targetCameraTransform;
    public Transform p_targetCameraTransform {
        get {
            if(m_targetCameraTransform == null){
                m_targetCameraTransform = transform;
            }
            return m_targetCameraTransform;
        }
    }

    [SerializeField] private float fogStart;
    [SerializeField] private float fogEnd;
    [Range(0, 1)][SerializeField] private float fogDensity;
    [SerializeField] private Color fogColor;

    void Awake(){
        targetCamera = GetComponent<Camera>();
    }
    void OnRenderImage(RenderTexture src, RenderTexture dest){
        if(postProcess != null){
            float near = targetCamera.nearClipPlane;
            float far = targetCamera.farClipPlane;
            float fov = targetCamera.fieldOfView;
            float aspect = targetCamera.aspect;

            float halfHeight = near * Mathf.Tan(fov * 0.5f * Mathf.Deg2Rad);
            //should compute a ray...

            //using this, we can compute screen width....
            float screenHalfWidth = halfHeight * aspect;
            Vector3 forwardDir = transform.forward * near;
            Vector3 upDir = transform.up * halfHeight;
            Vector3 rightDir = transform.right * screenHalfWidth;
            Vector3 rayCoordsTL = forwardDir + upDir - rightDir;
            float scale = rayCoordsTL.magnitude / near;
            rayCoordsTL = rayCoordsTL.normalized * scale;
            
            Vector3 rayCoordsBR = (forwardDir - upDir + rightDir).normalized * scale;
            Vector3 rayCoordsBL = (forwardDir - upDir - rightDir).normalized * scale;
            Vector3 rayCoordsTR = (forwardDir + upDir + rightDir).normalized * scale;

            Matrix4x4 frustumCorners = Matrix4x4.identity;
            frustumCorners.SetRow(0, rayCoordsBL);
            frustumCorners.SetRow(1, rayCoordsBR);
            frustumCorners.SetRow(2, rayCoordsTR);
            frustumCorners.SetRow(3, rayCoordsTL);
            //postProcess.SetFloat("_Near", targetCamera.nearClipPlane);
            //postProcess.SetFloat("_Far", targetCamera.farClipPlane);
            postProcess.SetMatrix("_FrustumCornersRay", frustumCorners);//and then just let the vertex shader interpolate
            postProcess.SetColor("_FogColor", fogColor);
            postProcess.SetFloat("_FogDensity", fogDensity);
            postProcess.SetFloat("_FogStart", fogStart);
            postProcess.SetFloat("_FogEnd", fogEnd);
            Graphics.Blit(src, dest, postProcess);
        }
        else{
            Graphics.Blit(src, dest);
        }
    }
}
