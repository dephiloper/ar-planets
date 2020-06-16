using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class Planet : MonoBehaviour
{
    public GameObject outerSphere;
    
    public float Mass => 4.0f / 3.0f * 3.14159f * radius * massCoefficient;

    public float radius;
    public Color color;
    public Vector3 initialVelocity;

    [SerializeField] private Material selectedMaterial;
    [SerializeField] private Material defaultMaterial;
    [SerializeField] private Transform sphere;
    [SerializeField] private float massCoefficient = 0.2f;
    [SerializeField] private float scaleCoefficient = 0.2f;

    public bool HasChanged { set; get; }
    public bool IsSetup { get; private set; }
    public bool IsSelected { get; set; }

    private const float InitialHeight = 0.001f;
    private Vector3 _prevPosition;
    private float _prevMass;
    private float _prevRadius;
    private Vector3 _prevInitialVelocity;
    private Color _prevColor;
    private bool _prevSelected;
    private MeshRenderer _meshRenderer;
    private bool _colorChanged;
    private static readonly int MainColor = Shader.PropertyToID("_MainColor");
    private static readonly int Albedo = Shader.PropertyToID("_Color");

    private void Start()
    {
        PlanetManager.Instance.RegisterPlanet(this);
        initialVelocity.z = Random.Range(-0.2f, 0.2f);
        _meshRenderer = GetComponentInChildren<MeshRenderer>();
    }

    private void Update()
    {
        CheckPropertiesChanged();
        if (!IsSetup) MoveToInitialPosition();

        if (_colorChanged && IsSelected)
        {
            _meshRenderer.material = selectedMaterial;
            _meshRenderer.material.SetColor(MainColor, color);
            _colorChanged = false;
        }
        else if (_colorChanged && !IsSelected)
        {
            _meshRenderer.material = defaultMaterial;
            _meshRenderer.material.SetColor(Albedo, color);
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
            sphere.localScale = new Vector3(radius, radius, radius) * scaleCoefficient;
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

    public void ShowOuterSphere(bool show)
    {
        outerSphere.SetActive(show);
    }
}