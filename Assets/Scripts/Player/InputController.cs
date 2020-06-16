using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MovementController))]
public class InputController : MonoBehaviour
{
    private const float MAXSPEED = 5.0f;
    private string lastAnimation = "";
    private TileMetaData cellBelowMeta;

    private Animator anim;

    private MovementController movementController;

    // Start is called before the first frame update
    void Start()
    {
        movementController = GetComponent<MovementController>();
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 velocity = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        Vector2.ClampMagnitude(velocity, MAXSPEED);
        velocity *= (MAXSPEED * Time.deltaTime);

        movementController.Move(ref velocity);

        // Get Information Cell Below Player's Feet
        TileMetaData tileMeta = movementController.GetTileBelowPlayer(transform.position + new Vector3(0, -0.5f, 0));

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
}
