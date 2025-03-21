using UnityEngine;
using UniRx;

public class HayFactory : BaseProductionFacility
{
    protected override float ProductionTime => 20f; 
    protected override int MaxCapacity => 5; // Maksimum kapasite 5
    protected override int ResourcePerProduction => 0; // Hammadde gerektirmez
    protected override string ResourceKey => "Wheat";   
    protected override string FactoryKey => "HayFactory"; 

    protected override void Start()
    {
        base.Start();
        StartProduction();
    }

    protected override void Update()
    {
        // HAYFACTORY UI KAPATMA İPTALİ
    }

    protected override void StartProduction()
    {
        if (_currentResourceAmount.Value >= MaxCapacity)
        {
            Debug.Log("MaxCapacity aşıldı, üretim durduruldu.");
            _isProducing.Value = false; // Üretimi durdur
            return;
        }

        _productionDisposable?.Dispose();
        _isProducing.Value = true;

        _productionDisposable = Observable.EveryUpdate()
            .TakeWhile(_ => _isProducing.Value)
            .Subscribe(_ =>
            {
                // MaxCapacity kontrolü (her frame'de kontrol et)
                if (_currentResourceAmount.Value >= MaxCapacity)
                {
                    Debug.Log("MaxCapacity aşıldı, üretim durduruldu.");
                    _isProducing.Value = false; 
                    _productionDisposable?.Dispose(); 
                    return;
                }

                // Zamanlayıcıyı güncelle
                _productionTimer += Time.deltaTime;
                _productionProgress.Value = _productionTimer / ProductionTime;
                _remainingTime.Value = ProductionTime - _productionTimer;

                // Üretim süresi doldu mu kontrol et
                if (_productionTimer >= ProductionTime)
                {
                    _currentResourceAmount.Value++;
                    Debug.Log($"Buğday üretildi! Mevcut miktar: {_currentResourceAmount.Value}");

                    // Zamanlayıcıyı sıfırla
                    _productionTimer = 0f;
                    _productionProgress.Value = 0f;
                    _remainingTime.Value = ProductionTime;

                    _productionDisposable.Dispose();

                    // MaxCapacity kontrolü (üretim tamamlandıktan sonra)
                    if (_currentResourceAmount.Value < MaxCapacity)
                    {
                        StartProduction(); // Yeni üretim başlat
                    }
                    else
                    {
                        Debug.Log("MaxCapacity aşıldı, üretim durduruldu.");
                        _isProducing.Value = false; // Üretimi durdur
                    }
                }
            })
            .AddTo(this);
    }

    protected override void OnMouseDown()
    {
        if (!_isPanelOpen.Value)
        {
            _isPanelOpen.Value = true;
        }
        else if (_currentResourceAmount.Value > 0)
        {
            int collectedAmount = _currentResourceAmount.Value;
            _currentResourceAmount.Value = 0;
            Debug.Log($"Toplanan buğday miktarı: {collectedAmount}");
            GameManager.Instance.AddResource(ResourceKey, collectedAmount);

            // Buğday toplandıktan sonra üretimi yeniden başlat
            if (!_isProducing.Value && _currentResourceAmount.Value < MaxCapacity)
            {
                StartProduction();
            }
        }
    }
}