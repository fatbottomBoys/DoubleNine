using System;
using UnityEngine;
using UnityEngine.UIElements;

public class MultiplayerUI : MonoBehaviour
{

    [SerializeField]
    private UIDocument m_uidocument;
    [SerializeField]
    private Button m_hostButton, m_clientButton, m_clientDisconnect;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public event Action OnStartHost, OnStartClient, OnDisconnectClient;
    private void Awake()
    {
        m_hostButton = m_uidocument.rootVisualElement.Q<Button>("ButtonHost");
        m_clientButton = m_uidocument.rootVisualElement.Q<Button>("ButtonClient");
        m_clientDisconnect = m_uidocument.rootVisualElement.Q<Button>("ButtonDisconnect");
    }

    // Update is called once per frame
    private void Start()
    {
        m_hostButton.clicked += () => OnStartHost?.Invoke();
        m_clientButton.clicked += () => OnStartClient?.Invoke();
        m_clientDisconnect.clicked += () => OnDisconnectClient?.Invoke();
        EnableButtons();
    }

    public void DisableButtons() 
    {
        m_hostButton.SetEnabled(false);
        m_clientButton.SetEnabled(false);
        m_clientDisconnect.SetEnabled(true);
    }

    public void EnableButtons()
    {
        m_hostButton.SetEnabled(true);
        m_clientButton.SetEnabled(true);
        m_clientDisconnect.SetEnabled(false);
    }
}
