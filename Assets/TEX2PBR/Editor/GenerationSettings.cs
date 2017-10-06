using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GenerationSettings
{
    [SerializeField]
    public float albedoNoiseRemovalStrength;
    [SerializeField]
    public float albedoShadowHighlightRemovalStrength;
    [SerializeField]
    public float heightSmooth;
    [SerializeField]
    public float normalDetailStrength;
    [SerializeField]
    public float occlusionSpread;
    [SerializeField]
    public float metallicness;
}
