using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class Planet : MonoBehaviour
{
    public float mass;
    public Vector3 initialVelocity;

    [SerializeField] private Material selectedMaterial;
    [SerializeField] private Material defaultMaterial;

    public bool HasChanged { set; get; }
    public bool IsSetup { get; private set; }
    public bool IsSelected { get; set; }


    private const float InitialHeight = 0.001f;
    private TextMesh _textMesh;
    private Vector3 _prevPosition;
    private float _prevMass;
    private Vector3 _prevInitialVelocity;
    private MeshRenderer _renderer;

    private void Start()
    {
        _textMesh = GetComponentInChildren<TextMesh>();
        PlanetManager.Instance.RegisterPlanet(this);
        initialVelocity.z = Random.Range(-0.2f, 0.2f);
        _renderer = GetComponentInChildren<MeshRenderer>();
    }

    private void Update()
    {
        CheckPropertiesChanged();
        if (!IsSetup) MoveToInitialPosition();
        _renderer.material = IsSelected ? selectedMaterial : defaultMaterial;
    }

    private void CheckPropertiesChanged()
    {
        if (Math.Abs(_prevMass - mass) > 0.001)
        {
            HasChanged = true;
            _prevMass = mass;
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

    public void SetText(string text)
    {
        _textMesh.text = text;
    }
}