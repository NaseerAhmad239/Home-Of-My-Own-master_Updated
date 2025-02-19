using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;
using TMPro;
using Unity.VisualScripting;

public class Board : MonoBehaviour
{
    public UI ui;
    public Tutorial ts;
    public WinCondition winCondition;
    public GameObject GamePieceHolder;
    public List<GamePiece> gamePieces = new List<GamePiece>();
    public List<GamePiece> clockWise = new List<GamePiece>();
    public List<GamePiece> Diagnol1 = new List<GamePiece>();
    public List<GamePiece> Diagnol2 = new List<GamePiece>();
    public GamePiece SpawnPiece;
    public Rigidbody Dice;
    private CameraMove cm;
    private SoundController sfx;
    public CardScript.Card RollingForCard = new CardScript.Card();
    public float radius;
    public int spacesToMove;
    public float offsetMultiplier;
    Vector3 startingPos;
    Quaternion startingRotation;

    public Color[] PlayerColorList;

    public Player currentPlayer;

    public GameObject BoardParent;
    public GameObject PlayerPrefab;

    public Inventory GroupInventory = new Inventory();

    public Material ArrowLight;
    public Material ArrowDark;
    public string keepString = "";
    public string discardString = "";
    private int conditionType;

    public class PlayerListType
    {
        public Player playerScript;
        public Transform cameraPos;
    }
    public List<PlayerListType> PlayersList = new List<PlayerListType>();
    public int playerRotationNum;
    int totalPlayers;

    private void Awake()
    {
        sfx = FindObjectOfType<SoundController>();
        GroupInventory.boardScript = this;
        ui.ShowUiLogic();
        int kids = PlayerPrefs.GetInt("Kids");
        int adults = PlayerPrefs.GetInt("Adults");
        totalPlayers = kids + adults;
        string playerType = "";
        for (int i = 0; i < totalPlayers; i++)
        {
            var instantiatedPlayer = Instantiate(PlayerPrefab, BoardParent.transform);
            if (kids <= 0)
            {
                adults--;
                playerType = "_K";
                instantiatedPlayer.GetComponent<Player>().isKidPlayer = true;

            }
            else
            {
                kids--;
                playerType = "_A";
            }
            
            //if (playerType == "_K")
            //{
            //}
            instantiatedPlayer.name = "Player_" + (i + 1) + playerType;

            Color playerColor = PlayerColorList[PlayerPrefs.GetInt("PLAYER " + (i + 1))];
            instantiatedPlayer.GetComponent<Renderer>().material.color = playerColor;

            instantiatedPlayer.GetComponent<Player>().MovePlayer(SpawnPiece);

            PlayersList.Add(new PlayerListType { playerScript = instantiatedPlayer.GetComponent<Player>(), cameraPos = instantiatedPlayer.transform.GetChild(0) });

            if (i == 0)
            {
                currentPlayer = instantiatedPlayer.GetComponent<Player>();
            }

        }
    }
    private void Start()
    {
        InitializeBoard();
        cm = FindObjectOfType<CameraMove>();
        startingPos = Dice.gameObject.transform.position;
        startingRotation = Dice.gameObject.transform.rotation;
    }

    private void InitializeBoard()
    {
        foreach (GamePiece gamePiece in gamePieces)
        {
            gamePiece.position = gamePiece.transform.gameObject.GetComponent<MeshCollider>().bounds.center;
        }
    }
    public void CheckToRoll(string type)
    {
        if (spacesToMove == 0)
        {
            ui.SetRollButton(false);

            switch (type)
            {
                case "Card":
                    ui.CloseCard();
                    break;
                case "Move":
                    break;
            }

            if(type == "Move" && ts.isTutorialStarted)
            {
                type = "Tutorial";
            }
            StartCoroutine(WaitToRoll(type));
        }
    }
    IEnumerator WaitToRoll(string rollType)
    {
        yield return new WaitForSeconds((rollType == "Card") ? .5f : 0);

        cm.MoveCamera(CameraMove.CameraPositions.Dice);

        yield return new WaitForSeconds(cm.cameraMoveTime);

        Vector3 forceDirection = transform.up;
        Vector3 force = forceDirection * 5;
        Dice.AddForce(force, ForceMode.Impulse);

        var time = 1f;
        var targetNum = UnityEngine.Random.Range(1, 7);
        if (!ts.isTutorialStarted)
        {
            targetNum = UnityEngine.Random.Range(1, 7);
        }
        else
        {
            //Debug.Log(ts.stepNumber);
            switch(ts.stepNumber)
            {
                case 2:
                    targetNum = 2;
                    currentPlayer.prevPiece = gamePieces[7];
                    break;
                case 7:
                    currentPlayer.prevPiece = gamePieces[7];
                    targetNum = 3;
                    break;
                case 12:
                    targetNum = 4;
                    break;
            }
        }
        LeanTween.rotate(Dice.gameObject, diceRotations[targetNum - 1], time);

        switch (rollType)
        {
            case "Move":
                sfx.PlaySound(4,1);
                spacesToMove = targetNum;
                ui.SetMoveText(spacesToMove);
                yield return new WaitForSeconds(time + .2f);
                cm.MoveCamera(CameraMove.CameraPositions.Player);
                currentPlayer.ShowArrows(currentPlayer.PlayerIsOn);
                break;
            case "Card":
                sfx.PlaySound(4, 1);
                yield return new WaitForSeconds(time);

                if (ts.isTutorialStarted && ts.stepNumber == 12)
                {
                    ts.IncrementStep();
                }
                else
                {
                    StartCoroutine(CheckCondition(targetNum));
                }
                //cm.MoveCamera(CameraMove.CameraPositions.Player);
                break;
            case "Tutorial":
                sfx.PlaySound(4, 1);
                ts.RolledNumber = targetNum;
                yield return new WaitForSeconds(time + .2f);
                cm.MoveCamera(CameraMove.CameraPositions.Player);
                ts.IncrementStep();

                spacesToMove = targetNum;
                ui.SetMoveText(spacesToMove);

                break;
        }
    }
    public void AddInfiniteCards()
    {
        GroupInventory.AddCard("Food", 100);
        GroupInventory.AddCard("Shelter", 100);
        GroupInventory.AddCard("Clothing", 100);
        GroupInventory.AddCard("WorkSchool", 100);
        GroupInventory.AddCard("FBFP", 100);
    }
    public void isEvenFunction(int targetNum)
    {
        StartCoroutine(CheckCondition(targetNum));

    }
    IEnumerator CheckCondition(int targetNum)
    {
        //bool isEven = (targetNum % 2 == 0) ? true : false;
        //Debug.Log("Even? = " + isEven);
        bool result = ConditionChecking(targetNum);
        if (result)
        {
            sfx.PlaySound(5, 1);
            GroupInventory.AddCard(RollingForCard.Resource.ToString(), 1);
            yield return new WaitForSeconds(1);
            ui.UpdateCardDisplay();
        }
        ui.OpenResultsPanel(RollingForCard, result);
    }


    public void SetCondition()
    {
        System.Random rand = new System.Random();
        int condition = rand.Next(0, 6);
        conditionType = condition;

        switch (conditionType)
        {
            case 0: // Keep if even, discard if odd
                keepString = "Even";
                discardString = "Odd";
                break;

            case 1: // Keep if odd, discard if even
                keepString = "Odd";
                discardString = "Even";
                break;

            case 2: // Keep if 1-3, discard if 4-6
                keepString = "1-3";
                discardString = "4-6";
                break;

            case 3: // Keep if 4-6, discard if 1-3
                keepString = "4-6";
                discardString = "1-3";
                break;

            case 4: // Keep if prime, discard if composite
                keepString = "Prime";
                discardString = "Composite";
                break;

            case 5: // Keep if composite, discard if prime
                keepString = "Composite";
                discardString = "Prime";
                break;

                // Add more conditions here if needed
        }
    }

    public bool ConditionChecking(int targetNum)
    {
        bool result = false;

        switch (conditionType)
        {
            case 0: // Keep if even, discard if odd
                result = targetNum % 2 == 0;
                break;

            case 1: // Keep if odd, discard if even
                result = targetNum % 2 != 0;
                break;

            case 2: // Keep if 1-3, discard if 4-6
                result = targetNum >= 1 && targetNum <= 3;
                break;

            case 3: // Keep if 4-6, discard if 1-3
                result = targetNum >= 4 && targetNum <= 6;
                break;

            case 4: // Keep if prime, discard if composite
                result = IsPrime(targetNum);
                break;

            case 5: // Keep if composite, discard if prime
                result = !IsPrime(targetNum);
                break;

                // Add more conditions here if needed
        }

        Debug.Log($"Keep Condition: {keepString}, Discard Condition: {discardString}");
        
        return result;
    }

    private bool IsPrime(int num)
    {
        //if (num <= 1) return false;
        //if (num == 2) return true;
        //if (num % 2 == 0) return false;
        //for (int i = 3; i <= Mathf.Sqrt(num); i += 2)
        //{
        //    if (num % i == 0) return false;
        //}
        //return true;

        if(num==1 || num==4 || num == 6)
        {
            return false;
        }
        else if(num==2 || num==3 || num == 5)
        {
            return true;
        }
        return true;
    }
    public TMP_Text PlayersTurnDisplay;
    public void SetNextPlayer()
    {
        if (playerRotationNum >= totalPlayers - 1)
        {
            playerRotationNum = -1;
        }
        playerRotationNum++;
        currentPlayer = PlayersList[playerRotationNum].playerScript;
        cm.targets[2].transform = PlayersList[playerRotationNum].cameraPos;
        cm.targets[2].parent = PlayersList[playerRotationNum].cameraPos.parent;
        PlayersTurnDisplay.text = "PLAYER " + (1 + playerRotationNum) + "'s Turn";
    }
    private readonly Vector3[] diceRotations =
    {
        new Vector3(270, 0, 0),     // 1
        new Vector3(0, 0, 0),       // 2
        new Vector3(0, 0, -90),     // 3
        new Vector3(0, 0, 90),      // 4
        new Vector3(180, 0, 0),     // 5
        new Vector3(90, 0, 0)       // 6
    };
    public void CheckReadyToBuy()
    {
        for (int i = 0; i < winCondition.Prices.Length; i++) // For each token
        {
            int am = 0;
            for (int e = 0; e < winCondition.Prices[i].resources.Length; e++) // For each card amount needed
            {
                if (GroupInventory.GetCardAmount(winCondition.Prices[i].resources[e].ToString()) >= 3) // If you have 3 or more
                {
                    am++;
                }
            }

            bool isPurchasable = am >= winCondition.Prices[i].resources.Length; // If all card requirements are met
            winCondition.Tokens[i].isPurchasable = isPurchasable; // Set if isPurcasable or not


            //if (am <= winCondition.Prices[i].resources.Length)
            //{
            //    winCondition.Tokens[i].isPurchasable = true;
            //}
            //else
            //{
            //    winCondition.Tokens[i].isPurchasable = false;
            //}
        }

    }
    public void InitiateTutorial()
    {
        GetComponentInParent<Tutorial>().DialogueBoxObject.SetActive(true);
        GetComponentInParent<Tutorial>().IncrementStep();
    }
}