using Hertzole.GoldPlayer;
using System.Collections.Generic;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class CoreGameManager : MonoBehaviour
{
    public static CoreGameManager Instance { get; private set; }
    public GoldPlayerController playerController;
    public DialogueController dialogueController, warningDialogueController;
    public AudioSource audioSource;
    public int candyCount = 0;
    public int minCandyCountForBucketCam = 0;
    public int poisonBottlesFound = 0;
    public int retryCount = 0;
    public CinemachineBrain brain;

    public bool isDebugMode = false;
    public bool canAccessMemory = false;

    public AudioClip snd_Tutorial;

    public GameStartCutscene startCutscene;

    public List<Color> possibleCandyColors;
    public Stack<Color> colorStack;

    public List<RingDoorBell> houseInfo;
    public List<Transform> bucketPositions;
    public List<Transform> carBucketPositions;
    public GameObject leaveTrigger;

    public List<TextMeshProUGUI> memoryTMPs; 
    public List<int> randomHouseLines;
    public List<RingDoorBell> ringDoorBells;
    public List<string> houseDialouges;

    public List<GameObject> PoisonBottlesParent;

    public List<string> readyToLeaveText;

    public List<Transform> candyObjs;

    public List<Transform> candyHandPos;

    bool testTrigger = false;
    public bool canPause = true;

    public List<Image> memoryCandyImgs;
    public List<Sprite> candySprites;
    public List<Image> memoryPoisonImgs;

    public List<Transform> RecieptParent;

    private void Start()
    {
        // Copy the list and shuffle it
        List<Color> tempList = new List<Color>(possibleCandyColors);
        for (int i = tempList.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            Color temp = tempList[i];
            tempList[i] = tempList[j];
            tempList[j] = temp;
        }

        // Create a stack from the shuffled list
        colorStack = new Stack<Color>(tempList);

        RandomizeHouseInfo();
    }

    private void Update()
    {
        if (isDebugMode && Input.GetKeyDown(KeyCode.T) && !testTrigger)
        {
            testTrigger = true;

            foreach(Transform candy in candyObjs)
            {
                CandyInfo cInfo = candy.GetComponent<CandyInfo>();
                int cID = cInfo.candyID;
                foreach (Transform bt in bucketPositions)
                {
                    var bID = bt.GetComponent<BucketCandySpot>().bucketCandySpotID;
                    if(bID == cID)
                    {
                        candy.gameObject.SetActive(true);
                        candy.transform.SetParent(bt, true);
                        candy.transform.localPosition = Vector3.zero;
                        candy.transform.localRotation = Quaternion.identity;
                        candy.gameObject.layer = LayerMask.NameToLayer("Hands");
                        candy.GetChild(0).gameObject.layer = LayerMask.NameToLayer("Hands");
                    }
                }
            }

            candyCount = 6;
            Debug.Log("MOVED CANDY TO BUCKET");
            ActivateLeaveTrigger();
        }

        if (isDebugMode && Input.GetKeyDown(KeyCode.B))
        {
            foreach(Transform candyT in candyObjs)
            {
                candyT.GetComponent<CandyInfo>().isPoisoned = true;
                Debug.Log("ALL CANDY POISONED");
            }
        }

        if (isDebugMode && Input.GetKeyDown(KeyCode.G))
        {
            foreach (Transform candyT in candyObjs)
            {
                candyT.GetComponent<CandyInfo>().isPoisoned = false;
                Debug.Log("ALL CANDY SAFE");
            }
        }

        if (isDebugMode && Input.GetKeyDown(KeyCode.K))
        {
            if(poisonBottlesFound < 3)
            {
                poisonBottlesFound = 3;
                Debug.Log("ALL POISON BOTTLES FOUND");
            }
            else
            {
                poisonBottlesFound = 0;
                Debug.Log("NO POISON BOTTLES FOUND");
            }
        }

        if (isDebugMode && Input.GetKeyDown(KeyCode.R))
        {
            RevealHints();
        }
    }

    public void MoveCandyToCarBucket()
    {
        foreach(Transform bt in bucketPositions)
        {
            Transform candyTransform = bt.GetChild(0);
            CandyInfo candyInfo = candyTransform.GetComponent<CandyInfo>();

            foreach (Transform bucketTransforms in carBucketPositions)
            {
                var spot_id = bucketTransforms.GetComponent<BucketCandySpot>().bucketCandySpotID;

                if (spot_id == candyInfo.candyID)
                {
                    candyTransform.SetParent(bucketTransforms.transform, false);
                    candyTransform.localPosition = Vector3.zero;
                    candyTransform.localRotation = Quaternion.identity;
                    candyTransform.gameObject.layer = LayerMask.NameToLayer("Default");
                    candyTransform.GetChild(0).gameObject.layer = LayerMask.NameToLayer("Default");
                }
            }
        }
    }

    public void RandomizeHouseInfo()
    {
        // Make a copy of the list so we can shuffle it
        List<RingDoorBell> shuffled = new List<RingDoorBell>(houseInfo);

        // Shuffle the list randomly
        for (int i = 0; i < shuffled.Count; i++)
        {
            RingDoorBell temp = shuffled[i];
            int randomIndex = Random.Range(i, shuffled.Count);
            shuffled[i] = shuffled[randomIndex];
            shuffled[randomIndex] = temp;
        }

        // First 3 are good, rest are bad
        for (int i = 0; i < shuffled.Count; i++)
        {
            bool isGood = i < 3;
            shuffled[i].isGood = isGood;
            shuffled[i].candyInfo.isPoisoned = !isGood;
        }

        //Place Random Candy in correct hand pos
        // Make a copy of the list so we can shuffle it
        List<Transform> candyShuffled = new List<Transform>(candyObjs);

        // Shuffle the list randomly
        for (int i = 0; i < candyShuffled.Count; i++)
        {
            Transform temp = candyShuffled[i];
            int randomIndex = Random.Range(i, candyShuffled.Count);
            candyShuffled[i] = candyShuffled[randomIndex];
            candyShuffled[randomIndex] = temp;
        }

        var count = 0;
        foreach(Transform candyTrans in candyShuffled)
        {
            CandyInfo info = candyTrans.GetComponent<CandyInfo>();
            int _candyID = info.candyID;

            info.isPoisoned = !shuffled[count].isGood;

            Transform handPosTransform = shuffled[count].candyTransform.GetChild(_candyID);

            candyTrans.SetParent(handPosTransform, true);
            candyTrans.localPosition = Vector3.zero;
            candyTrans.localRotation = Quaternion.identity;
            count += 1;
        }

        // Make a unique material instance for this candy
        foreach (RingDoorBell houseInfo in shuffled)
        {
            foreach(Transform t in houseInfo.candyTransform)
            {
                foreach(Transform candyT in t)
                {
                    if (candyT.childCount > 0)
                    {
                        Renderer candyR = candyT.GetChild(0).GetComponent<Renderer>();
                        // Create a new material instance (so only this candy changes)
                        candyR.material = new Material(candyR.materials[0]);

                        // Assign the desired color
                        houseInfo.candyColor = GetNextCandyColor(); // nextColor from your color stack
                        candyR.materials[0].color = houseInfo.candyColor;
                        houseInfo.candyInfo.candyID = candyT.parent.GetSiblingIndex();
                    }
                }
            }
        }

        //ChooseRandomDialogue
        for (int i = 0; i < 6; i++)
        {
            memoryTMPs[i].text = "";
            var randomNum = Random.Range(0, 3);
            randomHouseLines.Add(randomNum);
            bool isHouseNotPoisoned = houseInfo[i].isGood;
            if (isHouseNotPoisoned)
            {
                foreach (string dialogueLine in houseInfo[i].goodDialogue[randomNum].textList)
                    memoryTMPs[i].text += dialogueLine + "\n";
            }
            else
            {
                foreach (string dialogueLine in houseInfo[i].badDialogue[randomNum].textList)
                    memoryTMPs[i].text += dialogueLine + "\n";
            }
        }

        var assignedNewsID = 0;

        int GoodID = 0;
        int BadID = 3;


        //ChooseRandomPoisonBottle and Recipt
        for (int i = 0; i < 6; i++)
        {
            bool isHouseNotPoisoned = houseInfo[i].isGood;
            if (!isHouseNotPoisoned)
            {
                Transform parent = PoisonBottlesParent[i].transform;
                int childCount = parent.childCount;

                if (childCount > 0)
                {
                    // Disable all children first (optional, ensures only one is visible)
                    for (int c = 0; c < childCount; c++)
                        parent.GetChild(c).gameObject.SetActive(false);

                    // Choose a random child to enable
                    int randomIndex = Random.Range(0, childCount);
                    Transform poison = parent.GetChild(randomIndex);
                    poison.gameObject.SetActive(true);
                    poison.GetChild(0).GetChild(0).GetComponent<ReadNews>().newsID = assignedNewsID;
                    assignedNewsID++;

                    int randomSign = Random.Range(0, 2) * 2 - 1;
                    int recieptIndex = randomIndex + randomSign;
                    if (recieptIndex < 0)
                        recieptIndex = 2;
                    if (recieptIndex > 2)
                        recieptIndex = 0;

                    Transform recieptParent = RecieptParent[i].transform;
                    var recieptObj = recieptParent.GetChild(recieptIndex).gameObject;
                    recieptObj.SetActive(true);
                    ReadNews recipetInfo = recieptObj.transform.GetChild(0).GetComponent<ReadNews>();
                    recipetInfo.newsID = BadID;
                    BadID++;
                }
                else
                {
                    Debug.LogWarning($"PoisonBottlesParent[{i}] has no children.");
                }
            }
            else //House is good
            {
                Transform parent = RecieptParent[i].transform;
                int childCount = parent.childCount;

                // Choose a random child to enable
                int randomIndex = Random.Range(0, childCount);
                Transform reciept = parent.GetChild(randomIndex);
                reciept.gameObject.SetActive(true);
                ReadNews recieptInfo = reciept.GetChild(0).GetComponent<ReadNews>();
                recieptInfo.newsID = GoodID;
                GoodID++;
            }
        }

        //Assign Candy images to memory
        for (int i = 0; i < 6; i++)
        {
            memoryCandyImgs[i].sprite = candySprites[houseInfo[i].candyInfo.candyID];
            memoryCandyImgs[i].color = houseInfo[i].candyColor;

            Color poisonImgColor = Color.white;
            if (houseInfo[i].isGood) poisonImgColor.a = 0;
            memoryPoisonImgs[i].color = poisonImgColor;
        }
    }

    public Color GetNextCandyColor()
    {
        if (colorStack.Count == 0)
        {
            Debug.LogWarning("No more colors left in the stack!");
            return Color.white; // fallback color
        }

        return colorStack.Pop(); // removes and returns the top color
    }

    public void ActivateLeaveTrigger()
    {
        leaveTrigger.SetActive(true);

        dialogueController.minPitch = 1.2f;
        dialogueController.maxPitch = 1.3f;
        dialogueController.characterSoundEffects = new List<AudioClip>();
        dialogueController.ShowSubtitles(readyToLeaveText, true);
    }

    void Awake()
    {
        // If an instance already exists and it's not this one → destroy duplicate
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Persist between scenes
    }

    public void TogglePlayerMovement(bool value)
    {
        playerController.Movement.CanMoveAround = value;
    }

    public void TogglePlayerRotation(bool value)
    {
        playerController.Camera.CanLookAround = value;
    }

    public void PlaySoundEffect(AudioClip clip, float volume = 1)
    {
        audioSource.pitch = 1;
        audioSource.PlayOneShot(clip, volume);
    }

    public void CollectPoison()
    {
        poisonBottlesFound += 1;
    }

    public void RevealHints()
    {
        retryCount++;

        if (retryCount > 0)
        {
            foreach (Image candyImg in memoryCandyImgs)
                candyImg.gameObject.SetActive(true);
        }

        if(retryCount > 1)
        {
            foreach (Image image in memoryPoisonImgs)
                image.gameObject.SetActive(true);
        }
    }
}
