using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrtPostProcess : MonoBehaviour
{
    public Shader shader;

    [Header("Bending")]
    public float bend = 4.0f;

    [Header("Scanlines")]
    public float scanLineSize1 = 200.0f;
    public float scanLineSpeed1 = -10.0f;
    public float scanLineSize2 = 20.0f;
    public float scanLineSpeed2 = -3.0f;
    public float scanlineAmount = 0.05f;

    [Header("Vignette")]
    public float vignetteSize = 1.9f;
    public float vignetteSmoothness = 0.6f;
    public float vignetteEdgeRound = 8.0f;

    [Header("Noise")]
    public float noiseSize = 75.0f;
    public float noiseAmount = 0.05f;

    [Header("Cromatic Aberration")]
    public Vector2 redOffset = new Vector2(0, -0.01f);
    public Vector2 greenOffset = new Vector2(0, 0.01f);
    public Vector2 blueOffset = Vector2.zero;

    private Material material;

    private void Start()
    {
        material = new Material(shader);
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (!Settings.Instance.CrtFilterEnabled)
        {
            Graphics.Blit(source, destination);
            return;
        }

        material.SetFloat("u_time", Time.fixedTime);
        material.SetFloat("u_bend", bend);

        material.SetFloat("u_vignette_size", vignetteSize);
        material.SetFloat("u_vignette_smoothness", vignetteSmoothness);
        material.SetFloat("u_vignette_edge_round", vignetteEdgeRound);

        material.SetFloat("u_scanline_size_1", scanLineSize1);
        material.SetFloat("u_scanline_size_2", scanLineSize2);
        material.SetFloat("u_scanline_speed_1", scanLineSpeed1);
        material.SetFloat("u_scanline_speed_2", scanLineSpeed2);
        material.SetFloat("u_scanline_amount", scanlineAmount);

        material.SetFloat("u_noise_size", noiseSize);
        material.SetFloat("u_noise_amount", noiseAmount);

        material.SetVector("u_red_offset", redOffset);
        material.SetVector("u_green_offset", greenOffset);
        material.SetVector("u_blue_offset", blueOffset);
        

        Graphics.Blit(source, destination, material);
    }
}
