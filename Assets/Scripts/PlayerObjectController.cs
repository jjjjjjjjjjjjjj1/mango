using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Steamworks;
using UnityEngine.SceneManagement;

public class PlayerObjectController : NetworkBehaviour
{
    //Player Data           
    [SyncVar] public int ConnectionID;
    [SyncVar] public int PlayerIdNumber;
    [SyncVar] public ulong PlayerSteamID;
    [SyncVar(hook = nameof(PlayerNameUpdate))] public string PlayerName;
    [SyncVar(hook = nameof(PlayerReadyUpdate))] public bool Ready;
    private CustomNetworkManager manager;
    public Camera Camera;
    public AudioListener AudioListener;

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

    private void Start()
    {
        DontDestroyOnLoad(this.gameObject);
        SceneManager.activeSceneChanged += OnSceneChanged;
        ManageCamera();
    }

    private void ManageCamera()
    {
        if (SceneManager.GetActiveScene().name == "Game")
        {
            if (hasAuthority)
            {
                Camera.enabled = true;
                AudioListener.enabled = true;
            }
            else
            {
                Camera.enabled = false;
                AudioListener.enabled = false;
            }
        }
    }

    private void OnSceneChanged(Scene current, Scene next)
    {
        ManageCamera();
    }

    private void PlayerReadyUpdate(bool oldValue, bool newValue)
    {
        if (isServer)
        {
            this.Ready = newValue;
        }
        if (isClient)
        {
            LobbyController.Instance.UpdatePlayerList();
        }
    }

    [Command]
    private void CmdSetPlayerReady()
    {
        this.PlayerReadyUpdate(this.Ready, !this.Ready);
    }

    public void ChangeReady()
    {
        if (hasAuthority)
        {
            CmdSetPlayerReady();
        }
    }


    public override void OnStartAuthority()
    {
        CmdSetPlayerName(SteamFriends.GetPersonaName().ToString());
        gameObject.name = LobbyController.LocalGamePlayerObjectName;
        LobbyController.Instance.FindLocalPlayer();
        LobbyController.Instance.UpdateLobbyName();
    }

    public override void OnStartClient()
    {
        Manager.GamePlayers.Add(this);
        LobbyController.Instance.UpdateLobbyName();
        LobbyController.Instance.UpdatePlayerList();
    }

    public override void OnStopClient()
    {
        Manager.GamePlayers.Remove(this);
        LobbyController.Instance.UpdatePlayerList();

    }

    [Command]
    private void CmdSetPlayerName(string PlayerName)
    {
        this.PlayerNameUpdate(this.PlayerName, PlayerName);
    }


    public void PlayerNameUpdate(string OldValue, string NewValue)
    {
        if (isServer)
        {
            this.PlayerName = NewValue;
        }
        if (isClient)
        {
            LobbyController.Instance.UpdatePlayerList();
        }
    }

    // Start game

    public void CanStartGame(string SceneName)
    {
        if (hasAuthority)
        {
            CmdCanStartGame(SceneName);
        }
    }

    [Command]
    private void CmdCanStartGame(string SceneName)
    {
        manager.StartGame(SceneName);
    }
}
