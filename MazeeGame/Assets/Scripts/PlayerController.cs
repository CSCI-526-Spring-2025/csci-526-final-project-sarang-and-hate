using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections; 
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    private Rigidbody rb;  
    public Transform plane;
    // public GameObject gameOverPanel; 
    // public Button restartButton; 
    public GameObject victoryPanel;
    public Button victoryRestartButton;
    private Renderer playerRenderer;
    private Color currentColor;
    private bool gameWon = false;
    public Vector3 victoryLocalPosition = new Vector3(2.98f, 0f, 1.12f);

    void Start()
    {
        rb = GetComponent<Rigidbody>(); 

        // gameOverPanel.SetActive(false); 
        victoryPanel.SetActive(false);

        playerRenderer = GetComponent<Renderer>();
        currentColor = playerRenderer.material.color; 


        // if (restartButton != null)
        // {
        //     restartButton.onClick.AddListener(RestartGame);
        // }
        if (victoryRestartButton != null)
        {
            victoryRestartButton.onClick.AddListener(RestartGame);
        }
    }

    void FixedUpdate()
    {
        Vector3 planeNormal = plane.up;
        Vector3 gravityDirection = -planeNormal.normalized * 100f;
        rb.AddForce(gravityDirection, ForceMode.Acceleration);
    }

    void Update()
    {
        if (!gameWon && CheckVictory())
        {
            Victory();
        }
    }

    bool CheckVictory()
    {
        Vector3 localBallPosition = plane.InverseTransformPoint(transform.position);
        Debug.Log($"Local Ball Position: {localBallPosition}, Expected Victory Position: {victoryLocalPosition}");
        return localBallPosition.x >= victoryLocalPosition.x && localBallPosition.z >= victoryLocalPosition.z;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall")) 
        {
            Renderer wallRenderer = collision.gameObject.GetComponent<Renderer>();

            if (wallRenderer != null && wallRenderer.material.color == currentColor)
            {
                Physics.IgnoreCollision(collision.collider, GetComponent<Collider>(), true);
            }
            else
            {
                // TakeDamage();
                // StartCoroutine(RotateWallSmoothly(collision.gameObject));
            }

        }
        else if (collision.gameObject.CompareTag("MovingWall"))
        {
            StartCoroutine(MoveWallSmoothly(collision.gameObject));
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Pill")) 
        {
            CollectPill(other.gameObject);
        }
    }

    void CollectPill(GameObject pill)
    {
        Renderer pillRenderer = pill.GetComponent<Renderer>();
        if (pillRenderer != null)
        {
            currentColor = pillRenderer.material.color; 
            playerRenderer.material.color = currentColor;
        }

        Destroy(pill); 
    }


    // void GameOver()
    // {
    //     Debug.Log("Game Over!");
    //     gameOverPanel.SetActive(true);
    //     Time.timeScale = 0;
    // }

    void Victory() 
    {
        gameWon = true; 
        Debug.Log("🎉 Victory!");
        victoryPanel.SetActive(true); 
        Time.timeScale = 0; 
    }


    void RestartGame()
    {
        Time.timeScale = 1; 
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private Dictionary<GameObject, bool> wallMoveStates = new Dictionary<GameObject, bool>();

    IEnumerator MoveWallSmoothly(GameObject wall)
    {
        float moveDistance = 2.1f;
        float moveSpeed = 2f;
        float duration = moveDistance / moveSpeed;
        float elapsedTime = 0;

        Transform planeTransform = plane.transform;

        Vector3 localPosition = planeTransform.InverseTransformPoint(wall.transform.position);

        bool moveUp = !wallMoveStates.ContainsKey(wall) || wallMoveStates[wall];

        Vector3 localTargetPosition = localPosition + new Vector3(0, 0, moveUp ? moveDistance : -moveDistance);

        wallMoveStates[wall] = !moveUp;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            Vector3 worldTargetPosition = planeTransform.TransformPoint(localTargetPosition);
            wall.transform.position = Vector3.Lerp(wall.transform.position, worldTargetPosition, elapsedTime / duration);

            yield return null;
        }

        wall.transform.position = planeTransform.TransformPoint(localTargetPosition);
    }
}
