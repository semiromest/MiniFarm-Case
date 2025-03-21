using System;
using UniRx;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Singleton instance
    public static GameManager Instance { get; private set; }

    // Reactive properties for resources
    private ReactiveProperty<int> wheat = new ReactiveProperty<int>(0);
    private ReactiveProperty<int> flour = new ReactiveProperty<int>(0);
    private ReactiveProperty<int> bread = new ReactiveProperty<int>(0);

    // Public properties to expose reactive properties
    public IReadOnlyReactiveProperty<int> Wheat => wheat;
    public IReadOnlyReactiveProperty<int> Flour => flour;
    public IReadOnlyReactiveProperty<int> Bread => bread;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadResources(); 
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddResource(string resourceKey, int amount)
    {
        switch (resourceKey)
        {
            case "Wheat":
                wheat.Value += amount;
                break;
            case "Flour":
                flour.Value += amount;
                break;
            case "Bread":
                bread.Value += amount;
                break;
            default:
                Debug.LogError($"Geçersiz kaynak anahtarý: {resourceKey}");
                break;
        }
        SaveResources(); 
    }

    public bool RemoveResource(string resourceKey, int amount)
    {
        switch (resourceKey)
        {
            case "Wheat":
                if (wheat.Value >= amount)
                {
                    wheat.Value -= amount;
                    SaveResources(); 
                    return true;
                }
                break;
            case "Flour":
                if (flour.Value >= amount)
                {
                    flour.Value -= amount;
                    SaveResources();
                    return true;
                }
                break;
            case "Bread":
                if (bread.Value >= amount)
                {
                    bread.Value -= amount;
                    SaveResources();
                    return true;
                }
                break;
            default:
                Debug.LogError($"Geçersiz kaynak anahtarý: {resourceKey}");
                break;
        }
        return false;
    }

    public bool HasResource(string resourceKey, int amount)
    {
        switch (resourceKey)
        {
            case "Wheat":
                return wheat.Value >= amount;
            case "Flour":
                return flour.Value >= amount;
            case "Bread":
                return bread.Value >= amount;
            default:
                Debug.LogError($"Geçersiz kaynak anahtarý: {resourceKey}");
                return false;
        }
    }

    // Example method to convert wheat to flour
    public void ConvertWheatToFlour(int wheatAmount, int flourAmount)
    {
        if (wheat.Value >= wheatAmount)
        {
            RemoveResource("Wheat", wheatAmount);
            AddResource("Flour", flourAmount);
        }
        else
        {
            Debug.Log("Not enough wheat to convert to flour.");
        }
    }

    // Example method to convert flour to bread
    public void ConvertFlourToBread(int flourAmount, int breadAmount)
    {
        if (flour.Value >= flourAmount)
        {
            RemoveResource("Flour", flourAmount);
            AddResource("Bread", breadAmount);
        }
        else
        {
            Debug.Log("Not enough flour to convert to bread.");
        }
    }

    // Save resources to PlayerPrefs
    private void SaveResources()
    {
        PlayerPrefs.SetInt("Wheat", wheat.Value);
        PlayerPrefs.SetInt("Flour", flour.Value);
        PlayerPrefs.SetInt("Bread", bread.Value);
        PlayerPrefs.Save(); 
    }

    // Load resources from PlayerPrefs
    private void LoadResources()
    {
        wheat.Value = PlayerPrefs.GetInt("Wheat", 0);
        flour.Value = PlayerPrefs.GetInt("Flour", 0);
        bread.Value = PlayerPrefs.GetInt("Bread", 0);
    }
}