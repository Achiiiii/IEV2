﻿//-----------------------------------------------------------------------
// Copyright © 2019 Tobii Pro AB. All rights reserved.
//-----------------------------------------------------------------------

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Tobii.Research.Unity
{
    public class TrackBoxGuide : MonoBehaviour
    {
        /// <summary>
        /// Get <see cref="TrackBoxGuide"/> instance. This is assigned
        /// in Awake(), so call earliest in Start().
        /// </summary>
        public static TrackBoxGuide Instance { get; private set; }

        [SerializeField]
        [Tooltip("Activate or deactivate the track box guide.")]
        private bool _trackBoxGuideActive;

        [SerializeField]
        [Tooltip("This key will show or hide the track box guide.")]
        private KeyCode _toggleKey = KeyCode.None;

        [SerializeField]
        private GameObject _CanvasTrackBox;
        [SerializeField] private Calibration calibration;
        [SerializeField] private GameObject pointer;
        [SerializeField] private TMPro.TMP_Text content;
        [SerializeField] private GameObject blackTestBtn;
        [SerializeField] private GameObject colorTestBtn;


        private Color _colorMoverGood;
        private Color _colorMoverBad;
        private Color _colorEyeGood;
        private Color _colorEyeBad;

        private Rect _box;
        private Image _mover;
        private Image _colorPanel;
        private Image _eyeLeft;
        private Image _eyeRight;
        private Slider _sliderLeft;
        private Slider _sliderRight;
        private Vector3 _eyeScaleLeft;
        private Vector3 _eyeScaleRight;
        private EyeTracker _eyeTracker;
        private float _trackabilityLeft;
        private float _trackabilityRight;
        private float _moverScale;
        private float _leftEyeValidation;
        private float _rightEyeValidation;
        private float _validateTime;
        private float _countDownTime = 5;
        private bool _countDownLocker = false;

        /// <summary>
        /// Activate or deactivate the track box guide.
        /// </summary>
        public bool TrackBoxGuideActive
        {
            get
            {
                return _trackBoxGuideActive;
            }

            set
            {
                _trackBoxGuideActive = value;
                _eyeTracker.SubscribeToUserPositionGuide = value;
                _CanvasTrackBox.SetActive(_trackBoxGuideActive);
            }
        }

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            Application.targetFrameRate = 30;
            _eyeTracker = EyeTracker.Instance;

            var box = _CanvasTrackBox.transform.Find("ImageBox");
            _box = box.GetComponent<Image>().rectTransform.rect;
            _mover = box.Find("PanelMover").GetComponent<Image>();
            _colorPanel = box.Find("ImagePanel").GetComponent<Image>();
            _eyeLeft = _CanvasTrackBox.transform.Find("ImageEyeLeft").GetComponent<Image>();
            _eyeRight = _CanvasTrackBox.transform.Find("ImageEyeRight").GetComponent<Image>();
            _sliderLeft = _CanvasTrackBox.transform.Find("SliderLeft").GetComponent<Slider>();
            _sliderRight = _CanvasTrackBox.transform.Find("SliderRight").GetComponent<Slider>();

            _eyeScaleLeft = _eyeLeft.rectTransform.localScale;
            _eyeScaleRight = _eyeRight.rectTransform.localScale;

            _colorMoverGood = new Color32(23, 66, 57, 255);
            _colorMoverBad = new Color32(72, 26, 37, 255);
            _colorEyeGood = new Color32(200, 255, 200, 255);
            _colorEyeBad = new Color32(255, 200, 200, 255);

            TrackBoxGuideActive = _trackBoxGuideActive;
        }

        private void Update()
        {
            if (_countDownLocker)
            {
                Debug.Log("countdown: " + _countDownTime);
                _countDownTime -= Time.deltaTime;
                content.text = $"眼部校正將在 {Mathf.Max(Mathf.CeilToInt(_countDownTime), 0)} 秒後開始";
                if (_countDownTime < 0)
                {
                    Debug.Log("false");
                    _countDownLocker = false;
                    var calibrationStartResult = calibration.StartCalibration(
                        resultCallback: (calibrationResult) =>
                        {
                            Debug.Log("Calibration was " + (calibrationResult ? "successful" : "unsuccessful"));
                            pointer.SetActive(true);
                            blackTestBtn.SetActive(true);
                            colorTestBtn.SetActive(true);
                            content.text = "請問您今天想進行哪一種眼動測試呢？\n（凝視選項5秒）";
                        }
                    );
                }
            }

            if (_CanvasTrackBox.activeSelf != TrackBoxGuideActive)
            {
                TrackBoxGuideActive = _trackBoxGuideActive;
            }

            if (Input.GetKeyDown(_toggleKey))
            {
                TrackBoxGuideActive = !TrackBoxGuideActive;
            }

            if (_eyeTracker == null || !_trackBoxGuideActive)
            {
                return;
            }

            if (!_eyeTracker.SubscribeToUserPositionGuide)
            {
                _eyeTracker.SubscribeToUserPositionGuide = true;
            }

            var data = _eyeTracker.LatestUserPositionGuideData;
            var goLeft = data.LeftEye;
            var goRight = data.RightEye;
            var goLeftValid = data.LeftEyeValid;
            var goRightValid = data.RightEyeValid;

            _moverScale = PositionMover(goLeft.z, goRight.z, goLeftValid, goRightValid);
            PositionEye(goLeft, goLeftValid, _eyeLeft, _eyeScaleLeft);
            PositionEye(goRight, goRightValid, _eyeRight, _eyeScaleRight);

            var delta = Time.deltaTime / 2f;
            _leftEyeValidation = SetTrackabilitySlider(ref _trackabilityLeft, goLeftValid, _sliderLeft, delta);
            _rightEyeValidation = SetTrackabilitySlider(ref _trackabilityRight, goRightValid, _sliderRight, delta);

            if (_moverScale > 0.35f && _moverScale < 0.65f && _leftEyeValidation >= 0.8 && _rightEyeValidation >= 0.8) _validateTime += Time.deltaTime;
            else _validateTime = 0;

            if (_validateTime >= 2)
            {
                _validateTime = 0;

                _countDownLocker = true;

                TrackBoxGuideActive = false;
                Debug.Log("validate");
            }
        }

        /// <summary>
        /// Set trackability slider.
        /// </summary>
        /// <param name="trackability">Reference to the trackability history value.</param>
        /// <param name="valid">The eye validity.</param>
        /// <param name="slider">The slider to manimpulate.</param>
        /// <param name="delta">The delta change.</param>
        private static float SetTrackabilitySlider(ref float trackability, bool valid, Slider slider, float delta)
        {
            trackability = Mathf.Clamp01(trackability + (valid ? delta : -delta));
            slider.value = trackability;
            return trackability;
        }

        /// <summary>
        /// Move the depth indicator panel (the "mover").
        /// </summary>
        /// <param name="zLeft">The normalized track box z value of the left eye.</param>
        /// <param name="zRight">The normalized track box z value of the right eye.</param>
        /// <param name="validLeft">The validity of the left eye origin.</param>
        /// <param name="validRight">The validity of the right eye origin.</param>
        private float PositionMover(float zLeft, float zRight, bool validLeft, bool validRight)
        {
            if (!(validLeft || validRight))
            {
                // If nothing is valid, don't do anything.
                return 0;
            }

            // Select the scale.
            var scale = 1 - (validLeft && validRight ? (zLeft + zRight) / 2f : validLeft ? zLeft : zRight);

            // Set the scale.
            _mover.rectTransform.localScale = Vector3.one * Mathf.Lerp(0.5f, 1f, scale);

            // Set the color.
            _colorPanel.color = Color.Lerp(_colorMoverGood, _colorMoverBad, Mathf.Abs(0.5f - scale) * 2f);
            return scale;
        }

        /// <summary>
        /// Position eye in box.
        /// For a rect that is 512 wide and 256 high with pivot in center:
        /// Furthest away scale 0 - 128 in x and 0 - 64 in y
        /// Nearest             0 - 256    x     0 - 128   y
        /// (0, 0, 0) - the upper, right corner closest to the eye tracker,
        /// and (1, 1, 1) - the lower left, furthest away, corner.
        /// </summary>
        /// <param name="eyePos">The eye position normalized vector.</param>
        /// <param name="valid">The validity flag.</param>
        /// <param name="eye">The eye UI image component.</param>
        /// <param name="eyeScale">The UI scale of the eye.</param>
        private void PositionEye(Vector3 eyePos, bool valid, Image eye, Vector3 eyeScale)
        {
            if (!valid)
            {
                // "Close" the eye if signal is invalid.
                eye.rectTransform.localScale = Vector3.zero;
                return;
            }

            // Change the range of the z scale from 0 - 1 to 0.5 - 1.
            var scale = Mathf.Lerp(0.5f, 1f, 1 - eyePos.z);

            // Get an indicator on how near the center we are placed in the box.
            var distanceToCenter = Vector3.Distance(Vector3.one / 2f, eyePos) * 2f;

            // Scale x between 0 and 512 as if it was nearest, subtract half, then scale in z.
            eyePos.x = (Mathf.Lerp(0, _box.width, 1 - eyePos.x) - (_box.width / 2f)) * scale;


            // Scale x between 0 and 256 as if it was nearest, then subtract half, then scale in z.
            eyePos.y = (Mathf.Lerp(0, _box.height, 1 - eyePos.y) - (_box.height / 2f)) * scale;
            // Debug.Log(eyePos.x);
            // Debug.Log(eyePos.y);
            // Set the eye position.
            eye.rectTransform.anchoredPosition = eyePos;

            // Scale the eye.
            eye.rectTransform.localScale = eyeScale * scale;

            // Set the color.
            eye.color = Color.Lerp(_colorEyeGood, _colorEyeBad, distanceToCenter);
        }

    }
}