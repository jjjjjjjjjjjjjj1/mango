using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using UnityEngine.UI;
using System.Linq;


public class LobbyController : MonoBehaviour
{




    public static string LocalGamePlayerObjectName = "LocalGamePlayer";

    public static LobbyController Instance;

    // UI elements
    public Text LobbyNameText;

    // Player data
    public GameObject PlayerListViewContent;
    public GameObject PlayerListItemPrefab;
    public GameObject LocalPlayerObject;

    // Other data
    public ulong CurrentLobbyID;
    public bool PlayerItemCreated = false;
    private List<PlayerListItem> PlayerListItems = new List<PlayerListItem>();
    public PlayerObjectController LocalPlayerController;

    //Ready
    public Button StartGameButton;
    public Text ReadyButtonText;

    // Manager
    private CustomNetworkManager manager;

    private CustomNetworkManager Manager
    {
        get
        {
            if (manager != null)
            {
                return manager;
            }
            manager = CustomNetworkManager.singleton as CustomNetworkManager;
            return manager;
        }
    }

    private void Awake()
    {
        if (Instance == null) { Instance = this; }
    }


    public void ReadyPlayer()
    {
        LocalPlayerController.ChangeReady();

    }

    public void UpdateButton()
    {
        if (LocalPlayerController.Ready)
        {
            ReadyButtonText.text = "NotReady";
        }
        else
        {
            ReadyButtonText.text = "Ready";
        }
    }


    public void CheckIfCanStart()
    {
        bool canStart = false;
        int check = 0;
        int players = Manager.GamePlayers.Count;


        foreach (PlayerObjectController player in Manager.GamePlayers)
        {
            if (player.Ready)
            {
                check = check + 1;
            }
        }
        if (check >= players / 2)
        {
            canStart = true;
        }
    }








    public void UpdateLobbyName()
    {
        CurrentLobbyID = Manager.GetComponent<SteamLobby>().CurrentLobbyID;
        LobbyNameText.text = SteamMatchmaking.GetLobbyData(new CSteamID(CurrentLobbyID), "name");
    }

    public void UpdatePlayerList()
    {
        if (!PlayerItemCreated)
        {
            CreateHostPlayerItem(); // host player duh
        }
        if (PlayerListItems.Count < Manager.GamePlayers.Count)
        {
            // We have less players in the manager than there are in the real lobby
            // Create a new player
            CreateClientPlayerItem();
        }
        if (PlayerListItems.Count > Manager.GamePlayers.Count)
        {
            // We have more players in the manager than there are in the real lobby
            // Kill player
            CreateClientPlayerItem();
        }
        if (PlayerListItems.Count == Manager.GamePlayers.Count)
        {
            // Same amount of players on the network and in the manager, someone might have changed
            // their profile info (ready up status, name, profile pic,etc.)
            UpdatePlayerItem();
        }
    }

    public void FindLocalPlayer()
    {
        LocalPlayerObject = GameObject.Find(LocalGamePlayerObjectName);
        LocalPlayerController = LocalPlayerObject.GetComponent<PlayerObjectController>();
    }

    private void AddNewPlayerItem(PlayerObjectController player)
    {
        GameObject NewPlayerItem = Instantiate(PlayerListItemPrefab) as GameObject;
        PlayerListItem NewPlayerItemScript = NewPlayerItem.GetComponent<PlayerListItem>();

        NewPlayerItemScript.PlayerName = player.PlayerName;
        NewPlayerItemScript.ConnectionID = player.ConnectionID;
        NewPlayerItemScript.PlayerSteamID = player.PlayerSteamID;
        NewPlayerItemScript.SetPlayerValues();

        NewPlayerItem.transform.SetParent(PlayerListViewContent.transform);
        NewPlayerItem.transform.localScale = Vector3.one;

        PlayerListItems.Add(NewPlayerItemScript);
    }

    public void CreateHostPlayerItem()
    {
        foreach (PlayerObjectController player in Manager.GamePlayers)
        {
            AddNewPlayerItem(player);
        }
        PlayerItemCreated = true;
    }

    private bool HasMatchingPlayer(PlayerObjectController player)
    {
        bool hasMatchingPlayer = PlayerListItems.Any(b => b.ConnectionID == player.ConnectionID);
        return hasMatchingPlayer;
    }

    private bool HasMatchingPlayer(PlayerListItem player)
    {
        bool hasMatchingPlayer = PlayerListItems.Any(b => b.ConnectionID == player.ConnectionID);
        return hasMatchingPlayer;
    }

    public void CreateClientPlayerItem()
    {
        foreach (PlayerObjectController player in Manager.GamePlayers)
        {
            if (!HasMatchingPlayer(player))
            {
                AddNewPlayerItem(player);
            }
        }
    }

    public void UpdatePlayerItem()
    {
        foreach (PlayerObjectController player in Manager.GamePlayers)
        {
            foreach (PlayerListItem PlayerListItemScript in PlayerListItems)
            {
                if (PlayerListItemScript.ConnectionID == player.ConnectionID)
                {
                    // Script that corresponds with the current player we're accessing using the loop
                    PlayerListItemScript.PlayerName = player.PlayerName;
                    PlayerListItemScript.SetPlayerValues();
                }
            }
        }
    }
    public void RemovePlayerItem()
    {
        List<PlayerListItem> playerListItemsToRemove = new List<PlayerListItem>();

        foreach (PlayerListItem playerListItem in PlayerListItems)
        {
            if (!HasMatchingPlayer(playerListItem))
            {
                playerListItemsToRemove.Add(playerListItem);
            }
        }

        if (playerListItemsToRemove.Count > 0)
        {
            foreach (PlayerListItem playerListItem in playerListItemsToRemove)
            {
                GameObject ObjectToRemove = playerListItem.gameObject;
                PlayerListItems.Remove(playerListItem);
                Destroy(ObjectToRemove);
                ObjectToRemove = null;
            }
        }
    }

}
