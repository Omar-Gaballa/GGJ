using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class PlayerMoveAndAnimate : MonoBehaviour
{
    [Header("References")]
    public Transform cameraTransform; // drag Main Camera here

    [Header("Movement Speeds")]
    public float walkSpeed = 2.5f;
    public float runSpeed = 6.0f;
    public float fastStrafeSpeed = 4.5f;
    public float backwardSpeed = 2.0f;

    [Header("Rotation")]
    public float rotationSpeed = 12f;

    [Header("Animation Smoothing")]
    public float animDampTime = 0.08f;

    [Header("Attack")]
    public float attackCooldown = 0.35f;
    public bool alternateSlashes = true;
    public int attackLayerIndex = 1; // Base=0, Attack=1

    CharacterController controller;
    Animator animator;

    int velXHash;
    int velZHash;

    int trigOutwardHash;
    int trigInwardHash;

    float nextAttackTime = 0f;
    bool nextIsOutward = true;

    bool isAttacking = false;

    // Must match your Animator STATE names exactly (Attack layer states)
    const string OUTWARD_STATE = "Stable Sword Outward Slash";
    const string INWARD_STATE  = "Stable Sword Inward Slash";

    // Must match your Animator PARAMETER names exactly
    const string TRIG_OUTWARD = "Trig_Outward";
    const string TRIG_INWARD  = "Trig_Inward";

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        velXHash = Animator.StringToHash("Velocity X");
        velZHash = Animator.StringToHash("Velocity Z");

        trigOutwardHash = Animator.StringToHash(TRIG_OUTWARD);
        trigInwardHash  = Animator.StringToHash(TRIG_INWARD);

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;
        
        // Ensure attack layer is properly weighted
        if (attackLayerIndex >= 0 && attackLayerIndex < animator.layerCount)
            animator.SetLayerWeight(attackLayerIndex, 1f);
    }

    void Update()
    {
        UpdateAttackState();

        // LMB Attack (only if not currently attacking)
        if (!isAttacking && Input.GetMouseButtonDown(0) && Time.time >= nextAttackTime)
        {
            TriggerAttack();
            nextAttackTime = Time.time + attackCooldown;
        }

        // INPUT
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        bool shift = Input.GetKey(KeyCode.LeftShift);

        // CAMERA-RELATIVE MOVE DIRECTION (WORLD SPACE)
        Vector3 moveDir;
        if (cameraTransform != null)
        {
            Vector3 camForward = cameraTransform.forward;
            Vector3 camRight = cameraTransform.right;

            camForward.y = 0f;
            camRight.y = 0f;

            camForward.Normalize();
            camRight.Normalize();

            moveDir = camRight * x + camForward * z;
        }
        else
        {
            moveDir = transform.right * x + transform.forward * z;
        }

        if (moveDir.sqrMagnitude > 1f)
            moveDir.Normalize();

        // SPEED RULES
        bool isRunning = shift && moveDir.sqrMagnitude > 0f;
        float speed = isRunning ? runSpeed : walkSpeed;

        // MOVE + ROTATE (LOCKED DURING ATTACK)
        if (!isAttacking)
        {
            controller.Move(moveDir * speed * Time.deltaTime);

            // FIX: Character faces the direction they're moving (all 8 directions)
            if (moveDir.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(moveDir, Vector3.up);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRot,
                    rotationSpeed * Time.deltaTime
                );
            }
        }

        // ----- ANIMATOR VALUES (MATCH BLEND TREE) -----
        // Since character now faces movement direction, use local space
        // Character always moves "forward" in their own space
        
        float targetVelX = 0f;
        float targetVelZ = 0f;

        if (!isAttacking && moveDir.sqrMagnitude > 0.001f)
        {
            // Convert world movement to local space
            Vector3 localMove = transform.InverseTransformDirection(moveDir);
            
            // For forward movement animation (character faces where they're going)
            // Just use forward/back based on local Z
            targetVelZ = isRunning ? 2f : 0.5f;
            
            // Optional: If you want slight strafe when moving diagonally
            // Uncomment these lines:
            // targetVelX = isRunning ? 2f * localMove.x : 0.5f * localMove.x;
        }
        else
        {
            targetVelX = 0f;
            targetVelZ = 0f;
        }

        animator.SetFloat(velXHash, targetVelX, animDampTime, Time.deltaTime);
        animator.SetFloat(velZHash, targetVelZ, animDampTime, Time.deltaTime);
    }

    void TriggerAttack()
    {
        // Zero out movement parameters immediately when attack starts
        animator.SetFloat(velXHash, 0f);
        animator.SetFloat(velZHash, 0f);
        
        animator.ResetTrigger(trigOutwardHash);
        animator.ResetTrigger(trigInwardHash);

        animator.SetTrigger(nextIsOutward ? trigOutwardHash : trigInwardHash);

        if (alternateSlashes)
            nextIsOutward = !nextIsOutward;
    }

    void UpdateAttackState()
    {
        if (attackLayerIndex < 0 || attackLayerIndex >= animator.layerCount)
        {
            isAttacking = false;
            return;
        }

        AnimatorStateInfo s = animator.GetCurrentAnimatorStateInfo(attackLayerIndex);
        
        // Check normalized time to ensure we're actually in the attack
        bool inAttackState = (s.IsName(OUTWARD_STATE) || s.IsName(INWARD_STATE)) && s.normalizedTime < 0.95f;
        
        // Check if transitioning TO an attack state
        bool transitioningToAttack = false;
        if (animator.IsInTransition(attackLayerIndex))
        {
            AnimatorStateInfo next = animator.GetNextAnimatorStateInfo(attackLayerIndex);
            transitioningToAttack = next.IsName(OUTWARD_STATE) || next.IsName(INWARD_STATE);
        }
        
        isAttacking = inAttackState || transitioningToAttack;
    }
}

// using UnityEngine;
//
// [RequireComponent(typeof(CharacterController))]
// [RequireComponent(typeof(Animator))]
// public class PlayerMoveAndAnimate : MonoBehaviour
// {
//     [Header("References")]
//     public Transform cameraTransform; // drag Main Camera here
//
//     [Header("Movement Speeds")]
//     public float walkSpeed = 2.5f;
//     public float runSpeed = 6.0f;
//     public float fastStrafeSpeed = 4.5f;
//     public float backwardSpeed = 2.0f; // back-walk speed when NOT holding Shift
//
//     [Header("Rotation")]
//     public float rotationSpeed = 12f;
//
//     [Header("Animation Smoothing")]
//     public float animDampTime = 0.08f;
//
//     [Header("Attack")]
//     public float attackCooldown = 0.35f;
//     public bool alternateSlashes = true;
//     public int attackLayerIndex = 1; // Base=0, Attack=1
//
//     CharacterController controller;
//     Animator animator;
//
//     int velXHash;
//     int velZHash;
//
//     int trigOutwardHash;
//     int trigInwardHash;
//
//     float nextAttackTime = 0f;
//     bool nextIsOutward = true;
//
//     bool isAttacking = false;
//
//     // Must match your Animator STATE names exactly (Attack layer states)
//     const string OUTWARD_STATE = "Stable Sword Outward Slash";
//     const string INWARD_STATE  = "Stable Sword Inward Slash";
//
//     // Must match your Animator PARAMETER names exactly
//     const string TRIG_OUTWARD = "Trig_Outward";
//     const string TRIG_INWARD  = "Trig_Inward";
//
//     void Awake()
//     {
//         controller = GetComponent<CharacterController>();
//         animator = GetComponent<Animator>();
//
//         velXHash = Animator.StringToHash("Velocity X");
//         velZHash = Animator.StringToHash("Velocity Z");
//
//         trigOutwardHash = Animator.StringToHash(TRIG_OUTWARD);
//         trigInwardHash  = Animator.StringToHash(TRIG_INWARD);
//
//         if (cameraTransform == null && Camera.main != null)
//             cameraTransform = Camera.main.transform;
//     }
//
//     void Update()
//     {
//         UpdateAttackState();
//
//         // LMB Attack (only if not currently attacking)
//         if (!isAttacking && Input.GetMouseButtonDown(0) && Time.time >= nextAttackTime)
//         {
//             TriggerAttack();
//             nextAttackTime = Time.time + attackCooldown;
//         }
//
//         // INPUT
//         float x = Input.GetAxisRaw("Horizontal");
//         float z = Input.GetAxisRaw("Vertical");
//         bool shift = Input.GetKey(KeyCode.LeftShift);
//
//         // CAMERA-RELATIVE MOVE DIRECTION (WORLD SPACE)
//         Vector3 moveDir;
//         if (cameraTransform != null)
//         {
//             Vector3 camForward = cameraTransform.forward;
//             Vector3 camRight = cameraTransform.right;
//
//             camForward.y = 0f;
//             camRight.y = 0f;
//
//             camForward.Normalize();
//             camRight.Normalize();
//
//             moveDir = camRight * x + camForward * z;
//         }
//         else
//         {
//             moveDir = transform.right * x + transform.forward * z;
//         }
//
//         if (moveDir.sqrMagnitude > 1f)
//             moveDir.Normalize();
//
//         // SPEED RULES (Run forward AND backward with Shift)
//         bool runFB = shift && Mathf.Abs(z) > 0f;                 // Shift + W or Shift + S
//         bool fastStrafe = shift && Mathf.Abs(x) > 0f && z == 0f; // Shift + A/D only (no forward/back)
//
//         float speed = walkSpeed;
//
//         if (runFB)
//             speed = runSpeed;
//         else if (z < 0f)
//             speed = backwardSpeed;
//         else if (fastStrafe)
//             speed = fastStrafeSpeed;
//
//         // MOVE + ROTATE (LOCKED DURING ATTACK)
//         bool onlyStrafing = Mathf.Abs(x) > 0f && Mathf.Abs(z) == 0f;
//
//         if (!isAttacking)
//         {
//             controller.Move(moveDir * speed * Time.deltaTime);
//
//             // Rotation rule:
//             // - If ONLY strafing, face CAMERA forward (so strafe doesn't look backwards)
//             // - Otherwise, face movement direction
//             if (cameraTransform != null && onlyStrafing)
//             {
//                 Vector3 faceDir = cameraTransform.forward;
//                 faceDir.y = 0f;
//
//                 if (faceDir.sqrMagnitude > 0.001f)
//                 {
//                     Quaternion targetRot = Quaternion.LookRotation(faceDir.normalized, Vector3.up);
//                     transform.rotation = Quaternion.Slerp(
//                         transform.rotation,
//                         targetRot,
//                         rotationSpeed * Time.deltaTime
//                     );
//                 }
//             }
//             else if (moveDir.sqrMagnitude > 0.001f)
//             {
//                 Quaternion targetRot = Quaternion.LookRotation(moveDir, Vector3.up);
//                 transform.rotation = Quaternion.Slerp(
//                     transform.rotation,
//                     targetRot,
//                     rotationSpeed * Time.deltaTime
//                 );
//             }
//         }
//
//         // ----- ANIMATOR VALUES (MATCH BLEND TREE) -----
//         // IMPORTANT: Feed the blend tree LOCAL movement so your strafe points match correctly.
//         Vector3 localMove = transform.InverseTransformDirection(moveDir);
//
//         float targetVelZ = 0f;
//         float targetVelX = 0f;
//
//         if (!isAttacking)
//         {
//             // Forward/Backward (Velocity Z)
//             if (Mathf.Abs(z) > 0f)
//             {
//                 float zSign = Mathf.Sign(localMove.z);
//                 targetVelZ = runFB ? 2f * zSign : 0.5f * zSign;
//             }
//
//             // Strafe (Velocity X)
//             if (Mathf.Abs(x) > 0f)
//             {
//                 float xSign = Mathf.Sign(localMove.x);
//
//                 // Use fast strafe values when Shift + A/D only (no forward/back)
//                 targetVelX = fastStrafe ? 2f * xSign : 0.5f * xSign;
//             }
//         }
//
//         animator.SetFloat(velXHash, targetVelX, animDampTime, Time.deltaTime);
//         animator.SetFloat(velZHash, targetVelZ, animDampTime, Time.deltaTime);
//     }
//
//     void TriggerAttack()
//     {
//         animator.ResetTrigger(trigOutwardHash);
//         animator.ResetTrigger(trigInwardHash);
//
//         animator.SetTrigger(nextIsOutward ? trigOutwardHash : trigInwardHash);
//
//         if (alternateSlashes)
//             nextIsOutward = !nextIsOutward;
//     }
//
//     void UpdateAttackState()
//     {
//         if (attackLayerIndex < 0 || attackLayerIndex >= animator.layerCount)
//         {
//             isAttacking = false;
//             return;
//         }
//
//         AnimatorStateInfo s = animator.GetCurrentAnimatorStateInfo(attackLayerIndex);
//         isAttacking = s.IsName(OUTWARD_STATE) || s.IsName(INWARD_STATE);
//
//         // Treat transition into attacks as attacking (prevents tiny movement slips)
//         if (!isAttacking && animator.IsInTransition(attackLayerIndex))
//         {
//             AnimatorStateInfo next = animator.GetNextAnimatorStateInfo(attackLayerIndex);
//             if (next.IsName(OUTWARD_STATE) || next.IsName(INWARD_STATE))
//                 isAttacking = true;
//         }
//     }
// }

