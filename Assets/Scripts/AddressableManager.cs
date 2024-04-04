using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class AddressableManager : MonoBehaviour
{
    public GameManager gameManager;
    public Button loadingButton;
    public SpriteRenderer fadeBlack;

    void Awake()
    {
        Addressables.LoadAssetAsync<TextAsset>("Assets/Stages/Stage 1.txt").Completed += handle =>
        {
            gameManager.textFile1 = handle.Result;
        };

        Addressables.LoadAssetAsync<TextAsset>("Assets/Stages/Stage 2.txt").Completed += handle =>
        {
            gameManager.textFile2 = handle.Result;
        };
    }

    public void LoadingAsset()
    {
        loadingButton.gameObject.SetActive(false);
        fadeBlack.gameObject.SetActive(false);
        gameManager.StageStart();
    }
}
