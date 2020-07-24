using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// https://www.tantzygames.com/blog/unity-ugui-button-long-press/
/// </summary>
public class ButtonLongPress : Button, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [SerializeField] [Tooltip("How long must pointer be down on this object to trigger a long press")]
    private float holdTime = 1f;

    private bool _held;
    public new UnityEvent onClick = new UnityEvent();

    public UnityEvent onLongPress = new UnityEvent();

    public override void OnPointerDown(PointerEventData eventData)
    {
        _held = false;
        Invoke(nameof(OnLongPress), holdTime);
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        CancelInvoke(nameof(OnLongPress));

        if (!_held)
            onClick.Invoke();
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        CancelInvoke(nameof(OnLongPress));
    }

    private void OnLongPress()
    {
        _held = true;
        onLongPress.Invoke();
    }
}