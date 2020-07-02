using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// @author Philipp Bönsch
/// controls the editing of a planet
/// </summary>
public class EditPlanetController : MonoBehaviour
{
    public Planet SelectedPlanet { get; set; }

    [SerializeField] private GameObject editPane;

    [SerializeField] private Button doneButton;
    [SerializeField] private Button initVelButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button velEditDoneButton;

    [SerializeField] private GameObject colorWheel;
    [SerializeField] private Image colorImage;

    [SerializeField] private Slider radiusSlider;
    [SerializeField] private Text radiusVal;

    private Color _prevColor;
    private float _prevRadius;
    private Vector3 _prevVel;

    private Texture2D _colorWheelTex;
    private CircleCollider2D _colorWheelCollider;
    private RectTransform _colorWheelTransform;

    private bool _setVelocityMode;

    private void Start()
    {
        _prevColor = SelectedPlanet.color;
        _prevRadius = SelectedPlanet.radius;
        _prevVel = SelectedPlanet.initialVelocity;

        radiusSlider.value = 0.5f;
        radiusVal.text = $"{_prevRadius}";
        colorImage.color = _prevColor;
        _colorWheelTex = colorWheel.GetComponent<Image>().sprite.texture;
        _colorWheelCollider = colorWheel.GetComponent<CircleCollider2D>();
        _colorWheelTransform = colorWheel.GetComponent<RectTransform>();

        // initialization of all buttons in the edit pane
        initVelButton.onClick.AddListener(() =>
        {
            editPane.SetActive(false);
            _setVelocityMode = true;
            velEditDoneButton.gameObject.SetActive(true);
            SelectedPlanet.outerSphere.SetActive(true);
        });

        velEditDoneButton.onClick.AddListener(() =>
        {
            editPane.SetActive(true);
            _setVelocityMode = false;
            velEditDoneButton.gameObject.SetActive(false);
            SelectedPlanet.outerSphere.SetActive(false);
        });

        closeButton.onClick.AddListener(() =>
        {
            ModeManager.Instance.ShowButtons(true);
            gameObject.SetActive(false);
            SelectedPlanet.color = _prevColor;
            SelectedPlanet.radius = _prevRadius;
            SelectedPlanet.initialVelocity = _prevVel;
            SelectedPlanet = null;
        });

        doneButton.onClick.AddListener(() =>
        {
            ModeManager.Instance.ShowButtons(true);
            gameObject.SetActive(false);
            SelectedPlanet = null;
        });

        radiusSlider.onValueChanged.AddListener(val =>
        {
            var roundedVal = (int) (val * 100) / 100.0f;
            radiusVal.text = $"{roundedVal}";
            SelectedPlanet.radius = roundedVal;
        });
    }

    private void Update()
    {
        if (_setVelocityMode)
        {
            if (Input.GetKey(KeyCode.Escape))
            {
                SelectedPlanet.initialVelocity = _prevVel;
                velEditDoneButton.OnSubmit(null);
            }

            // setting the initial velocity/direction by shooting a ray from the camera center
            // to the outer sphere of the selected planet
            var ray = Camera.current.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            ray = new Ray(ray.origin + ray.direction * 1000f, -ray.direction);

            if (Physics.Raycast(ray, out var hit, 1000f) && hit.collider.gameObject.CompareTag("OuterSphere"))
                SelectedPlanet.initialVelocity = (hit.point - SelectedPlanet.transform.position).normalized * 0.1f;
        }
        else
        {
            if (Input.GetKey(KeyCode.Escape))
                closeButton.OnSubmit(null);

            if (Input.touchCount >= 1)
            {
                var touchPos = Input.GetTouch(0).position;

                // get the selected color from the color wheel
                if (_colorWheelCollider.OverlapPoint(touchPos))
                {
                    if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_colorWheelTransform,
                        touchPos, null, out var rectPos))
                    {
                        var rect = _colorWheelTransform.rect;
                        var loc = rectPos + new Vector2(rect.width, rect.height);
                        loc.x = loc.x / rect.width * _colorWheelTex.width;
                        loc.y = loc.y / rect.height * _colorWheelTex.height;
                        colorImage.color = _colorWheelTex.GetPixel((int) loc.x, (int) loc.y);
                        SelectedPlanet.color = colorImage.color;
                    }
                }
            }
        }
    }
}