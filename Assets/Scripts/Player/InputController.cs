using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MovementController))]
public class InputController : MonoBehaviour
{
    private const float MAXSPEED = 5.0f;
    private string lastAnimation = "";
    private TileMetaData cellBelowMeta;
    private GlobalObjects globalObjects;
    private NPC InteractableNPC;

    private Animator anim;

    private MovementController movementController;

    // Start is called before the first frame update
    void Start()
    {
        movementController = GetComponent<MovementController>();
        anim = GetComponent<Animator>();
        globalObjects = GameObject.FindGameObjectWithTag("Globals").GetComponent<GlobalObjects>();
    }

    public void SetInteractableNPC(NPC npc)
    {
        InteractableNPC = npc;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
            Application.Quit();

        // Dialogue Control
        if (globalObjects.GameState == GameStates.InDialogue)
        {
            if (Input.GetKeyDown(KeyCode.Return))
                globalObjects.GetComponent<DialogueController>().NextStep();
            else if (Input.GetKeyDown(KeyCode.DownArrow))
                globalObjects.GetComponent<DialogueController>().NextOption();
            else if (Input.GetKeyDown(KeyCode.UpArrow))
                globalObjects.GetComponent<DialogueController>().PreviousOption();
            
            return;
        }

        // Get Information Cell Below Player's Feet
        TileMetaData tileMeta = movementController.GetTileBelowPlayer(transform.position + new Vector3(0, -0.5f, 0));

        if (globalObjects.GameState == GameStates.Playing)
        {
            Vector2 velocity = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            Vector2.ClampMagnitude(velocity, MAXSPEED);
            velocity *= (MAXSPEED * Time.deltaTime);

            movementController.Move(ref velocity);

            string animationName = "";
            if (tileMeta.IsLadder & velocity.y != 0) animationName = "Walk Ladder";
            else if (velocity.y > 0) animationName = "Walk North";
            else if (velocity.y < 0) animationName = "Walk South";
            else if (velocity.x > 0) animationName = "Walk East";
            else if (velocity.x < 0) animationName = "Walk West";
            else animationName = lastAnimation.Replace("Walk", "Idle");

            if (animationName != lastAnimation)
            {
                lastAnimation = animationName;
                anim.Play(animationName);
            }
        }

        // Interaction Handler
        if (Input.GetKeyDown(KeyCode.Return))
        {
            // Check if we are near enough to interact with an NPC
            if (InteractableNPC != null)
            {
                string[] actionParams = InteractableNPC.InteractFunction.Split(';');
                switch (actionParams[0])
                {
                    case "Talk": 
                        globalObjects.GetComponent<DialogueController>().StartConversation(InteractableNPC.name);
                        globalObjects.DialogueCanvas.gameObject.SetActive(true);
                        break;
                }
            }
            else if (tileMeta.Action != "")
            {
                string[] actionParameters = tileMeta.Action.Split(';');
                switch (actionParameters[0])
                {
                    case "ShowSign":
                        if (globalObjects.GameState == GameStates.InSign)
                            globalObjects.HideSign();
                        else
                            globalObjects.ShowSign(actionParameters[1]);
                        break;
                }
            }
        }
    }
}
