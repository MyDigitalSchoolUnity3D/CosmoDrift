using UnityEngine;

public class ScrollingStars : MonoBehaviour
{
    public float scrollSpeed = 2.0f; 
    private float spriteHeight;

    void Start()
    {
        spriteHeight = GetComponent<SpriteRenderer>().bounds.size.y;
    }

    void Update()
    {
        transform.position += Vector3.down * scrollSpeed * Time.deltaTime;
        if (transform.position.y <= -spriteHeight)
        {
            transform.position += new Vector3(0, 2 * spriteHeight, 0);
        }
    }
}
