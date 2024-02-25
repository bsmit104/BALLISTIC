using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SpawnerTester : MonoBehaviour
{

    public GameObject box;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            GameObject boxCopy = Instantiate(box);
            boxCopy.transform.position = Spawner.GetSpawnPoint(); // boxCopy.GetComponent<BoxCollider>().bounds
        }
    }
}
