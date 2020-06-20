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

    private Animator anim;

    private MovementController movementController;

    // Start is called before the first frame update
    void Start()
    {
        movementController = GetComponent<MovementController>();
        anim = GetComponent<Animator>();
        Grid grid = GameObject.FindGameObjectWithTag("Grid").GetComponent<Grid>();
        globalObjects = grid.GetComponent<GlobalObjects>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
            Application.Quit();

        // Get Information Cell Below Player's Feet
        TileMetaData tileMeta = movementController.GetTileBelowPlayer(transform.position + new Vector3(0, -0.5f, 0));

        if (!globalObjects.Paused)
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

        // Check if cell below us has an action if interact key is pressed
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (tileMeta.Action != "")
            {
                string[] actionParameters = tileMeta.Action.Split(';');
                switch (actionParameters[0])
                {
                    case "ShowSign":
                        if (globalObjects.SignActive)
                            globalObjects.HideSign();
                        else
                            globalObjects.ShowSign(actionParameters[1]);
                        break;
                }
            }
        }
    }
}
