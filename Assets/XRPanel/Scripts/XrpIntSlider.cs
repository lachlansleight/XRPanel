using UnityEngine;

namespace XRP
{
	public class XrpIntSlider : XrpControl
	{
		
		public int MinValue = 0;
		public int MaxValue = 5;
		public int CurrentValue = 2;
		private float _currentDisplayValue = 2f;

		private Transform _sliderGeometry;

		public FloatDelegate OnValueChanged;
		public UnityIntEvent OnValueChangedEvent;

		public override void Awake()
		{
			base.Awake();

			_sliderGeometry = transform.Find("Geometry/Main/Slider");
		}

		public override void Update()
		{
			base.Update();
			if (CurrentState != State.Press) _currentDisplayValue = CurrentValue;
			
			SetSliderGeometry();
		}

		private void SetSliderGeometry()
		{
			var sliderLerpValue = Mathf.InverseLerp(MinValue, MaxValue, _currentDisplayValue);
			
			//find shortest side - that side x 0.05 = width of border geometry and width of padding
			var shortest = Mathf.Min(transform.localScale.x, transform.localScale.y);
			var gapX = 0.05f * shortest / transform.localScale.x;
			var gapY = 0.05f * shortest / transform.localScale.y;

			var maxSize = 1f - (gapX * 2f);
			var xScale = Mathf.Lerp(0f, maxSize, sliderLerpValue);
			var yScale = 1f - (gapY * 2f);

			_sliderGeometry.localScale = new Vector3(xScale, yScale, 1.5f);

			_sliderGeometry.localPosition = new Vector3(Mathf.Lerp(-maxSize / 2f, 0f, sliderLerpValue), 0f, 0f);
		}

		protected override void DoPress()
		{
			base.DoPress();
			if (CurrentState != State.Press) return;
			
			FadePanel.localScale = new Vector3(
				Mathf.InverseLerp(MinValue, MaxValue, _currentDisplayValue),
				FadePanel.localScale.y, 
				FadePanel.localScale.z
			);
			
			FadePanel.localPosition = new Vector3(
				Mathf.Lerp(0.5f, 0f, Mathf.InverseLerp(MinValue, MaxValue, _currentDisplayValue)),
				FadePanel.localPosition.y,
				FadePanel.localPosition.z
			);

			var localPoint = transform.InverseTransformPoint(ActivePointer.transform.position);
			var preValue = CurrentValue;
			_currentDisplayValue = Mathf.Lerp(MinValue, MaxValue, Mathf.InverseLerp(0.5f, -0.5f, localPoint.x));
			var remainder = _currentDisplayValue % 1f;
			remainder = QuinticLerpInOut(remainder);
			_currentDisplayValue = Mathf.Floor(_currentDisplayValue) + remainder;

			var currentActualValue = Mathf.RoundToInt(_currentDisplayValue);

			if (currentActualValue != preValue) {
				CurrentValue = Mathf.RoundToInt(currentActualValue);
				OnValueChanged?.Invoke(CurrentValue);
				OnValueChangedEvent.Invoke(CurrentValue);
			}
		}

		private float QuinticLerpInOut(float t)
		{
			if (t < 0.5f) return 16f * t * t * t * t * t;
			t--;
			return 16f * t * t * t * t * t + 1f;
		}
		
	}
}