using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Launcher : MonoBehaviour
{
    public GameObject dodgeballPrefab;
    public Transform launchDirection;
    public float launchForce = 100f;
    public float launchInterval = 3f;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(ShootDodgeballs());
    }

    private IEnumerator ShootDodgeballs()
    {
        while (true)
        {
            yield return new WaitForSeconds(launchInterval);

            LaunchDodgeball();
        }
    }

    private void LaunchDodgeball()
    {
        GameObject dodgeball = Instantiate(dodgeballPrefab, transform.position, Quaternion.identity);
        Rigidbody rb = dodgeball.GetComponent<Rigidbody>();

        if (rb != null)
        {
            // Set the dodgeball's initial velocity based on the launch direction and force
            rb.velocity = launchDirection.forward * launchForce;
        }
        else
        {
            Debug.LogError("Rigidbody component not found on the dodgeball prefab.");
        }
    }
}
