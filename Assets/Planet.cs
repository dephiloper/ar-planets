using System;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// @author Philipp Bönsch
/// manages planet behaviour in the simulation
/// </summary>
public class Planet : MonoBehaviour
{
    private const float MassCoefficient = 1.0f;
    public const float ScaleCoefficient = 0.2f;
    public float Mass => 4.0f / 3.0f * 3.14159f * Mathf.Pow(radius, 3) * MassCoefficient;


    public GameObject outerSphere;
    public float radius;
    public Color color;
    public Vector3 initialVelocity;

    [SerializeField] private Transform sphere;
    [SerializeField] private Material defaultMaterial;
    [SerializeField] private Material highlightMaterial;

    public bool HasChanged { set; get; }
    public bool IsSetup { get; private set; }
    public bool IsSelected { get; set; }

    public bool HasCollided { get; set; }


    private const float InitialHeight = 0.001f;
    private Vector3 _startPosition;
    private Vector3 _prevPosition;
    private float _prevMass;
    private float _prevRadius;
    private float _prevSpeed;
    private Vector3 _prevInitialVelocity;
    private Color _prevColor;
    private bool _prevSelected;
    private MeshRenderer _meshRenderer;
    private bool _colorChanged;
    private static readonly int MainColor = Shader.PropertyToID("MainColor");
    private static readonly int Albedo = Shader.PropertyToID("_Color");

    private void Start()
    {
        PlanetManager.Instance.RegisterPlanet(this);
        if (initialVelocity == Vector3.zero)
            initialVelocity.z = Random.Range(-0.2f, 0.2f);
        _meshRenderer = GetComponentInChildren<MeshRenderer>();
        color = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
    }

    private void Update()
    {
        CheckPropertiesChanged();
        if (!IsSetup) MoveToInitialPosition();

        if (_colorChanged && IsSelected)
        {
            _meshRenderer.material = highlightMaterial;
            _meshRenderer.material.SetColor(MainColor, color);
            _meshRenderer.material.SetColor(Albedo, color);
            GetComponent<LineRenderer>().material.color = new Color(color.r, color.g, color.b, 0.2f);
            _colorChanged = false;
        }
        else if (_colorChanged && !IsSelected)
        {
            _meshRenderer.material = defaultMaterial;
            _meshRenderer.material.SetColor(Albedo, color);
            GetComponent<LineRenderer>().material.color = new Color(color.r, color.g, color.b, 0.2f);
            _colorChanged = false;
        }
    }

    private void CheckPropertiesChanged()
    {
        if (Math.Abs(_prevMass - Mass) > 0.001f)
        {
            HasChanged = true;
            _prevMass = Mass;
        }

        if (_prevPosition != transform.position)
        {
            HasChanged = true;
            _prevPosition = transform.position;
        }

        if (_prevInitialVelocity != initialVelocity)
        {
            HasChanged = true;
            _prevInitialVelocity = initialVelocity;
        }

        if (Math.Abs(_prevRadius - radius) > 0.001f)
        {
            HasChanged = true;
            _prevRadius = radius;
            sphere.localScale = new Vector3(radius, radius, radius) * ScaleCoefficient;
        }

        if (_prevColor != color)
        {
            HasChanged = true;
            _prevColor = color;
            _colorChanged = true;
        }

        if (_prevSelected != IsSelected)
        {
            HasChanged = true;
            _prevSelected = IsSelected;
            _colorChanged = true;
        }
    }

    private void MoveToInitialPosition()
    {
        var t = transform;

        t.position += Vector3.up * Time.deltaTime;
        if (transform.position.y >= InitialHeight)
        {
            var newPosition = transform.position;
            newPosition.y = InitialHeight;
            t.position = newPosition;
            IsSetup = true;
        }
    }

    public void ResetPosition() => transform.position = _startPosition;

    public void SaveStartPosition() => _startPosition = transform.position;
}