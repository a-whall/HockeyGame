using System.Collections;
using UnityEngine;
using static System.IO.File;
using static UnityEngine.Application;
using static UnityEngine.Physics;
using static UnityEngine.Mathf;

public class RinkTextureGenerator : MonoBehaviour
{	
    public Transform Rink;

	IEnumerator Start ()
	{
		yield return PNG();
	}

	// Scan the rink with a bunch of raycasts, storing each color in a texture and then saving the texture as a PNG.
	// It takes a minute.
	// aa - antialiasing factor. Determines the sample size of rays to cast per unit of world space. Scales resolution of PNG.
	IEnumerator PNG()
	{
		yield return new WaitForEndOfFrame();
		float aa = 100f;
		float scan_h = 4f;
		float w = Rink.localScale.x*aa;
		float h = Rink.localScale.z*aa;
		float w2 = Rink.localScale.x/2f;
		float h2 = Rink.localScale.z/2f;
		Texture2D texture = new Texture2D((int)w, (int)h, TextureFormat.RGBA32, false);
		for (float i = 0, x = -w2; i < w; x = Lerp(-w2, w2, (++i)/w))
			for (float j = 0, z = -h2; j < h; z = Lerp(-h2, h2, (++j)/h))
				if (Raycast(new Vector3(x, scan_h, z), Vector3.down, out RaycastHit hit))
					texture.SetPixel((int)i, (int)j, hit.collider.GetComponent<Renderer>().sharedMaterial.color, 0);
		WriteAllBytes(dataPath + "/Textures/out.PNG", texture.EncodeToPNG());
		Destroy(texture);
		Debug.Log("Image done");
        yield break;
	}
}