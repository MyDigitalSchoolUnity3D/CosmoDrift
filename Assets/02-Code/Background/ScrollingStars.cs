using UnityEngine;

public class ScrollingStars : MonoBehaviour
{
    public float scrollSpeed = 2.0f; // Vitesse du défilement
    private float spriteHeight; // Hauteur du sprite

    void Start()
    {
        spriteHeight = GetComponent<SpriteRenderer>().bounds.size.y;
    }

    void Update()
    {
        // Déplacer vers le bas
        transform.position += Vector3.down * scrollSpeed * Time.deltaTime;

        // Lorsqu’une étoile quitte l’écran en bas, elle revient au-dessus de l’autre
        if (transform.position.y <= -spriteHeight)
        {
            transform.position += new Vector3(0, 2 * spriteHeight, 0);
        }
    }
}
