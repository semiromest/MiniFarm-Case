using System;
using UnityEngine;
using TMPro;
using UniRx;
using UnityEngine.UI;

public abstract class BaseProductionFacility : MonoBehaviour, ProductionPanelUI.IProductionFacility
{
    [Header("UI Elements")]
    [SerializeField] protected TextMeshProUGUI resourceAmountText;
    [SerializeField] protected TextMeshProUGUI countdownTimerText;
    [SerializeField] protected TextMeshProUGUI queueCountText;
    [SerializeField] protected Slider productionProgressSlider;
    [SerializeField] protected GameObject productionPanel;
    [SerializeField] protected Button addToQueueButton;
    [SerializeField] protected Button removeFromQueueButton;

    protected ReactiveProperty<int> _currentResourceAmount = new ReactiveProperty<int>(0);
    protected ReactiveProperty<float> _productionProgress = new ReactiveProperty<float>(0f);
    protected ReactiveProperty<float> _remainingTime = new ReactiveProperty<float>(0f);
    protected ReactiveProperty<bool> _isProducing = new ReactiveProperty<bool>(false);
    protected ReactiveProperty<bool> _isPanelOpen = new ReactiveProperty<bool>(false);

    protected ReactiveCollection<bool> _productionQueue = new ReactiveCollection<bool>();
    public ReactiveCollection<bool> ProductionQueue => _productionQueue;

    protected IDisposable _productionDisposable;
    protected float _productionTimer = 0f;

    protected abstract float ProductionTime { get; }
    protected abstract int MaxCapacity { get; }
    protected abstract int ResourcePerProduction { get; }
    protected abstract string ResourceKey { get; } 
    protected abstract string FactoryKey { get; }

    protected virtual string ProducedResourceKey => ResourceKey;

    protected virtual void Start()
    {
        LoadGameData();
        SetupUIBindings();
        UpdateButtonStates();
        ResumeProductionAfterLoad();

        if (productionPanel != null)
        {
            productionPanel.SetActive(false);
        }
    }

    protected virtual void Update()
    {
        if (_isPanelOpen.Value && Input.GetMouseButtonDown(0))
        {
            bool clickedOnUI = UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();

            if (!clickedOnUI && !IsMouseOverFactory())
            {
                _isPanelOpen.Value = false;
                Debug.Log("UI dýþýnda bir yere týklandý, panel kapatýldý.");
            }
        }
    }

    private bool IsMouseOverFactory()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.gameObject == gameObject)
            {
                return true;
            }
        }

        return false;
    }

    protected virtual void OnApplicationQuit()
    {
        SaveGameData();
    }

    protected virtual void OnDestroy()
    {
        _productionDisposable?.Dispose();
        if (addToQueueButton != null) addToQueueButton.onClick.RemoveAllListeners();
        if (removeFromQueueButton != null) removeFromQueueButton.onClick.RemoveAllListeners();
    }

    protected virtual void SetupUIBindings()
    {
        _currentResourceAmount.Subscribe(amount =>
        {
            UpdateResourceAmountText(amount);
            UpdateButtonStates();
        }).AddTo(this);

        _productionProgress.Subscribe(progress =>
        {
            UpdateProgressSlider(progress);
        }).AddTo(this);

        _remainingTime.Subscribe(time =>
        {
            UpdateCountdownTimer(time);
        }).AddTo(this);

        _isPanelOpen.Subscribe(isOpen =>
        {
            if (productionPanel != null)
            {
                productionPanel.SetActive(isOpen);
            }
        }).AddTo(this);

        _productionQueue.ObserveCountChanged().Subscribe(count =>
        {
            UpdateQueueCountText(count);
            UpdateButtonStates();
            if (!_isProducing.Value && count > 0) ProcessNextInQueue();
        }).AddTo(this);

        if (addToQueueButton != null) addToQueueButton.onClick.AddListener(AddProductionToQueue);
        if (removeFromQueueButton != null) removeFromQueueButton.onClick.AddListener(RemoveProductionFromQueue);
    }

    protected virtual void UpdateResourceAmountText(int amount)
    {
        if (resourceAmountText != null) resourceAmountText.text = amount.ToString();
    }

    protected virtual void UpdateProgressSlider(float progress)
    {
        if (productionProgressSlider != null) productionProgressSlider.value = progress;
    }

    protected virtual void UpdateCountdownTimer(float remainingSeconds)
    {
        if (countdownTimerText != null)
        {
            int minutes = Mathf.FloorToInt(remainingSeconds / 60);
            int seconds = Mathf.FloorToInt(remainingSeconds % 60);
            countdownTimerText.text = $"{minutes}:{seconds:00}";
        }
    }

    protected virtual void UpdateQueueCountText(int count)
    {
        if (queueCountText != null) queueCountText.text = $"{count}/{MaxCapacity}";
    }

    protected virtual void UpdateButtonStates()
    {
        if (addToQueueButton != null)
        {
            bool canAddProduction = GameManager.Instance.HasResource(ResourceKey, ResourcePerProduction) &&
                                   (_currentResourceAmount.Value + _productionQueue.Count) < MaxCapacity;
            addToQueueButton.interactable = canAddProduction;
        }

        if (removeFromQueueButton != null)
        {
            bool canRemoveProduction = _productionQueue.Count > 0;
            removeFromQueueButton.interactable = canRemoveProduction;
        }
    }

    public virtual void AddProductionToQueue()
    {
        if (_currentResourceAmount.Value + _productionQueue.Count >= MaxCapacity)
        {
            Debug.Log("Kapasite dolu, daha fazla üretim emri eklenemiyor!");
            return;
        }

        // Kaynak harca
        if (GameManager.Instance.RemoveResource(ResourceKey, ResourcePerProduction))
        {
            _productionQueue.Add(true);
            Debug.Log($"Üretim sýraya eklendi! Sýra uzunluðu: {_productionQueue.Count}");
            UpdateButtonStates();
        }
        else
        {
            Debug.Log($"Yeterli {ResourceKey} yok!");
        }
    }

    public virtual void RemoveProductionFromQueue()
    {
        if (_productionQueue.Count == 0)
        {
            Debug.Log("Sýrada üretim yok!");
            return;
        }

        // Harcanan kaynaðý geri ver
        GameManager.Instance.AddResource(ResourceKey, ResourcePerProduction);
        _productionQueue.RemoveAt(_productionQueue.Count - 1);
        Debug.Log($"Üretim sýradan çýkarýldý! Sýra uzunluðu: {_productionQueue.Count}");
        UpdateButtonStates(); 
    }

    protected virtual void ProcessNextInQueue()
    {
        if (_productionQueue.Count == 0) return;
        _isProducing.Value = true;
        _productionTimer = 0f;
        _productionProgress.Value = 0f;
        _remainingTime.Value = ProductionTime;
        _productionQueue.RemoveAt(0);
        StartProduction();
    }

    protected virtual void StartProduction()
    {
        _productionDisposable?.Dispose();
        _productionDisposable = Observable.EveryUpdate()
            .TakeWhile(_ => _isProducing.Value)
            .Subscribe(_ =>
            {
                _productionTimer += Time.deltaTime;
                _productionProgress.Value = _productionTimer / ProductionTime;
                _remainingTime.Value = ProductionTime - _productionTimer;

                if (_productionTimer >= ProductionTime)
                {
                    _currentResourceAmount.Value++;
                    Debug.Log($"{ProducedResourceKey} üretildi! Mevcut miktar: {_currentResourceAmount.Value}");
                    _isProducing.Value = false;
                    _productionTimer = 0f;
                    _productionProgress.Value = 0f;
                    _remainingTime.Value = ProductionTime;
                    _productionDisposable.Dispose();
                    if (_productionQueue.Count > 0) ProcessNextInQueue();
                }
            })
            .AddTo(this);
    }

    protected virtual void OnMouseDown()
    {
        if (!_isPanelOpen.Value)
        {
            _isPanelOpen.Value = true;
        }
        else if (_currentResourceAmount.Value > 0)
        {
            int collectedAmount = _currentResourceAmount.Value;
            _currentResourceAmount.Value = 0;
            Debug.Log($"Toplanan {ProducedResourceKey} miktarý: {collectedAmount}");
            GameManager.Instance.AddResource(ProducedResourceKey, collectedAmount);
        }
    }

    protected virtual void SaveGameData()
    {
        SaveManager.SaveInt($"{FactoryKey}_{ProducedResourceKey}Amount", _currentResourceAmount.Value);
        SaveManager.SaveFloat($"{FactoryKey}_ProductionTimer", _productionTimer);
        SaveManager.SaveInt($"{FactoryKey}_QueueCount", _productionQueue.Count);
        SaveManager.SaveInt($"{FactoryKey}_IsProducing", _isProducing.Value ? 1 : 0);
        SaveManager.SaveDateTime($"{FactoryKey}_LastSaveTime", DateTime.Now);
        Debug.Log($"{FactoryKey} verileri kaydedildi.");
    }

    protected virtual void LoadGameData()
    {
        _currentResourceAmount.Value = SaveManager.LoadInt($"{FactoryKey}_{ProducedResourceKey}Amount", 0);
        _productionTimer = SaveManager.LoadFloat($"{FactoryKey}_ProductionTimer", 0f);
        SetProductionQueue(SaveManager.LoadInt($"{FactoryKey}_QueueCount", 0));
        _isProducing.Value = SaveManager.LoadInt($"{FactoryKey}_IsProducing", 0) == 1;
        SimulateOfflineProduction((float)(DateTime.Now - SaveManager.LoadDateTime($"{FactoryKey}_LastSaveTime", DateTime.Now)).TotalSeconds);
        Debug.Log($"{FactoryKey} verileri yüklendi.");
    }

    protected virtual void SetProductionQueue(int queueCount)
    {
        _productionQueue.Clear();
        for (int i = 0; i < queueCount; i++) _productionQueue.Add(true);
    }

    protected virtual void SimulateOfflineProduction(float elapsedSeconds)
    {
        bool wasProducing = SaveManager.LoadInt($"{FactoryKey}_IsProducing", 0) == 1;
        float savedProductionTimer = SaveManager.LoadFloat($"{FactoryKey}_ProductionTimer", 0f);

        if (wasProducing)
        {
            float remainingProductionTime = ProductionTime - savedProductionTimer;
            if (elapsedSeconds >= remainingProductionTime)
            {
                _currentResourceAmount.Value++;
                elapsedSeconds -= remainingProductionTime;
                _isProducing.Value = false;
                _productionTimer = 0f;
                _productionProgress.Value = 0f;
                _remainingTime.Value = ProductionTime;

                int additionalProductions = Mathf.FloorToInt(elapsedSeconds / ProductionTime);
                int possibleProductions = Math.Min(additionalProductions, _productionQueue.Count);
                _currentResourceAmount.Value += possibleProductions;
                for (int i = 0; i < possibleProductions; i++) _productionQueue.RemoveAt(0);
                elapsedSeconds -= (possibleProductions * ProductionTime);

                if (_productionQueue.Count > 0 && elapsedSeconds > 0)
                {
                    _productionQueue.RemoveAt(0);
                    _isProducing.Value = true;
                    _productionTimer = Math.Min(elapsedSeconds, ProductionTime);
                    _productionProgress.Value = _productionTimer / ProductionTime;
                    _remainingTime.Value = ProductionTime - _productionTimer;
                }
            }
            else
            {
                _isProducing.Value = true;
                _productionTimer = savedProductionTimer + elapsedSeconds;
                _productionProgress.Value = _productionTimer / ProductionTime;
                _remainingTime.Value = ProductionTime - _productionTimer;
            }
        }
        else if (_productionQueue.Count > 0 && elapsedSeconds > 0)
        {
            int possibleProductions = Mathf.FloorToInt(elapsedSeconds / ProductionTime);
            int completedProductions = Math.Min(possibleProductions, _productionQueue.Count);
            _currentResourceAmount.Value += completedProductions;
            for (int i = 0; i < completedProductions; i++) _productionQueue.RemoveAt(0);
            elapsedSeconds -= (completedProductions * ProductionTime);

            if (_productionQueue.Count > 0 && elapsedSeconds > 0)
            {
                _productionQueue.RemoveAt(0);
                _isProducing.Value = true;
                _productionTimer = Math.Min(elapsedSeconds, ProductionTime);
                _productionProgress.Value = _productionTimer / ProductionTime;
                _remainingTime.Value = ProductionTime - _productionTimer;
            }
        }
    }

    public virtual void ResumeProductionAfterLoad()
    {
        if (_isProducing.Value) StartProduction();
        else if (_productionQueue.Count > 0) ProcessNextInQueue();
    }
}