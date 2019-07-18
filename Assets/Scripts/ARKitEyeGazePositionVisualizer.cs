﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

/// <summary>
/// Visualizes the eye gaze position in face space for an <see cref="ARFace"/>.
/// </summary>
/// <remarks>
/// Face space is the space where the origin is the transform of an <see cref="ARFace">.
/// </remarks>
[RequireComponent(typeof(ARFace))]
public class ARKitEyeGazePositionVisualizer : MonoBehaviour
{
	[SerializeField]
	GameObject m_GazePositionPrefab;

	public GameObject gazePositionPrefab
	{
		get => m_GazePositionPrefab;
		set => m_GazePositionPrefab = value;
	}

	GameObject m_GazePositionGameObject;

	ARFace m_Face;
#if UNITY_IOS && !UNITY_EDITOR
    XRFaceSubsystem m_FaceSubsystem;
#endif

	void Awake()
	{
		m_Face = GetComponent<ARFace>();
		CreateEyeGameObjects();
	}

	void CreateEyeGameObjects()
	{
		m_GazePositionGameObject = Instantiate(m_GazePositionPrefab, m_Face.transform);
		m_GazePositionGameObject.SetActive(false);
	}

	void SetVisible(bool visible)
	{
		m_GazePositionGameObject.SetActive(visible);
	}

    void UpdateVisibility()
	{
		var visible =
			enabled &&
			(m_Face.trackingState == TrackingState.Tracking) &&
#if UNITY_IOS && !UNITY_EDITOR
            m_FaceSubsystem.supportedEyeGazeTracking &&
#endif
			(ARSession.state > ARSessionState.Ready);

		SetVisible(visible);
	}

    void OnEnable()
	{
#if UNITY_IOS && !UNITY_EDITOR
        var faceManager = FindObjectOfType<ARFaceManager>();
        if (faceManager != null)
        {
            m_FaceSubsystem = (XRFaceSubsystem)faceManager.subsystem;
        }
#endif
		UpdateVisibility();
		m_Face.updated += OnUpdated;
	}

    void OnUpdated(ARFaceUpdatedEventArgs eventArgs)
	{
		UpdateVisibility();
		UpdateEyeGazePosition();
	}

    void UpdateEyeGazePosition()
	{
#if UNITY_IOS && !UNITY_EDITOR
        var gazePosition = new Vector3();

        if (m_FaceSubsystem.supportedEyeGazeTracking)
        {
            if (m_FaceSubsystem.TryGetEyeGazePosition(m_Face.trackableId, ref gazePosition))
            {
                // The vector that represents the gaze offset from the face is not normalized and therefore could be behind the camera.
                // So set the offset to be 1/10th of meter in the direction of the offset
				m_GazePositionGameObject.transform.localPosition = Vector3.Normalize(gazePosition) / 10;
            }
            else
            {
                Debug.Log("Failed to get the face's eye poses.");
            }
        }
#endif
	}

}
