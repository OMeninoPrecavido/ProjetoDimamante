using UnityEngine;

public class LerpTest : MonoBehaviour
{
    [SerializeField] Transform startingPos;
    [SerializeField] Transform endingPos;

    // Movement speed in units per second.
    public float speed = 1.0F;

    // Time when the movement started.
    private float startTime;

    // Total distance between the markers.
    private float journeyLength;

    void Start()
    {
        // Keep a note of the time the movement started.
        startTime = Time.time;

        // Calculate the journey length.
        journeyLength = Vector3.Distance(startingPos.position, endingPos.position);
    }


    private void Update()
    {
        // Distance moved equals elapsed time times speed..
        float distCovered = (Time.time - startTime) * speed;

        // Fraction of journey completed equals current distance divided by total distance.
        float fractionOfJourney = distCovered / journeyLength;

        // Set our position as a fraction of the distance between the markers.
        transform.position = Vector3.Lerp(startingPos.position, endingPos.position, fractionOfJourney);

        if (transform.position == endingPos.position)
        {
            startTime = Time.time;
            Transform aux = startingPos;
            startingPos = endingPos;
            endingPos = aux;
        }
    }
}

