using UnityEngine;

public class StarSpawner : MonoBehaviour
{
    public GameObject starPrefab; // Prefab de l’étoile
    private GameObject star1, star2; // Deux étoiles pour un effet continu
    private float spriteHeight; // Hauteur du sprite

    void Start()
    {
        // Instancier deux étoiles superposées
        star1 = Instantiate(starPrefab, Vector3.zero, Quaternion.identity);
        star2 = Instantiate(starPrefab, Vector3.up * GetSpriteHeight(), Quaternion.identity);

        // Ajouter le script de défilement aux deux
        star1.AddComponent<ScrollingStars>();
        star2.AddComponent<ScrollingStars>();
    }

    float GetSpriteHeight()
    {
        if (spriteHeight == 0) // Récupérer la hauteur du sprite une seule fois
        {
            spriteHeight = starPrefab.GetComponent<SpriteRenderer>().bounds.size.y;
        }
        return spriteHeight;
    }
}
