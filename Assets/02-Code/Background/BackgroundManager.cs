using UnityEngine;
using System.Collections;

public class BackgroundManager : MonoBehaviour
{
    [Header("Options Modifiables")]
    public float scrollSpeed = 2.0f;         // Vitesse de défilement
    public GameObject backgroundPrefab;      // Le prefab (contenant un SpriteRenderer)
    public Sprite[] backgroundSprites;       // Tableau de 3 sprites (dans l'ordre souhaité)

    // Paramètres internes (gérés automatiquement)
    private GameObject[] backgrounds = new GameObject[3]; // Instanciations des 3 backgrounds
    private float backgroundHeight;         // Hauteur du background (calculée automatiquement)
    private bool isScrolling = true;
    private float fadeDuration = 1.0f;        // Durée du fade-in (fixe)

    void Start()
    {
        // Vérification : on doit avoir 1 prefab et exactement 3 sprites
        if (backgroundPrefab == null)
        {
            Debug.LogError("❌ backgroundPrefab n'est pas assigné !");
            return;
        }
        if (backgroundSprites == null || backgroundSprites.Length < 3)
        {
            Debug.LogError("❌ Il faut exactement 3 sprites dans backgroundSprites !");
            return;
        }

        // Instancier 3 backgrounds empilés verticalement
        for (int i = 0; i < backgrounds.Length; i++)
        {
            Vector3 pos = new Vector3(0, i * GetBackgroundHeight(), 0);
            backgrounds[i] = Instantiate(backgroundPrefab, pos, Quaternion.identity);

            SpriteRenderer sr = backgrounds[i].GetComponent<SpriteRenderer>();
            sr.sprite = backgroundSprites[i];
            sr.color = Color.white;
        }

        // Lancer le scrolling
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
            // Faire défiler tous les backgrounds vers le bas
            foreach (GameObject bg in backgrounds)
            {
                bg.transform.position += Vector3.down * scrollSpeed * Time.deltaTime;
            }

            // Lorsque le premier background est complètement sorti (position y <= -backgroundHeight)
            if (backgrounds[0].transform.position.y <= -GetBackgroundHeight())
            {
                RepositionBackground();
            }

            yield return null;
        }
    }

    /// <summary>
    /// Replace le background qui est en bas en le repositionnant en haut de la pile,
    /// lui assigne toujours le 3ᵉ sprite et lance un fade-in pour une transition douce.
    /// </summary>
    void RepositionBackground()
    {
        // Récupérer le background le plus bas
        GameObject lowestBg = backgrounds[0];

        // On définit un léger chevauchement pour éviter une coupure brutale (overlap)
        float overlapOffset = 0.5f;

        // Repositionner ce background juste au-dessus du dernier (backgrounds[2])
        lowestBg.transform.position = new Vector3(
            0,
            backgrounds[2].transform.position.y + GetBackgroundHeight() - overlapOffset,
            0
        );

        // Décaler le tableau pour conserver l'ordre :
        // backgrounds[0] = backgrounds[1], backgrounds[1] = backgrounds[2], backgrounds[2] = lowestBg
        for (int i = 0; i < backgrounds.Length - 1; i++)
        {
            backgrounds[i] = backgrounds[i + 1];
        }
        backgrounds[2] = lowestBg;

        // Appliquer le 3ᵉ sprite au background replacé (selon ta condition)
        SpriteRenderer sr = backgrounds[2].GetComponent<SpriteRenderer>();
        sr.sprite = backgroundSprites[2];

        // Commencer le fade-in pour une transition douce
        sr.color = new Color(1, 1, 1, 0);
    }

    
}
