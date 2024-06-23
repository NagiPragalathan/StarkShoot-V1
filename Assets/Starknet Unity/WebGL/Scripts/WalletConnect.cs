using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Globalization;
using System.Numerics;

public class WalletConnect : MonoBehaviour
{
    public static string playerAddress;

    private IEnumerator ConnectWalletAsync(Action connectWalletFunction)
    {
        bool isConnected = false;
        string errorMessage = string.Empty;

        // Call the JavaScript method to connect the wallet
        connectWalletFunction();

        // Wait for the connection to be established
        yield return new WaitUntil(() => {
            try
            {
                isConnected = JSInteropManager.IsConnected();
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }
            return isConnected || !string.IsNullOrEmpty(errorMessage);
        });

        if (!string.IsNullOrEmpty(errorMessage))
        {
            Debug.LogError("Error connecting wallet: " + errorMessage);
            yield break;
        }

        playerAddress = JSInteropManager.GetAccount();
        PlayerPrefs.SetString("playerAddress", playerAddress);
        Debug.Log("Connected to wallet: " + playerAddress);

        // Load the Start scene after connection
        SceneManager.LoadScene("Start");

        // Get the wallet balance
        StartCoroutine(GetWalletBalance(playerAddress));
    }

    public void OnButtonConnectWalletArgentX()
    {
        StartCoroutine(ConnectWalletAsync(JSInteropManager.ConnectWalletArgentX));
    }

    public void OnButtonConnectWalletBraavos()
    {
        StartCoroutine(ConnectWalletAsync(JSInteropManager.ConnectWalletBraavos));
    }

    // Start is called before the first frame update
    void Start()
    {
        if (PlayerPrefs.HasKey("playerAddress"))
        {
            playerAddress = PlayerPrefs.GetString("playerAddress");
            Debug.Log("Connected to wallet: " + playerAddress);
        }
        else
        {
            bool available = JSInteropManager.IsWalletAvailable();
            if (!available)
            {
                JSInteropManager.AskToInstallWallet();
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    private IEnumerator GetWalletBalance(string userAddress)
    {
        string contractAddress = "CONTRACT_ADDRESS"; // Replace with your contract address
        string[] calldata = new string[] { userAddress };
        string calldataString = JsonUtility.ToJson(new ArrayWrapper { array = calldata });
        
        // Call the smart contract to get the balance
        JSInteropManager.CallContract(contractAddress, "balanceOf", calldataString, "WalletConnect", "OnBalanceCallback");

        yield return null;
    }

    public void OnBalanceCallback(string response)
    {
        JsonResponse jsonResponse = JsonUtility.FromJson<JsonResponse>(response);
        BigInteger balance = BigInteger.Parse(jsonResponse.result[0].Substring(2), NumberStyles.HexNumber);
        Debug.Log("Wallet balance: " + balance);
    }
}

[Serializable]
public class ArrayWrapper
{
    public string[] array;
}

[Serializable]
public class JsonResponse
{
    public string[] result;
}
