using UnityEngine;
using System.Collections;

public class Planet : MonoBehaviour
{
    public float rotationSpeed = 10f;
    public float fallSpeed = 0.5f;
    public float planetSize = 1f;
    private bool rotatingRight;
    private bool gameStarted = false;
    private bool isStartPlanet = false;
    private SpriteRenderer spriteRenderer;

    // Added variables to store size limits
    private float minSize = 0.8f;
    private float maxSize = 1.5f;
    
    // Special planet properties
    [Header("Special Planet Settings")]
    public bool isSpecialPlanet = false;
    public enum SpecialEffect { None, HighJump, ScoreBonus, SpeedBoost }
    public SpecialEffect specialEffect = SpecialEffect.None;
    public float effectValue = 2f; // Multiplier or bonus value
    public Color specialColor = Color.yellow; // Visual indicator

    void Start()
    {
        // Get the SpriteRenderer
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }

        // Ensure the trigger is activated for collision
        CircleCollider2D collider = GetComponent<CircleCollider2D>();
        if (collider != null)
        {
            collider.isTrigger = true;
        }

        // Check if this is the starting planet in multiple ways

        // Method 1: check with the tag
        if (gameObject.CompareTag("StartPlanet"))
        {
            isStartPlanet = true;
        }
        // Method 2: check with the reference in the PlanetSpawner
        else
        {
            PlanetSpawner spawner = Object.FindFirstObjectByType<PlanetSpawner>();
            if (spawner != null && gameObject == spawner.startPlanet)
            {
                isStartPlanet = true;
                gameObject.tag = "StartPlanet"; // Make sure the tag is correct
            }
        }

        // If it's the starting planet, don't make it rotate
        if (isStartPlanet)
        {
            rotationSpeed = 0f;
            // DO NOT change the size of the start planet
        }
        else
        {
            // Apply the size only for planets that are NOT the starting planet
            ApplyPlanetSize();
        }
    }

    public void Initialize(float speed, float size, float fall, Sprite texture = null)
    {
        rotationSpeed = speed;
        fallSpeed = fall;

        // Don't modify the size of the starting planet
        if (!isStartPlanet)
        {
            // Get size limits from PlanetSpawner
            PlanetSpawner spawner = FindFirstObjectByType<PlanetSpawner>();
            if (spawner != null)
            {
                minSize = spawner.minPlanetSize;
                maxSize = spawner.maxPlanetSize;
            }

            // Ensure the size is within limits
            planetSize = Mathf.Clamp(size, minSize, maxSize);
        }

        rotatingRight = Random.value > 0.5f;

        // If it's the starting planet, don't make it rotate
        if (isStartPlanet)
        {
            rotationSpeed = 0f;
        }

        // Apply the texture if provided
        if (texture != null && !isStartPlanet)
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
                if (spriteRenderer == null)
                {
                    spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
                }
            }

            spriteRenderer.sprite = texture;

            // Make sure the sprite is visible and correctly configured
            spriteRenderer.enabled = true;
            spriteRenderer.sortingOrder = 1;
        }
        
        // Initialize special planet (about 10% chance)
        if (!isStartPlanet && Random.value < 0.1f)
        {
            isSpecialPlanet = true;
            // Random special effect
            specialEffect = (SpecialEffect)Random.Range(1, System.Enum.GetValues(typeof(SpecialEffect)).Length);
            SetupSpecialPlanet();
        }

        // Apply the size after setting the sprite
        if (!isStartPlanet)
        {
            ApplyPlanetSize();
        }
    }
    
    private void SetupSpecialPlanet()
    {
        if (isStartPlanet || !isSpecialPlanet) return;
        
        // Apply visual effect to indicate special planet
        if (spriteRenderer != null)
        {
            spriteRenderer.color = specialColor;
        }
    }
    
    // Method to apply the special effect when player lands
    public void ApplySpecialEffect(PlayerController player)
    {
        if (!isSpecialPlanet) return;
        
        switch (specialEffect)
        {
            case SpecialEffect.HighJump:
                player.jumpForce *= effectValue;
                StartCoroutine(ResetPlayerJumpForce(player));
                break;
            case SpecialEffect.ScoreBonus:
                GameManager gameManager = FindFirstObjectByType<GameManager>();
                if (gameManager != null)
                {
                    gameManager.AddScoreBonus(100 * effectValue);
                }
                break;
            case SpecialEffect.SpeedBoost:
                player.moveSpeed *= effectValue;
                StartCoroutine(ResetPlayerMoveSpeed(player));
                break;
        }
    }
    
    private IEnumerator ResetPlayerJumpForce(PlayerController player)
    {
        yield return new WaitForSeconds(5f);
        player.jumpForce /= effectValue;
    }
    
    private IEnumerator ResetPlayerMoveSpeed(PlayerController player)
    {
        yield return new WaitForSeconds(5f);
        player.moveSpeed /= effectValue;
    }

    public void ApplyPlanetSize()
    {
        if (isStartPlanet) return;

        // Apply visual scale
        transform.localScale = new Vector3(planetSize, planetSize, planetSize);

        // Adjust the collider
        CircleCollider2D circleCollider = GetComponent<CircleCollider2D>();
        if (circleCollider != null)
        {
            circleCollider.offset = Vector2.zero;

            // Alternative method: instead of adjusting the radius, recreate the collider
            // based on the exact size of the sprite
            if (spriteRenderer != null && spriteRenderer.sprite != null)
            {
                // Calculation based on sprite dimensions and current scale
                float spriteWidth = spriteRenderer.sprite.bounds.size.x;
                float spriteRadius = spriteWidth / 2.0f;
                
                // The radius of the collider must be defined without taking scale into account
                // because Unity adjusts it automatically afterwards
                circleCollider.radius = spriteRadius;
            }
            else
            {
                // Fallback if no sprite
                circleCollider.radius = 0.5f;
            }
        }
    }

    void Update()
    {
        // Planets rotate all the time (except the starting planet which has rotationSpeed = 0)
        float direction = rotatingRight ? 1 : -1;
        transform.Rotate(Vector3.forward * rotationSpeed * direction * Time.deltaTime);

        // But they only fall when the game has started
        if (gameStarted)
        {
            transform.position += Vector3.down * fallSpeed * Time.deltaTime;

            // Destroy the planet when it's no longer visible
            if (!IsVisibleFrom(Camera.main))
            {
                Destroy(gameObject);
            }
        }
    }

    public void StartFalling()
    {
        // Enable falling for this planet
        gameStarted = true;

        // Make sure the fall speed is positive - use a default value if necessary
        if (fallSpeed <= 0)
        {
            PlanetSpawner spawner = FindFirstObjectByType<PlanetSpawner>();
            if (spawner != null)
            {
                fallSpeed = Random.Range(spawner.minFallSpeed, spawner.maxFallSpeed);
            }
            else
            {
                fallSpeed = 0.5f; // Default value if PlanetSpawner is not found
            }
        }

        // Add a downward force via Rigidbody2D if available
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.simulated = true;
            rb.gravityScale = fallSpeed * 2; // Use fallSpeed to influence gravity
        }
    }

    bool IsVisibleFrom(Camera cam)
    {
        if (cam == null) return false;

        Vector3 screenPoint = cam.WorldToViewportPoint(transform.position);
        float buffer = 0.2f;

        return screenPoint.x > -buffer &&
               screenPoint.x < (1 + buffer) &&
               screenPoint.y > -buffer;
    }
}
