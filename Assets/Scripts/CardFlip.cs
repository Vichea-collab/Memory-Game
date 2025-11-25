using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardFlip : MonoBehaviour
{
    [Header("References")]
    // Assign these in the Inspector to avoid "NullReference" errors
    [SerializeField] GameObject Card; 
    [SerializeField] GameObject gameController;
    [SerializeField] GameObject audioControl;
    [SerializeField] GameObject pairs;

    public static SpriteRenderer previousCard, presentCard = null, fstCard = null;
    public static int count = 0;
    public Sprite[] front;
    public Sprite back; 
    public SpriteRenderer spriteRenderer;
    
    GameObject hoverCard;
    bool isMouseHover, allowed=true;
    public int frontIndex;
    Vector3 ogSize;

    bool updateFlag = false;
    static int c = 0;

    // Optimization: Store scripts so we don't use GetComponent every frame
    private GameController gameControllerScript;
    private AudioControl audioControlScript;

    void Start()
    {
        // 1. Attempt to find objects automatically if not assigned in Inspector
        if (pairs == null) pairs = GameObject.Find("PAirs"); // Double check capitalization in your scene!
        if (gameController == null) gameController = GameObject.Find("GameController");
        if (audioControl == null) audioControl = GameObject.Find("AudioControl");
        if (Card == null) Card = GameObject.Find("Card");

        // 2. Set up local components
        spriteRenderer = GetComponent<SpriteRenderer>();
        hoverCard = gameObject; // GetComponent<SpriteRenderer>().gameObject is just gameObject
        ogSize = transform.localScale;
        
        isMouseHover = false;
        count = 0;

        // 3. Cache the scripts (Safety Check)
        if (gameController != null)
            gameControllerScript = gameController.GetComponent<GameController>();
        else
            Debug.LogError("GameController object not found! Check your scene names.");

        if (audioControl != null)
            audioControlScript = audioControl.GetComponent<AudioControl>();
        else
            Debug.LogError("AudioControl object not found! Check your scene names.");
    }

    public void OnMouseDown() 
    {
        // Safety check: verify pairs and scripts exist before using
        if (pairs != null)
            pairs.GetComponent<pairs>().SetRotationFalse();

        if(allowed && gameControllerScript != null)
        {
            if(gameControllerScript.match[frontIndex] == false)
            {
                if(spriteRenderer.sprite == back)
                {
                    if(!gameControllerScript.TwoCardsUp())
                    {
                        count++;
                        spriteRenderer.sprite = front[frontIndex];
                        
                        if(gameControllerScript.firstCard == null)
                            gameControllerScript.firstCard = spriteRenderer;
                        else
                            gameControllerScript.secondCard = spriteRenderer;
                        
                        gameControllerScript.AddFrontFace(frontIndex);
                        gameControllerScript.match[frontIndex] = gameControllerScript.IsMatch();
                        
                        if(gameControllerScript.match[frontIndex])
                        {
                            Debug.Log("Success!");
                        }
                        gameControllerScript.twoCardsUp = gameControllerScript.TwoCardsUp();
                        
                        if(!gameControllerScript.TwoCardsUp())
                            fstCard = spriteRenderer;
                        else
                            presentCard = spriteRenderer;
                    }
                }
                else
                {
                    spriteRenderer.sprite = back;
                    gameControllerScript.RemoveFrontFace(frontIndex);
                }
            }
        }
    }

    IEnumerator FlipBack()
    {
        yield return new WaitForSeconds(1);
        c++;
        if(spriteRenderer == fstCard)
        {
            spriteRenderer.sprite = back;
            fstCard = null;
        }
        if(spriteRenderer == presentCard)
        {
            spriteRenderer.sprite = back;
            presentCard = null;
        }
        
        if(gameControllerScript != null)
            gameControllerScript.RemoveAllFrontFace();
        
        allowed = true;
    }

    void FixedUpdate()
    {
        if (gameControllerScript == null) return; // Stop if controller is missing

        if(gameControllerScript.match[frontIndex] && !updateFlag) 
        {
            updateFlag = true;
            spriteRenderer.color = Color.white;
        }
        
        if(gameControllerScript.TwoCardsUp() && allowed)
        {
            if(!gameControllerScript.IsMatch())
            {
                allowed = false;
                
                // Safe audio call
                if (audioControlScript != null)
                    audioControlScript.WrongSound();

                StartCoroutine(FlipBack());
            }
        }
    }

    IEnumerator TimeDelay(int delayTime)
    {
        yield return new WaitForSeconds(2);
        spriteRenderer.sprite = back;
    }

    private void OnMouseOver()
    {
        if(!isMouseHover)
        {
            Hover();
        }
        isMouseHover = true;
    }

    void OnMouseExit()
    {
        if(isMouseHover)
            Hover_nt();
        isMouseHover = false;
    }

    void Hover()
    {
        // Prevent crash if script is missing
        if (gameControllerScript == null) return;

        if(!gameControllerScript.match[frontIndex]) 
        { 
            // THIS IS WHERE YOUR ERROR WAS (Line 158)
            // We used to access audioControl directly, now we check if it exists first
            transform.localScale += new Vector3(0.9f, 0.9f, 0); // Changed 9f to 0.9f, 9f is usually way too big? Check this.
            
            if(audioControlScript != null)
            {
                audioControlScript.HoverSound();
            }
            
            spriteRenderer.color = Color.green;
        }
        else
        {
            spriteRenderer.color = Color.red;
        }
    }

    void Hover_nt()
    {  
        if (gameControllerScript == null) return;

        if(!gameControllerScript.match[frontIndex]) 
        {
            spriteRenderer.color = Color.white;
        }
        transform.localScale = ogSize;
        updateFlag = false;
    }
}