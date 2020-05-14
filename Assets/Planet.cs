using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class Planet : MonoBehaviour
{
    public bool isSetup;

    public float mass;
    public Vector3 initialVelocity;

    public Vector3 velocity;
    public bool hasChanged;

    private const float InitialHeight = 0.02f;
    private TextMesh _textMesh;
    private Vector3 _prevPosition;
    private float _prevMass;
    private Vector3 _prevInitialVelocity;
    
    private void Start()
    {
        _textMesh = GetComponentInChildren<TextMesh>();
        PlanetManager.Instance.RegisterPlanet(this);
        initialVelocity.z = Random.Range(-0.2f, 0.2f);
    }
    
    private void Update()
    {
        CheckPropertiesChanged();
        if (!isSetup) MoveToInitialPosition();
    }

    private void CheckPropertiesChanged()
    {
        if (Math.Abs(_prevMass - mass) > 0.001)
        {
            hasChanged = true;
            _prevMass = mass;
        }
        
        if (_prevPosition != transform.position)
        {
            hasChanged = true;
            _prevPosition = transform.position;
        }
        
        if (_prevInitialVelocity != initialVelocity)
        {
            hasChanged = true;
            _prevInitialVelocity = initialVelocity;
        }
    }

    private void MoveToInitialPosition()
    {
        var t = transform;
        
        t.position += Vector3.up *  Time.deltaTime;
        if (transform.position.y >= InitialHeight)
        {
            var newPosition = transform.position;
            newPosition.y = InitialHeight;
            t.position = newPosition;
            isSetup = true;
            velocity = initialVelocity;
        }
    }
}
