using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Responsible for moving the player automatically and receiving input
/// </summary>
[RequireComponent(typeof(Rigidbody))]

public class PlayerBehaviour : MonoBehaviour
{
    /// <summary>
    /// A Reference to the Rigidbody component
    /// </summary>

    private Rigidbody rb;

    [Tooltip("How fast the ball moves left / right")]
    public float dodgeSpeed = 5f;

    [Tooltip("How fast the ball moves foward automatically")]
    [Range(0f, 10f)]
    public float rollSpeed = 5f;

    public enum MobileHorizMovement
    {
        Accelerometer,
        ScreenTouch
    }

    [Tooltip("What horizontal movement type should be used")]
    public MobileHorizMovement horizMovement = MobileHorizMovement.Accelerometer;

    [Header("Swipe properties")]
    [Tooltip("How far will the player move upon swiping")]
    public float swipeMove = 2f;

    [Tooltip("How far must the player swipe before we will execute the action (in crease)")]
    public float minSwipeDistance = 0.25f;

    /// <summary>
    /// Used to hold the value that converts
    /// minSwipeDistance to pixels
    /// </summary>
    private float minSwipeDistancePixels;

    /// <summary>
    /// Stores the starting position of mobile touch
    /// events
    /// </summary>
    private Vector2 touchStart;

    [Header("Scaling Properties")]
    [Tooltip("The minimum size (in Unity units) that the player should be")]
    public float minScale = 0.5f;

    [Tooltip("The maximum size (in Unity units) that the player should be")]
    public float maxScale = 3.0f;

    /// <summary>
    /// The current scale of the player
    /// </summary>
    private float currentScale = 1;

    // Start is called before the first frame update
    void Start()
    {
        //Get access to our Rigidbody component
        rb = GetComponent<Rigidbody>();

        minSwipeDistancePixels = minSwipeDistance * Screen.dpi;
    }

    /// <summary>
    /// Update is called once per frame
    /// </summary>
    private void Update()
    {
        /* Check if we are running either in the unity
         * editor or in a standalone build*/
        #if UNITY_STANDALONE || UNITY_EDITOR

        /* If the mouse is tapped */
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 screenPos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            TouchObjects(screenPos);
        }

        /* Check if we are running on a mobile device */
        #elif UNITY_ANDROID
            
            /* Check if input has registered more than zero touches */
            if (Input.touchCount > 0)
            {
                /* Store the first touch detected */
                Touch touch = Input.touches[0];

                TouchObjects(touch.position);
                SwipeTeleport(touch);
                ScalePlayer();
            }

        #endif
    }

    /// <summary>
    /// FixedUpdate is a prime place to put physics
    /// calculations happening over a period of time
    /// </summary>
    void FixedUpdate()
    {
        //Check if we're moving to the side
        var horizontalSpeed = Input.GetAxis("Horizontal") * dodgeSpeed;

    /* Check if we are running either in the Unity
     * editor or in a * standalone build */
    #if UNITY_STANDALONE || UNITY_WEBPLAYER || UNITY_EDITOR
            /* If the mouse is held down (or the screen is tapped)
             * on Mobile. */
            if (Input.GetMouseButton(0))
            {
                var screenPos = Input.mousePosition;
                horizontalSpeed = CalculateMovement(screenPos);
            }

    /* Check if we are running on a mobile device */
    #elif UNITY_ANDROID

        switch (horizMovement)
        {
            case MobileHorizMovement.Accelerometer:
                /* Move player based on accelerometer direction */
                horizontalSpeed = Input.acceleration.x * dodgeSpeed;
                break;
            case MobileHorizMovement.ScreenTouch:
                /* Check if Input registered more than zero touches */
                if (Input.touchCount > 0)
                {
                    var firstTouch = Input.touches[0];
                    var screenPos = firstTouch.position;
                    horizontalSpeed = CalculateMovement(screenPos);
                }
                break;
        }

        if (horizMovement == MobileHorizMovement.Accelerometer)
        {
            //Move player based on direction of the accelerometer
            horizontalSpeed = Input.acceleration.x * dodgeSpeed;
        }

        //Check if Input has registered more than one zero touches
        if (Input.touchCount > 0)
        {
            if (horizMovement == MobileHorizMovement.ScreenTouch)
            {
                //Store the first touch detected
                Touch touch = Input.touches[0];
                horizontalSpeed = CalculateMovement(touch.position);
            }
        }

    #endif

        rb.AddForce(horizontalSpeed, 0 ,rollSpeed);
        
    }

    /// <summary>
    /// Will figure out where to move the player horizontaly
    /// </summary>
    /// <param name="screenPos">
    /// The position that player has touched/clicked on in screen space
    /// </param>
    /// <returns>
    /// The direction to move in the x axis
    /// </returns>
    private float CalculateMovement(Vector3 screenPos)
    {
        /* Get a reference to the camera for converting between spaces */
        var cam = Camera.main;

        /* Converts mouse position to a 0 or 1 range */
        var viewPos = cam.ScreenToViewportPoint(screenPos);

        float xMove = 0f;

        /* If we press the right side of the screen */
        if (viewPos.x < 0.5f)
        {
            xMove = -1;
        }
        else
        {
            /* otherwise we're on the left */
            xMove = 1;
        }

        /* Replace horizontalSpeed with our own value */
        return xMove * dodgeSpeed;
    }

    /// <summary>
    /// Will teleport the player if swiped to the left or right
    /// </summary>
    /// <param name="touch">
    /// Current touch event
    /// </param>
    private void SwipeTeleport(Touch touch)
    {
        /* Check if the touch just started */
        if (touch.phase == TouchPhase.Began)
        {
            /* If so, set touchStart */
            touchStart = touch.position;
        }

        /* If the touch has ended */
        else if (touch.phase == TouchPhase.Ended)
        {
            /* Get the position the touch ended at */
            Vector2 touchEnd = touch.position;

            /* Calculate the difference between the 
             * beginnig and end of the touch on the x axis */
            float x = touchEnd.x - touchStart.x;

            /* If not moving far enough, don't do the teleport */
            if (Mathf.Abs(x) < minSwipeDistancePixels)
            {
                return;
            }

            Vector3 moveDirection;
            /* If moved negatively in the x axis, move left */
            if (x < 0)
            {
                moveDirection = Vector3.left;
            }
            else
            {
                /* Otherwise player is on the right */
                moveDirection = Vector3.right;
            }

            RaycastHit hit;

            /* Only move if player woundn't hit something */
            if (!rb.SweepTest(moveDirection, out hit, swipeMove))
            {
                /* Move the player */
                var movement = moveDirection * swipeMove;
                var newPos = rb.position + movement;

                rb.MovePosition(newPos);
            }
        }
    }

    /// <summary>
    /// Will change the player's scale via pinching and stretching two touch events
    /// </summary>
    private void ScalePlayer()
    {
        /* We must have two touches to check if we are scaling object */
        if (Input.touchCount != 2)
        {
            return;
        }
        else
        {
            /* Store the touche detected */
            Touch touch0 = Input.touches[0];
            Touch touch1 = Input.touches[1];

            Vector2 t0Pos = touch0.position;
            Vector2 t0Delta = touch0.deltaPosition;

            Vector2 t1Pos = touch1.position;
            Vector2 t1Delta = touch1.deltaPosition;

            /* Find the previous frame position of each touch */
            Vector2 t0Prev = t0Pos - t0Delta;
            Vector2 t1Prev = t1Pos - t1Delta;

            /* Find the distance (or magnitude) between the touches in each frame */
            float prevTDeltaMag = (t0Prev - t1Prev).magnitude;
            float tDeltaMag = (t0Pos - t1Pos).magnitude;

            /* Found the difference in the distances between each frame */
            float deltaMagDiff = prevTDeltaMag - tDeltaMag;

            /* Keep the chance consistent no matter what the framerate is */
            float newScale = currentScale;
            newScale -= (deltaMagDiff * Time.deltaTime);

            //Ensure that the new value is valid
            newScale = Mathf.Clamp(newScale, minScale, maxScale);

            /* Update the player's scale */
            transform.localScale = Vector3.one * newScale;

            /* Set our current scale for the next frame */
            currentScale = newScale;
        }
    }

    /// <summary>
    /// Will determine if we are touching a game object
    /// and if so call events for it
    /// </summary>
    /// <param name="screenPos">
    /// The position of the touch
    /// in screen space
    /// </param>
    private static void TouchObjects(Vector2 screenPos)
    {
        /* Convert the position into a ray */
        Ray touchRay = Camera.main.ScreenPointToRay(screenPos);
        RaycastHit hit;

        /* create a LayerMask that will collide with all
         * possible channels */
        int layerMask = ~0;

        /* Are we touching an object with a collider? */
        if (Physics.Raycast(touchRay, out hit, Mathf.Infinity, layerMask, QueryTriggerInteraction.Ignore))
        {
            /* Call the PlayerTouch function if it exists 
             * on a component attached to this object */
            hit.transform.SendMessage("PlayerTouch", SendMessageOptions.DontRequireReceiver);
        }
    }

    /// <summary>
    /// Will determine if we are touching a game object
    /// and if so call events for it
    /// </summary>
    /// <param name="touch">
    /// Our touch event
    /// </param>
    private static void TouchObjects(Touch touch)
    {
        /* Convert the position into a ray */
        Ray touchRay = Camera.main.ScreenPointToRay(touch.position);
        RaycastHit hit;

        /* create a LayerMask that will collide with all
         * possible channels */
        int layerMask = ~0;

        /* Are we touching an object with a collider? */
        if (Physics.Raycast(touchRay, out hit, Mathf.Infinity, layerMask, QueryTriggerInteraction.Ignore))
        {
            /* Call the PlayerTouch function if it exists 
             * on a component attached to this object */
            hit.transform.SendMessage("PlayerTouch", SendMessageOptions.DontRequireReceiver);
        }
    }


}