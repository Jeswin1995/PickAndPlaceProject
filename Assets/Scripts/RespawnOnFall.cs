using UnityEngine;

public class RespawnOnFall : MonoBehaviour
{
    [Tooltip("The Y position threshold below which the object will respawn.")]
    public float fallThreshold = -10f;

    private Vector3 initialPosition;

    void Start()
    {
        // Store the initial position of the GameObject
        initialPosition = transform.localPosition;
    }

    void Update()
    {
        // Check if the localPosition.y falls below the threshold
        if (transform.localPosition.y < fallThreshold)
        {
            Respawn();
        }
    }

    void Respawn()
    {
        // Reset the GameObject's position to its initial position
        transform.localPosition = initialPosition;
    }
}