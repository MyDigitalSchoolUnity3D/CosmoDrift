using UnityEngine;

public class PlanetDebug : MonoBehaviour
{
    private Planet planetScript;
    private Vector3 lastPosition;

    void Start()
    {
        planetScript = GetComponent<Planet>();
        lastPosition = transform.position;
    }

    void Update()
    {
        // Vérifier si la planète bouge
        float movement = Vector3.Distance(transform.position, lastPosition);
        if (movement > 0.001f)
        {
            Debug.Log($"Planet {name} moved {movement} units");
        }
        lastPosition = transform.position;

        // Interface de débogage
        if (Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log("Force this planet to fall");
            planetScript.StartFalling();

            // Aussi essayer en modifiant directement
            System.Reflection.FieldInfo field = typeof(Planet).GetField("gameStarted",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);

            if (field != null)
            {
                field.SetValue(planetScript, true);
                Debug.Log("Forced gameStarted=true via Reflection");
            }
        }
    }

    void OnGUI()
    {
        // Ajouter un bouton de test à l'écran
        if (GUI.Button(new Rect(10, 70, 150, 30), "Force Planet " + name + " to Fall"))
        {
            planetScript.StartFalling();
            Debug.Log($"Manual trigger: Force planet {name} to fall");
        }
    }
}
