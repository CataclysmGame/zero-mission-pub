using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using UnityEngine;

public class TestMint : MonoBehaviour
{
    // Start is called before the first frame update
    async void Start()
    {
        using (var httpClient = new HttpClient())
        {
            using (var request = new HttpRequestMessage(new HttpMethod("POST"), "http://localhost:8000/evm/createMintNFTTransaction"))
            {
                var contentList = new List<string>();
                contentList.Add($"chain={Uri.EscapeDataString("ethereum")}");
                contentList.Add($"network={Uri.EscapeDataString("goerli")}");
                contentList.Add($"account={Uri.EscapeDataString("0xd25b827D92b0fd656A1c829933e9b0b836d5C3e2")}");
                contentList.Add($"to={Uri.EscapeDataString("0xa270a31815C47391770020eC20Fb7E02598d959f")}");
                contentList.Add(Uri.EscapeDataString("QmbnT8LsBCShSaeSSXyrmX1rHdWZdbj45Whyioomqekwr4"));
                request.Content = new StringContent(string.Join("&", contentList));
                request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");

                var response = await httpClient.SendAsync(request);
                print("Response: " + response);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
