using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingPrefabScreenshotTool : MonoBehaviour
{
    public Transform stage;
    public int glowBorderPixelSize = 1;
    public Color borderColor = Color.white;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(TakeScreenshot());
    }

    /// <summary>
    /// Takes a screenshot of each object in the stage and saves it to the Assets/Screenshots folder
    /// </summary>
    /// <returns></returns>
    public IEnumerator TakeScreenshot()
    {
        for (int i = 0; i < stage.childCount; i++)
        {
            stage.GetChild(i).gameObject.SetActive(false);
        }

        yield return new WaitForSeconds(1); // Wait for Instantion of all objects

        // Take screenshot of each Object in the Stage
        for(int i = 0; i<stage.childCount; i++)
        {
            GameObject child = stage.GetChild(i).gameObject;
            child.SetActive(true);
            yield return new WaitForEndOfFrame();
            Texture2D tex = ScreenCapture.CaptureScreenshotAsTexture();

            // Remove background
            for (int x = 0; x < tex.width; x++)
            {
                for (int y = 0; y < tex.height; y++)
                {
                    Color c = tex.GetPixel(x, y);
                    if (c.g >= 1f)
                    {
                        c = new Color(0, 0, 0, 0);
                        tex.SetPixel(x, y, c);
                    }
                }
            }

            // AddBorder(tex);


            // Save to file
            byte[] bytes = tex.EncodeToPNG();
            string filename = child.name + ".png";
            string path = Application.dataPath + "/Screenshots/" + filename;

            // Make Sure the Directory exists
            if (!System.IO.Directory.Exists(Application.dataPath + "/Screenshots/"))
            {
                System.IO.Directory.CreateDirectory(Application.dataPath + "/Screenshots/");
            }

            System.IO.File.WriteAllBytes(path, bytes);
            child.SetActive(false);
        }

        Debug.Log($"Screenshots saved in {Application.dataPath + "/Screenshots/"}");

        // End Playmode
        UnityEditor.EditorApplication.isPlaying = false;
    }

    private void AddBorder(Texture2D tex)
    {
        List<Vector2Int> borderPixel = new List<Vector2Int>();
        // Get Border Pixel
        for (int x = 0; x < tex.width; x++)
        {
            for (int y = 0; y < tex.height; y++)
            {
                Color c = tex.GetPixel(x, y);
                if (c.g >= 0.9f)
                {
                    borderPixel.Add(new Vector2Int(x, y));
                }
            }
        }

        // Set Border
        foreach (Vector2Int pixel in borderPixel)
        {
            int x = pixel.x;
            int y = pixel.y;
            Color c = tex.GetPixel(x, y);

            for (int j = 0; j < glowBorderPixelSize; j++)
            {
                if (x + j < tex.width)
                {
                    tex.SetPixel(x + j, y, borderColor);
                }
                if (x - j >= 0)
                {
                    tex.SetPixel(x - j, y, borderColor);
                }
                if (y + j < tex.height)
                {
                    tex.SetPixel(x, y + j, borderColor);
                }
                if (y - j >= 0)
                {
                    tex.SetPixel(x, y - j, borderColor);
                }
            }

        }
    }
}
