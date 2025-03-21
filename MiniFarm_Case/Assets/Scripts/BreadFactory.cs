using UnityEngine;

public class BreadFactory : BaseProductionFacility
{
    [SerializeField] private string factoryVersion = "V1"; 

    protected override float ProductionTime => factoryVersion == "V1" ? 60f : 90f; 
    protected override int MaxCapacity => factoryVersion == "V1" ? 20 : 10; 
    protected override int ResourcePerProduction => factoryVersion == "V1" ? 2 : 1; 
    protected override string ResourceKey => "Flour"; 
    protected override string FactoryKey => $"BreadFactory_{factoryVersion}";

    protected override string ProducedResourceKey => "Bread";

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
            Debug.Log($"Toplanan ekmek miktarý: {collectedAmount}");
            GameManager.Instance.AddResource(ProducedResourceKey, collectedAmount);
        }
    }
}