using System.Collections;
using System.Collections.Generic;
using GoodHub.Core.Runtime;
using Unity.Mathematics;
using UnityEngine;

public class EffectsController : SceneSingleton<EffectsController>
{
    [SerializeField] private GameObject _pointsDingEffectPrefab;


    public PointsEffect CreatePointsDing(int pointsValue, bool prefixPlus, Vector3 position)
    {
        PointsEffect pointsDingEffect = Instantiate(_pointsDingEffectPrefab, position, quaternion.identity, transform).GetComponent<PointsEffect>();
        pointsDingEffect.Initialise(pointsValue, prefixPlus);

        return pointsDingEffect;
    }
}