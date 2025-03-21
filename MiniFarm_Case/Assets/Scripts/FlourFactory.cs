using UnityEngine;

public class FlourFactory : BaseProductionFacility
{
    protected override float ProductionTime => 40f; 
    protected override int MaxCapacity => 10; // Maksimum kapasite 10
    protected override int ResourcePerProduction => 1; 
    protected override string ResourceKey => "Wheat"; 
    protected override string FactoryKey => "FlourFactory";

    protected override string ProducedResourceKey => "Flour";

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
            Debug.Log($"Toplanan un miktarı: {collectedAmount}");
            GameManager.Instance.AddResource(ProducedResourceKey, collectedAmount);
        }
    }
}