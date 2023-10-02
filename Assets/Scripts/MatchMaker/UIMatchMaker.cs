using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Matchmaker;
using Unity.Services.Matchmaker.Models;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIMatchMaker : MonoBehaviour
{
    public const string DEFAULT_QUEUE = "default-queue";

    [SerializeField]
    private Transform lookingForMatchTransform;

    private CreateTicketResponse createTicketResponse;
    private float pollTickerTimer;
    private float pollTickerTImerMax = 1.1f;

    private void Awake()
    {
        lookingForMatchTransform.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (createTicketResponse == null)
        {
            return;
        }

        if (pollTickerTimer > 0)
        {
            pollTickerTimer -= Time.deltaTime;

            if (pollTickerTimer <= 0)
            {
                PollMatchmakerTicket();
                pollTickerTimer = pollTickerTImerMax;
            }
        }
    }
    public async void FindMatch()
    {
        Debug.Log("FindMatch");
        lookingForMatchTransform.gameObject.SetActive(true);

        createTicketResponse = await MatchmakerService.Instance.CreateTicketAsync(new List<Player>
        {
            new Player(AuthenticationService.Instance.PlayerId, new MatchmakingPlayerData { Skill = 10 })
        }, new CreateTicketOptions { QueueName = DEFAULT_QUEUE });

        pollTickerTimer = pollTickerTImerMax;
    }

    private async void PollMatchmakerTicket()
    {
        Debug.Log("PollMatchmakerTicker");

        TicketStatusResponse statusResponse = await MatchmakerService.Instance.GetTicketAsync(createTicketResponse.Id);

        if (statusResponse == null)
        {
            Debug.Log("Null means no updates to this ticket, keep waiting");
            return;
        }

        if (statusResponse.Type == typeof(MultiplayAssignment))
        {
            MultiplayAssignment multiplayAssignment = statusResponse.Value as MultiplayAssignment;

            Debug.Log("MultiplayAssignment.Status: " + multiplayAssignment.Status);

            switch (multiplayAssignment.Status)
            {
                case MultiplayAssignment.StatusOptions.Timeout:
                    Debug.Log("Multiplay Timeout!");
                    createTicketResponse = null;
                    lookingForMatchTransform.gameObject.SetActive(false);
                    GetComponent<Button>().interactable = true;
                    break;

                case MultiplayAssignment.StatusOptions.Failed:
                    Debug.LogError("Failed to create Multiplay server. Error: " + multiplayAssignment.Message);

                    createTicketResponse = null;
                    lookingForMatchTransform.gameObject.SetActive(false);
                    GetComponent<Button>().interactable = true;
                    break;

                case MultiplayAssignment.StatusOptions.InProgress:
                    // Waiting...
                    break;

                case MultiplayAssignment.StatusOptions.Found:
                    Debug.Log("Found Match! " + multiplayAssignment.Ip + ", " + multiplayAssignment.Port);

                    createTicketResponse = null;

                    string ipv4Address = multiplayAssignment.Ip;
                    ushort port = (ushort)multiplayAssignment.Port;
                    NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(ipv4Address, port);

                    // Start Client
                    NetworkManager.Singleton.StartClient();

                    SceneManager.LoadScene(1);

                    break;
                default:
                    break;
            }
        }
    }


    [Serializable]
    public struct MatchmakingPlayerData
    {
        public int Skill;
    }
}
