using UnityEngine;

public class CheckBucket : MonoBehaviour
{
    public CoreGameManager gameManager;
    public GameObject bucketCam;

    // Update is called once per frame
    void Update()
    {
        if (gameManager.candyCount < gameManager.minCandyCountForBucketCam) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            bucketCam.SetActive(true);
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            bucketCam.SetActive(false);
        }
    }
}
