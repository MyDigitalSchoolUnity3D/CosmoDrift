using UnityEngine;

public class StarSpawner : MonoBehaviour
{
    public GameObject starPrefab; 
    private GameObject star1, star2; 
    private float spriteHeight; 

    void Start()
    {
        star1 = Instantiate(starPrefab, Vector3.zero, Quaternion.identity);
        star2 = Instantiate(starPrefab, Vector3.up * GetSpriteHeight(), Quaternion.identity);

        star1.AddComponent<ScrollingStars>();
        star2.AddComponent<ScrollingStars>();
    }

    float GetSpriteHeight()
    {
        if (spriteHeight == 0)
        {
            spriteHeight = starPrefab.GetComponent<SpriteRenderer>().bounds.size.y;
        }
        return spriteHeight;
    }
}
