using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManagerUI : MonoBehaviour
{
    [SerializeField] Button hostButton;
    [SerializeField] Button serverButton;
    [SerializeField] Button clientButton;

    private void Awake()
    {
        hostButton.onClick.AddListener(() => { NetworkManager.Singleton.StartHost(); });
        serverButton.onClick.AddListener(() => { NetworkManager.Singleton.StartServer(); });
        clientButton.onClick.AddListener(() => { NetworkManager.Singleton.StartClient(); });
    }

}