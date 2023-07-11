using UnityEngine;

public class NFTTexture : MonoBehaviour
{
    public Texture2D originalTexture;
    public Texture2D nftTexture;

    private SpriteRenderer spriteRenderer;

    private Shader nftShader;

    void Awake()
    {
        nftShader = Shader.Find("Custom/NFTShader");
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (nftTexture != null)
        {
            SetNFTTexture(nftTexture);
        }
    }

    public void SetNFTTexture(Texture2D nftTexture)
    {
        nftTexture.filterMode = originalTexture.filterMode;

        var nftMaterial = new Material(nftShader);

        spriteRenderer.material = nftMaterial;
        spriteRenderer.material.SetTexture("_MainTex2", nftTexture);
    }
}
