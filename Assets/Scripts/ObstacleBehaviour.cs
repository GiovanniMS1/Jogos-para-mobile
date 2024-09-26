using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // LoadScene

public class ObstacleBehaviour : MonoBehaviour
{
    [Tooltip("How long to wait before restarting the game")]
    public float waitTime = 2.0f;

    [Tooltip("Explosion effect to play when tapped")]
    public GameObject explosion;

    private void OnCollisionEnter(Collision collision)
    {
        // First check if we collided with the player
        if(collision.gameObject.GetComponent<PlayerBehaviour>())
        {
            // Destroy the player
            Destroy(collision.gameObject);

            // Call the function ResetGame after
            // waitTime has passed
            Invoke("ResetGame",waitTime);
        } 
    }

    /// <summary>
    /// will restart the currentlu loaded level
    /// </summary>
    private void ResetGame()
    {
        // Get the current level's name
        string sceneName = SceneManager.GetActiveScene().name;

        // Restart the current level
        SceneManager.LoadScene(sceneName);
    }

    private void PlayerTouch()
    {
        if(explosion != null)
        {
            var particles = Instantiate(explosion,transform.position,Quaternion.identity);
            Destroy(particles, 1.0f);
        }

        Destroy(this.gameObject);
    }
}
