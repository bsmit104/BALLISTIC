using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dodgeball : MonoBehaviour
{
    private Animator ragdollAnimator;
    private bool isDead = false; // Tracks if ball is "dead" (ball has hit the floor already)

    private void OnCollisionEnter(Collision collider)
    {
        // Check if the collided GameObject has the "Player" tag
        if (!isDead && collider.gameObject.CompareTag("Player"))
        {
            // Activate the ragdoll for the player
            ActivatePlayerRagdoll(collider.gameObject);
        }
        else if (collider.gameObject.CompareTag("isGround"))
        {
            isDead = true;
        }
    }

    private void ActivatePlayerRagdoll(GameObject player)
    {
        // Assuming the ragdoll components are part of the player's hierarchy
        ragdollAnimator = player.GetComponentInChildren<Animator>();
        ragdollAnimator.enabled = false;

        // get the player controller script from the parent object
        PlayerController playerController = player.GetComponent<PlayerController>();
        playerController.enabled = false;

        // get and disable the player's rigidbody and collider
        player.GetComponent<Rigidbody>().isKinematic = true;
        player.GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.Discrete;
        player.GetComponent<CapsuleCollider>().enabled = false;

        // get cinemachine camera and change the look at target to the player's head
        Cinemachine.CinemachineFreeLook freeLook = player.GetComponentInChildren<Cinemachine.CinemachineFreeLook>();

        // find the child in children that is called "LookTargetOnDeath"
        Transform newLookTarget = player.transform.Find("Animated/mixamorig:Hips/LookTargetOnDeath");
        // set the cinemachine look at target to newLookTarget
        freeLook.m_Follow = newLookTarget;
        freeLook.m_LookAt = newLookTarget;

        // ===== Zoom out to have a dramatic effect for the death =====
        // increase the height for the bottom rig/middle rig/top rig
        freeLook.m_Orbits[0].m_Height = Mathf.Lerp(freeLook.m_Orbits[0].m_Height, 2, 2f);
        freeLook.m_Orbits[1].m_Height = Mathf.Lerp(freeLook.m_Orbits[1].m_Height, 4, 2f);
        freeLook.m_Orbits[2].m_Height = Mathf.Lerp(freeLook.m_Orbits[2].m_Height, 6, 2f);

        // increase the radius for the bottom rig/middle rig/top rig
        freeLook.m_Orbits[0].m_Radius = Mathf.Lerp(freeLook.m_Orbits[0].m_Radius, 5, 2f);
        freeLook.m_Orbits[1].m_Radius = Mathf.Lerp(freeLook.m_Orbits[1].m_Radius, 6, 2f);
        freeLook.m_Orbits[2].m_Radius = Mathf.Lerp(freeLook.m_Orbits[2].m_Radius, 7, 2f);
        // =============================================================
    }
}

