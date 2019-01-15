using System.Collections;
using System.Collections.Generic;
using PixelComrades;
using UnityEngine;

[RequireComponent (typeof(LineRenderer))]

public class LitTendril : MonoBehaviour {

	[Tooltip("Start Point transform.")]
	public Transform StartPoint;

	[Tooltip("Wave Speed")]
	public float Speed = 1.0f;

	[Tooltip("Wave Frequency Phase in Start Point Local X.")]
	public float WavePhaseX = -5.0f;

	[Tooltip("Wave Frequency Phase in Start Point Local Y.")]
	public float WavePhaseY = 0.0f;

    [Tooltip("Wave Size Local X.")]
    public float WaveSizeX = 2.0f;

    [Tooltip("Wave Size Local Y.")]
    public float WaveSizeY = 2.0f;

    [Tooltip("Tendril Length from Start Point Local Z.")]
	public float TendrilLength = 6.0f;

	[Tooltip("Tendril Color")]
	public Color StartColor = Color.yellow;
	public Color EndColor = Color.red;

	[Tooltip("Number of sections in Tendril detail (Suggested < 25)")]
	public int SectionDetail = 15;

	[Tooltip("Tendril width")]
	public float StartSize = 0.5f;
	public float EndSize = 0.05f;

	[Tooltip("Tendril center light (optional)")]
	public Light TendrilLight;

	[Tooltip("Use Tendril Color for light")]
	public bool UseTendrilColor = false;

	[Tooltip("Use Tendril length for light range")]
	public bool UseTendrilRange = false;

	private LineRenderer _lineRenderer;
	private Vector3 _end;

	void Awake() {
		_lineRenderer = GetComponent<LineRenderer>();
		//_lineRenderer.useWorldSpace = true;
        _lineRenderer.receiveShadows = false;
        _lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        if (_lineRenderer.material == null){
			_lineRenderer.material = new Material (Shader.Find("Particles/Additive"));
		}
        InitColors();
		if (!StartPoint){
			StartPoint = gameObject.transform;
		}
	}

    [ContextMenu("InitColors")]
    public void InitColors() {
        _lineRenderer.startWidth = StartSize;
        _lineRenderer.endWidth = EndSize;
        _lineRenderer.startColor = StartColor;
        _lineRenderer.endColor = EndColor;
        _lineRenderer.positionCount = SectionDetail;
        SectionDetail = Mathf.Clamp(SectionDetail, 2, 50);
        if (TendrilLight) {
            if (UseTendrilColor) {
                TendrilLight.color = StartColor;
            }
            if (UseTendrilRange) {
                TendrilLight.range = TendrilLength;
            }
        }
    }

	void Update() {        
	    if (TendrilLight){
			TendrilLight.transform.position = 
                StartPoint.position + (StartPoint.forward*TendrilLength/2) + (StartPoint.right*WaveSizeX/2*(Mathf.Sin(-WavePhaseX+(Time.time*Speed)))) + (StartPoint.up*WaveSizeY/2*(Mathf.Sin(-WavePhaseY+(Time.time*Speed))));
		}
		for (var i = 0 ; i < SectionDetail*1f ; i++ ){
			var g = i/(SectionDetail*1f);
			_end =  (StartPoint.right * WaveSizeX * g * (Mathf.Sin(g *WavePhaseX+(Time.time*Speed)))) + (StartPoint.up* WaveSizeY * g * (Mathf.Sin(g *WavePhaseY+(Time.time*Speed)))) + (StartPoint.forward*TendrilLength*g) ;
			_lineRenderer.SetPosition(i, StartPoint.position + _end);
		}       
	}

#if UNITY_EDITOR

    void OnDrawGizmosSelected(){
        SectionDetail = Mathf.Clamp(SectionDetail, 2, 50);
        if (!StartPoint){
			StartPoint = gameObject.transform;
		}
		Gizmos.color = Color.white; 
		Gizmos.DrawLine(StartPoint.position, StartPoint.position + (StartPoint.forward * TendrilLength) ); 
		for(var i = 0 ; i < SectionDetail; i++ ){
			var g = i/(SectionDetail);
			Gizmos.color = Color.Lerp(StartColor, EndColor, g);
			Gizmos.DrawWireSphere(StartPoint.position + (StartPoint.forward * TendrilLength * g), (MathEx.Max(WaveSizeX, WaveSizeY)) * g);
		}
	}
#endif
}
