using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UniRx;

public class ResourceUIManager : MonoBehaviour
{
    [Header("Text References")]
    [SerializeField] private TextMeshProUGUI wheatText;
    [SerializeField] private TextMeshProUGUI flourText;
    [SerializeField] private TextMeshProUGUI breadText;

    void Start()
    {
        // Bu�day de�i�ikliklerini dinle
        GameManager.Instance.Wheat
            .Subscribe(amount => UpdateWheatUI(amount))
            .AddTo(this);

        // Un de�i�ikliklerini dinle
        GameManager.Instance.Flour
            .Subscribe(amount => UpdateFlourUI(amount))
            .AddTo(this);

        // Ekmek de�i�ikliklerini dinle
        GameManager.Instance.Bread
            .Subscribe(amount => UpdateBreadUI(amount))
            .AddTo(this);
    }

    private void UpdateWheatUI(int amount)
    {
        if (wheatText != null)
        {
            wheatText.text = amount.ToString();
        }
    }

    private void UpdateFlourUI(int amount)
    {
        if (flourText != null)
        {
            flourText.text = amount.ToString();
        }
    }

    private void UpdateBreadUI(int amount)
    {
        if (breadText != null)
        {
            breadText.text = amount.ToString();
        }
    }
}