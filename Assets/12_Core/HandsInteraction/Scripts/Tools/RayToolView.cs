/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 * All rights reserved.
 *
 * Licensed under the Oculus SDK License Agreement (the "License");
 * ...
 */

using Meta.XR.Samples;
using UnityEngine;
using UnityEngine.Assertions;

namespace OculusSampleFramework
{
    /// <summary>
    /// Visual portion of ray tool, simplified to work without InteractableTool.
    /// </summary>
    [MetaCodeSample("StarterSample.Core-HandsInteraction")]
    public class RayToolView : MonoBehaviour
    {
        private const int NUM_RAY_LINE_POSITIONS = 25;
        private const float DEFAULT_RAY_CAST_DISTANCE = 3.0f;

        [Header("手部 Transform（用於射線起點）")]
        public Transform handTransform;

        [SerializeField] private Transform _targetTransform = null;
        [SerializeField] private LineRenderer _lineRenderer = null;

        private Transform _focusedTransform = null;
        private Vector3[] linePositions = new Vector3[NUM_RAY_LINE_POSITIONS];
        private Gradient _oldColorGradient, _highLightColorGradient;

        private void Awake()
        {
            Assert.IsNotNull(_targetTransform, "Target Transform is required!");
            Assert.IsNotNull(_lineRenderer, "LineRenderer is required!");
            Assert.IsNotNull(handTransform, "Hand Transform is required!");
            _lineRenderer.positionCount = NUM_RAY_LINE_POSITIONS;

            _oldColorGradient = _lineRenderer.colorGradient;
            _highLightColorGradient = new Gradient();
            _highLightColorGradient.SetKeys(
                new GradientColorKey[]
                {
                    new GradientColorKey(new Color(0.90f, 0.90f, 0.90f), 0.0f),
                    new GradientColorKey(new Color(0.90f, 0.90f, 0.90f), 1.0f)
                },
                new GradientAlphaKey[]
                {
                    new GradientAlphaKey(1.0f, 0.0f),
                    new GradientAlphaKey(1.0f, 1.0f)
                }
            );
        }

        /// <summary>
        /// 設定目前被聚焦的 Interactable（可選）
        /// </summary>
        public void SetFocusedInteractable(Interactable interactable)
        {
            _focusedTransform = interactable != null ? interactable.transform : null;
        }

        /// <summary>
        /// 控制射線顯示開關
        /// </summary>
        public bool EnableState
        {
            get { return _lineRenderer.enabled; }
            set
            {
                if (_targetTransform != null) _targetTransform.gameObject.SetActive(value);
                if (_lineRenderer != null) _lineRenderer.enabled = value;
            }
        }

        public void UpdateRayDirectly(Transform handTransform, Vector3 targetPosition)
        {
            if (handTransform == null || _lineRenderer == null || _targetTransform == null)
                return;

            var myPosition = handTransform.position;
            var myForward = handTransform.forward;

            var targetVector = targetPosition - myPosition;
            var targetDistance = targetVector.magnitude;

            var p0 = myPosition;
            var p1 = myPosition + myForward * targetDistance * 0.3333f;
            var p2 = myPosition + myForward * targetDistance * 0.6667f;
            var p3 = targetPosition;

            for (int i = 0; i < NUM_RAY_LINE_POSITIONS; i++)
            {
                linePositions[i] = GetPointOnBezierCurve(p0, p1, p2, p3, i / 25.0f);
            }

            _lineRenderer.SetPositions(linePositions);
            _targetTransform.position = targetPosition;
        }

        /// <summary>
        /// 射線更新
        /// </summary>
        private void Update()
        {
            if (handTransform == null || _lineRenderer == null || _targetTransform == null)
                return;

            // 射線起點與方向
            var myPosition = handTransform.position;
            var myForward = handTransform.forward;

            // 射線目標
            var targetPosition = _focusedTransform != null
                ? _focusedTransform.position
                : myPosition + myForward * DEFAULT_RAY_CAST_DISTANCE;

            var targetVector = targetPosition - myPosition;
            var targetDistance = targetVector.magnitude;

            // Bezier curve 計算
            var p0 = myPosition;
            var p1 = myPosition + myForward * targetDistance * 0.3333f;
            var p2 = myPosition + myForward * targetDistance * 0.6667f;
            var p3 = targetPosition;

            for (int i = 0; i < NUM_RAY_LINE_POSITIONS; i++)
            {
                linePositions[i] = GetPointOnBezierCurve(p0, p1, p2, p3, i / 25.0f);
            }

            _lineRenderer.SetPositions(linePositions);
            _targetTransform.position = targetPosition;
        }

        /// <summary>
        /// 四點 Bezier 曲線計算
        /// </summary>
        public static Vector3 GetPointOnBezierCurve(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            t = Mathf.Clamp01(t);
            var oneMinusT = 1f - t;
            var oneMinusTSqr = oneMinusT * oneMinusT;
            var tSqr = t * t;
            return oneMinusT * oneMinusTSqr * p0
                 + 3f * oneMinusTSqr * t * p1
                 + 3f * oneMinusT * tSqr * p2
                 + t * tSqr * p3;
        }
    }
}
