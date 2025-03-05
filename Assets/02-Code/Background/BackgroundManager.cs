using UnityEngine;
using System.Collections;

public class BackgroundManager : MonoBehaviour
{
    public float scrollSpeed = 2.0f;         
    public GameObject backgroundPrefab;      
    public Sprite[] backgroundSprites;       

    private GameObject[] backgrounds = new GameObject[3];
    private float backgroundHeight;         
    private bool isScrolling = true;

    void Start()
    {
        for (int i = 0; i < backgrounds.Length; i++)
        {
            Vector3 pos = new Vector3(0, i * GetBackgroundHeight(), 0);
            backgrounds[i] = Instantiate(backgroundPrefab, pos, Quaternion.identity);

            SpriteRenderer sr = backgrounds[i].GetComponent<SpriteRenderer>();
            sr.sprite = backgroundSprites[i];
            sr.color = Color.white;
        }

        StartCoroutine(ScrollBackgrounds());
    }

    float GetBackgroundHeight()
    {
        if (backgroundHeight == 0)
        {
            SpriteRenderer sr = backgroundPrefab.GetComponent<SpriteRenderer>();
            backgroundHeight = sr.bounds.size.y;
        }
        return backgroundHeight;
    }

    IEnumerator ScrollBackgrounds()
    {
        while (isScrolling)
        {
            foreach (GameObject bg in backgrounds)
            {
                bg.transform.position += Vector3.down * scrollSpeed * Time.deltaTime;
            }
            if (backgrounds[0].transform.position.y <= -GetBackgroundHeight())
            {
                RepositionBackground();
            }

            yield return null;
        }
    }

   
    /// Replace le background qui est en bas en le repositionnant en haut de la pile,
    /// lui assigne toujours le 3ᵉ sprite et le remet immédiatement en affichage.
    void RepositionBackground()
    {
        GameObject lowestBg = backgrounds[0];

        lowestBg.transform.position = new Vector3(
            0,
            backgrounds[2].transform.position.y + GetBackgroundHeight(),
            0
        );

        for (int i = 0; i < backgrounds.Length - 1; i++)
        {
            backgrounds[i] = backgrounds[i + 1];
        }
        backgrounds[2] = lowestBg;

        SpriteRenderer sr = backgrounds[2].GetComponent<SpriteRenderer>();
        sr.sprite = backgroundSprites[2];
        sr.color = Color.white; 
    }
}
