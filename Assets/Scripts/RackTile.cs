using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class RackTile : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{

    [SerializeField] private GameObject _graphicsRoot;
    [SerializeField] private TextMeshProUGUI _valueText;
    [SerializeField] private GameObject _shadowObject;

    private int _value;

    public int Value => _value;

    public void SetState(int value, bool isActive = true, bool showShadow = true)
    {
        gameObject.SetActive(isActive);

        _value = value;
        _valueText.text = _value.ToString();

        _shadowObject.gameObject.SetActive(showShadow);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _graphicsRoot.SetActive(false);

        RackController.Instance.StartedDraggingTile(this);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector3 dragPosition = Camera.main.ScreenToWorldPoint(eventData.position);
        dragPosition.z = 0;

        RackController.Instance.DraggingTile(this, dragPosition);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _graphicsRoot.SetActive(true);

        RackController.Instance.FinishedDraggingTile(this);
    }

}