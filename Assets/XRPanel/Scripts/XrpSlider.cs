using UnityEngine;

namespace XRP
{
	[System.Serializable]
	public class UnityFloatEvent : UnityEngine.Events.UnityEvent<float> { }
	
	public class XrpSlider : XrpControl
	{
		public float MinValue = 0f;
		public float MaxValue = 1f;
		public float CurrentValue = 0.5f;

		private Transform _sliderGeometry;
		private Transform _fadePanel;
		private BoxCollider _boxCollider;
		
		public UnityFloatEvent OnValueChanged;

		public override void Awake()
		{
			base.Awake();

			_sliderGeometry = transform.Find("Geometry/Main/Slider");
			_fadePanel = transform.Find("ActiveGeometry/FadePanel");
			_boxCollider = GetComponent<BoxCollider>();
		}

		public override void Update()
		{
			base.Update();
			SetSliderGeometry();
		}

		private void SetSliderGeometry()
		{
			var sliderLerpValue = Mathf.InverseLerp(MinValue, MaxValue, CurrentValue);
			
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
			
			_fadePanel.localScale = new Vector3(
				Mathf.InverseLerp(MinValue, MaxValue, CurrentValue),
				_fadePanel.localScale.y, 
				_fadePanel.localScale.z
			);
			
			_fadePanel.localPosition = new Vector3(
				Mathf.Lerp(0.5f, 0f, Mathf.InverseLerp(MinValue, MaxValue, CurrentValue)),
				_fadePanel.localPosition.y,
				_fadePanel.localPosition.z
			);

			var localPoint = transform.InverseTransformPoint(ActivePointer.transform.position);
			var preValue = CurrentValue;
			CurrentValue = Mathf.Lerp(MinValue, MaxValue, Mathf.InverseLerp(0.5f, -0.5f, localPoint.x));

			if (CurrentValue != preValue) {
				OnValueChanged.Invoke(CurrentValue);
			}
		}
		
		public override Vector3 GetDistance(Vector3 worldPoint)
		{
			var localPoint = transform.InverseTransformPoint(worldPoint);
			var displacement = localPoint;
			
			//we need the distance to the rectangular bounds, not the center
			var pointerDisplacement = new Vector3();
			if (displacement.x > _boxCollider.size.x * -0.5f && displacement.x < _boxCollider.size.x * 0.5f)
				pointerDisplacement.x = 0f;
			else if (displacement.x < _boxCollider.size.x * -0.5f)
				pointerDisplacement.x = displacement.x - _boxCollider.size.x * -0.5f;
			else if (displacement.x > transform.localScale.x * 0.5f)
				pointerDisplacement.x = displacement.x - _boxCollider.size.x * 0.5f;
			
			if (displacement.y > _boxCollider.size.y * -0.5f && displacement.y < _boxCollider.size.y * 0.5f)
				pointerDisplacement.y = 0f;
			else if (displacement.y < _boxCollider.size.y * -0.5f)
				pointerDisplacement.y = displacement.y - _boxCollider.size.y * -0.5f;
			else if (displacement.y > _boxCollider.size.y * 0.5f)
				pointerDisplacement.y = displacement.y - _boxCollider.size.y * 0.5f;
			
			if (displacement.z > _boxCollider.size.z * -0.5f && displacement.z < _boxCollider.size.z * 0.5f)
				pointerDisplacement.z = 0f;
			else if (displacement.z < _boxCollider.size.z * -0.5f)
				pointerDisplacement.z = displacement.z - _boxCollider.size.z * -0.5f;
			else if (displacement.z > _boxCollider.size.z * 0.5f)
				pointerDisplacement.z = displacement.z - _boxCollider.size.z * 0.5f;

			pointerDisplacement.x *= transform.lossyScale.x;
			pointerDisplacement.y *= transform.lossyScale.y;
			pointerDisplacement.z *= transform.lossyScale.z;
			
			return pointerDisplacement;
		}
	}
}