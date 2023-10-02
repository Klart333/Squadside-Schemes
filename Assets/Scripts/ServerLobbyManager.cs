using System;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplay;
using UnityEngine;

public class ServerLobbyManager : Singleton<ServerLobbyManager>
{
    [SerializeField]
    private GameManager gameManagerPrefab;

    private IServerQueryHandler serverQueryHandler;
    private bool alreadyAutoAllocated;
    private float autoAllocateTimer;

    private int playersJoined = 0;

    protected override void Awake()
    {
        base.Awake();

        DontDestroyOnLoad(gameObject);

        InitializeUnityAuthentication();

#if DEDICATED_SERVER
        NetworkManager.Singleton.OnClientConnectedCallback += Singleton_OnClientConnectedCallback;   
#endif
    }

    private async void InitializeUnityAuthentication()
    {
        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            InitializationOptions options = new InitializationOptions();
#if !DEDICATED_SERVER

            options.SetProfile(UnityEngine.Random.Range(0, 10000).ToString());
#endif

            await UnityServices.InitializeAsync(options);

#if !DEDICATED_SERVER

            await AuthenticationService.Instance.SignInAnonymouslyAsync();
#endif

#if DEDICATED_SERVER
            Debug.Log("DEDICATED_SERVER LOBBY");

            MultiplayEventCallbacks multiplayEventCallbacks = new MultiplayEventCallbacks();
            multiplayEventCallbacks.Allocate += MultiplayEventCallbacks_Allocate;
            multiplayEventCallbacks.Deallocate += MultiplayEventCallbacks_Deallocate;
            multiplayEventCallbacks.Error += MultiplayEventCallbacks_Error;
            multiplayEventCallbacks.SubscriptionStateChanged += MultiplayEventCallbacks_SubscriptionStateChanged;
            IServerEvents serverEvents = await MultiplayService.Instance.SubscribeToServerEventsAsync(multiplayEventCallbacks);

            serverQueryHandler = await MultiplayService.Instance.StartServerQueryHandlerAsync(2, "Squad Server", "Ranked", "1.0", "Default");
            ServerConfig serverConfig = MultiplayService.Instance.ServerConfig;
            if (serverConfig.AllocationId != "")
            {
                // Already Allocated
                MultiplayEventCallbacks_Allocate(new MultiplayAllocation("", serverConfig.ServerId, serverConfig.AllocationId));  
            }
#endif
        }
        else
        {
#if DEDICATED_SERVER
            Debug.Log("DEDICATED_SERVER LOBBY - ALREADY INITIALIZED");

            ServerConfig serverConfig = MultiplayService.Instance.ServerConfig;
            if (serverConfig.AllocationId != "")
            {
                // Already Allocated
                MultiplayEventCallbacks_Allocate(new MultiplayAllocation("", serverConfig.ServerId, serverConfig.AllocationId));
            }
#endif
        }
    }
#if DEDICATED_SERVER

    private void Update()
    {
        autoAllocateTimer -= Time.deltaTime;
        if (autoAllocateTimer <= 0f)
        {
            autoAllocateTimer = 99999f;
            MultiplayEventCallbacks_Allocate(null);
        }

        if (serverQueryHandler != null)
        {
            if (NetworkManager.Singleton.IsServer)
            {
                serverQueryHandler.CurrentPlayers = (ushort)NetworkManager.Singleton.ConnectedClientsIds.Count;
            }

            serverQueryHandler.UpdateServerCheck();
        }
    }

    private void MultiplayEventCallbacks_SubscriptionStateChanged(MultiplayServerSubscriptionState obj)
    {
        Debug.Log("DEDICATED_SERVER MultiplayEventCallbacks_SubscriptionStateChanged");

    }

    private void MultiplayEventCallbacks_Error(MultiplayError obj)
    {
        Debug.Log("DEDICATED_SERVER MultiplayEventCallbacks_Error");

    }

    private void MultiplayEventCallbacks_Deallocate(MultiplayDeallocation obj)
    {
        Debug.Log("DEDICATED_SERVER MultiplayEventCallbacks_Deallocate");

    }

    private void MultiplayEventCallbacks_Allocate(MultiplayAllocation obj)
    {
        Debug.Log("DEDICATED_SERVER MultiplayEventCallbacks_Allocate");

        if (alreadyAutoAllocated)
        {
            Debug.Log("Already Auto Allocated");
            return;
        }

        alreadyAutoAllocated = true;

        ServerConfig serverConfig = MultiplayService.Instance.ServerConfig;
        Debug.Log($"Server ID[{serverConfig.ServerId}]");
        Debug.Log($"AllocationID[{serverConfig.AllocationId}]");
        Debug.Log($"Port[{serverConfig.Port}]");
        Debug.Log($"QueryPort[{serverConfig.QueryPort}]");
        Debug.Log($"LogDirectory[{serverConfig.ServerLogDirectory}]");

        string ipv4Address = "0.0.0.0";
        ushort port = serverConfig.Port;
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(ipv4Address, port, "0.0.0.0");

        // Start Server
        NetworkManager.Singleton.StartServer();

        StartServerForPlayers();
    }

    private async void StartServerForPlayers()
    {
        Debug.Log("DEDICATED_SERVER StartServerForPlayers");

        Debug.Log("ReadyServerForPlayersAsync");
        await MultiplayService.Instance.ReadyServerForPlayersAsync();

        Camera.main.enabled = false;
    }

    private void Singleton_OnClientConnectedCallback(ulong obj)
    {
        if (++playersJoined == 2)
        {
            SpawnGameManager();
        }
    }

    private void SpawnGameManager()
    {
        GameManager gameManager = Instantiate(gameManagerPrefab, Vector3.zero, Quaternion.identity);

        NetworkObject networkObject = gameManager.GetComponent<NetworkObject>();
        networkObject.SpawnWithOwnership(0, true);
    }
#endif
}