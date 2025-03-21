using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UniRx;
using System;

public class ProductionPanelUI : MonoBehaviour
{
    public interface IProductionFacility
    {
        ReactiveCollection<bool> ProductionQueue { get; }
    }

    [SerializeField] private MonoBehaviour productionFacilityComponent;
    [SerializeField] private Button addToQueueButton;
    [SerializeField] private Button removeFromQueueButton;
    [SerializeField] private TextMeshProUGUI queueStatusText;
    
    [SerializeField] private Image addButtonImage;
    [SerializeField] private Image removeButtonImage;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

    private IProductionFacility _productionFacility;
    private ReactiveCollection<bool> _productionQueue;

    private void Awake()
    {
        if (productionFacilityComponent != null)
        {
            if (productionFacilityComponent is IProductionFacility)
            {
                _productionFacility = productionFacilityComponent as IProductionFacility;
            }
            else
            {
                Debug.LogError($"{productionFacilityComponent.GetType().Name} sýnýfý IProductionFacility arayüzünü uygulamýyor!");
            }
        }
        else
        {
            var parentComponents = GetComponentsInParent<MonoBehaviour>();
            foreach (var component in parentComponents)
            {
                if (component is IProductionFacility)
                {
                    _productionFacility = component as IProductionFacility;
                    productionFacilityComponent = component;
                    break;
                }
            }
        }

        if (_productionFacility == null)
        {
            Debug.LogError("Hiçbir üretim tesisi bulunamadý! IProductionFacility arayüzünü uygulayan bir bileþen atayýn.");
            enabled = false;
            return;
        }

        _productionQueue = _productionFacility.ProductionQueue;
    }

    private void Start()
    {
        gameObject.SetActive(false);

        //_productionQueue.ObserveCountChanged()
            //.Subscribe(count => UpdateQueueStatus(count))
            //.AddTo(this);

        Observable.EveryUpdate()
            .Subscribe(_ => UpdateButtonVisuals())
            .AddTo(this);
    }

    private void UpdateQueueStatus(int count)
    {
        if (queueStatusText != null)
        {
            queueStatusText.text = $"Sýradaki Üretim: {count}";
        }
    }

    private void UpdateButtonVisuals()
    {
        if (addButtonImage != null && addToQueueButton != null)
        {
            addButtonImage.color = addToQueueButton.interactable ? normalColor : disabledColor;
        }
        if (removeButtonImage != null && removeFromQueueButton != null)
        {
            removeButtonImage.color = removeFromQueueButton.interactable ? normalColor : disabledColor;
        }
    }

    public void SetActive(bool isActive)
    {
        gameObject.SetActive(isActive);
    }

    public bool IsActive()
    {
        return gameObject.activeInHierarchy;
    }

    public void SetProductionFacility(MonoBehaviour facility)
    {
        if (facility is IProductionFacility)
        {
            productionFacilityComponent = facility;
            _productionFacility = facility as IProductionFacility;
            _productionQueue = _productionFacility.ProductionQueue;
        }
        else
        {
            Debug.LogError($"{facility.GetType().Name} sýnýfý IProductionFacility arayüzünü uygulamýyor!");
        }
    }
}