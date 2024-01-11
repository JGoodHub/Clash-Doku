using System;
using System.Collections;
using System.Collections.Generic;
using GoodHub.Core.Runtime;
using UnityEngine;

public enum SortingLayer
{
    BOARD,
    RACK,
    MOVING
}

public class SortingLayerHandler : SceneSingleton<SortingLayerHandler>
{
    [SerializeField] private RectTransform _boardItemsRoot;
    [SerializeField] private RectTransform _rackItemsRoot;
    [SerializeField] private RectTransform _movingItemsRoot;

    public void SetSortingLayer(Transform itemTransform, SortingLayer sortingLayer)
    {
        if (sortingLayer == SortingLayer.BOARD && itemTransform.transform.parent != _boardItemsRoot)
            itemTransform.transform.SetParent(_boardItemsRoot, true);

        if (sortingLayer == SortingLayer.RACK && itemTransform.transform.parent != _rackItemsRoot)
            itemTransform.transform.SetParent(_rackItemsRoot, true);

        if (sortingLayer == SortingLayer.MOVING && itemTransform.transform.parent != _movingItemsRoot)
            itemTransform.transform.SetParent(_movingItemsRoot, true);
    }

    public RectTransform GetTransformForSortingLayer(SortingLayer layer)
    {
        switch (layer)
        {
            case SortingLayer.BOARD:
                return _boardItemsRoot;
            case SortingLayer.RACK:
                return _rackItemsRoot;
            case SortingLayer.MOVING:
                return _movingItemsRoot;
            default:
                return null;
        }
    }
}
