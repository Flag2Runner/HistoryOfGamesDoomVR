using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PinkyAI : EnemyCharacter
{
    public enum AIState { Idle, Chasing, Telegraphing, Charging }

    [Header("Perception")]
    [Tooltip("How close the player needs to be for Pinky to see them")]
    public float sightRange = 15f;

    [Tooltip("How long Pinky charges forward (seconds)")]
    public float chargeDuration = 0.5f;

    [Header("Movement & Attack")]
    public float moveSpeed = 3.0f;
    public float chargeSpeed = 10.0f;
    public float attackRange = 4.0f;
    public int meleeDamage = 15;
    [Tooltip("How long Pinky stands still before lunging")]
    public float telegraphTime = 0.5f;

    private CharacterController controller;
    private Transform playerTarget;
    [SerializeField] private AIState currentState = AIState.Idle;
    private float stateTimer = 0f;

    protected override void Start()
    {
        base.Start();
        controller = GetComponent<CharacterController>();

        if (PlayerCharacter.Instance != null)
        {
            playerTarget = PlayerCharacter.Instance.transform;
        }
    }

    private void Update()
    {
        if (currentHealth <= 0 || playerTarget == null) return;

        // Calculate distance and target point (ignoring height differences)
        float distance = Vector3.Distance(transform.position, playerTarget.position);
        Vector3 targetPos = new Vector3(playerTarget.position.x, transform.position.y, playerTarget.position.z);

        switch (currentState)
        {
            case AIState.Idle:
                // Only wake up if close enough AND we have line of sight!
                if (distance <= sightRange && HasLineOfSight())
                {
                    currentState = AIState.Chasing;
                }
                break;

            case AIState.Chasing:
                transform.LookAt(targetPos);

                // If player runs away, lose aggro and go back to idle
                if (distance > sightRange * 1.5f)
                {
                    currentState = AIState.Idle;
                }
                else if (distance <= attackRange)
                {
                    // Winding up for the attack!
                    currentState = AIState.Telegraphing;
                    stateTimer = telegraphTime;
                }
                else
                {
                    // Just keep walking forward
                    controller.SimpleMove(transform.forward * moveSpeed);
                }
                break;

            case AIState.Telegraphing:
                // Stand perfectly still, but keep looking at the player
                transform.LookAt(targetPos);
                stateTimer -= Time.deltaTime;

                if (stateTimer <= 0)
                {
                    // Time to lunge!
                    currentState = AIState.Charging;
                    stateTimer = chargeDuration; // The charge lasts for 0.4 seconds
                }
                break;

            case AIState.Charging:
                // LOCK rotation. Don't look at the player, just blast forward like a bull!
                controller.SimpleMove(transform.forward * chargeSpeed);
                stateTimer -= Time.deltaTime;

                // If we get close enough during the charge, CHOMP!
                if (distance <= 1.5f)
                {
                    AttackPlayer();
                    currentState = AIState.Chasing; // Reset state after hitting
                }
                // If the timer runs out and we missed the player, go back to chasing
                else if (stateTimer <= 0)
                {
                    currentState = AIState.Chasing;
                }
                break;
        }
    }

    // This is the "Reverse Perception" check!
    //needed to do it this way cause Ray interactions are off on the XR Origin thing idk :/
    private bool HasLineOfSight()
    {
        Vector3 playerChest = playerTarget.position + new Vector3(0, 1.2f, 0);
        Vector3 pinkyChest = transform.position + Vector3.up;

        Vector3 dirToPlayer = (playerChest - pinkyChest).normalized;
        float distanceToPlayer = Vector3.Distance(pinkyChest, playerChest);

        // Draw the laser in the Scene view so you can still test it!
        Debug.DrawRay(pinkyChest, dirToPlayer * distanceToPlayer, Color.red);

        // We shoot the raycast, BUT we strictly limit its length to the exact distance to the player.
        if (Physics.Raycast(pinkyChest, dirToPlayer, out RaycastHit hit, distanceToPlayer))
        {
            // The laser hit SOMETHING before it reached the player's distance.
            // Because the player is on the "Ignore Raycast" layer, this MUST be a wall or obstacle!
            return false; // Vision is blocked!
        }

        // If the laser traveled the full distance and hit absolutely nothing, 
        // the path is completely clear. Pinky sees you!
        return true;
    }

    private void AttackPlayer()
    {
        Debug.Log("Pinky lands the charge!");
        if (PlayerCharacter.Instance != null)
        {
            PlayerCharacter.Instance.TakeDamage(meleeDamage);
        }
    }
}