using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.Events;

namespace XRP
{

	[RequireComponent(typeof(BoxCollider))]
	public class XrpButton : XrpControl
	{

		public UnityEvent OnClick;

		public Renderer DebugRenderer;

		public float ThresholdDistance = 0.01f;
		public float TriggerDistance = 0.4f;

		private BoxCollider _boxCollider;
		private Transform _fadePanel;
		private Material _fadePanelMat;
		private LineRenderer _line;

		public override void Awake()
		{
			base.Awake();
			_boxCollider = GetComponent<BoxCollider>();
			_fadePanel = transform.Find("ActiveGeometry/FadePanel");
			_fadePanelMat = _fadePanel.GetComponent<Renderer>().material;
			_line = transform.Find("ActiveGeometry/Line").GetComponent<LineRenderer>();
		}

		public void Update()
		{
			ShowDebugColor();

			if (ActivePointer == null) return;
			
			ShowDebugPointer();
			if (CurrentState == State.Disabled) {
				CheckReEnable();
			} else {
				if(CurrentState != State.Press) CheckForPress();
				else DoPress();
			}
		}

		private void ShowDebugColor()
		{
			switch (CurrentState) {
				case State.Disabled:
					DebugRenderer.material.color = Color.Lerp(DebugRenderer.material.color, Color.red, 0.2f);
					break;
				case State.Inactive:
					DebugRenderer.material.color = Color.Lerp(DebugRenderer.material.color, Color.black, 0.2f);
					break;
				case State.Hover:
					DebugRenderer.material.color = Color.Lerp(DebugRenderer.material.color, Color.gray, 0.2f);
					break;
				case State.Touch:
					DebugRenderer.material.color = Color.Lerp(DebugRenderer.material.color, Color.white, 0.2f);
					break;
				case State.Press:
					DebugRenderer.material.color = Color.Lerp(DebugRenderer.material.color, Color.cyan, 0.2f);
					break;
				default:
					Debug.LogError("Unexpected state unhandled");
					break;
			}
		}

		private void ShowDebugPointer()
		{
			if (ActivePointer != null) {
				Debug.DrawLine(transform.position, ActivePointer.transform.position, Color.red);
			}
		}

		private void CheckReEnable()
		{
			var pointerPos = ActivePointer.transform.position;
			var localPos = transform.InverseTransformPoint(pointerPos);
			if (!(localPos.z > TriggerDistance)) return;
			
			CurrentState = State.Inactive;
			ActivePointer = null;
		}

		private void CheckForPress()
		{
			var pointerPos = ActivePointer.transform.position;
			var localPos = transform.InverseTransformPoint(pointerPos);
			if (localPos.z < -ThresholdDistance) {
				StartPress();
			}
		}

		private void DoPress()
		{
			var pointerPos = ActivePointer.transform.position;
			var localPos = transform.InverseTransformPoint(pointerPos);
			var invertedLocalPos = new Vector3(localPos.x, localPos.y, -localPos.z);

			if (localPos.z > ThresholdDistance) {
				StopPress();
				return;
			}
			
			_fadePanel.localPosition = new Vector3(0f, 0f, -localPos.z);
			_fadePanelMat.color = new Color(1f, 1f, 1f, Mathf.Lerp(0f, 1f, Mathf.InverseLerp(0f, -TriggerDistance, localPos.z)));
			_line.startColor = _line.endColor = _fadePanelMat.color;
			_line.positionCount = 2;
			var vertices = new[]
			{
				localPos,
				invertedLocalPos
			};
			_line.SetPositions(vertices);

			if (localPos.z < -TriggerDistance) {
				Trigger();
			}
		}

		public override void StartHover()
		{
			base.StartHover();
		}

		public override void StartTouch(XrpPointer pointer)
		{
			base.StartTouch(pointer);
		}

		public override void StopTouch()
		{
			base.StopTouch();
		}

		public override void StartPress()
		{
			base.StartPress();
			
			_line.positionCount = 2;
			_line.enabled = true;
			_fadePanel.gameObject.SetActive(true);
			_fadePanel.localPosition = Vector3.zero;
			_fadePanelMat.color = new Color(1f, 1f, 1f, 0f);
			_line.startColor = _line.endColor = _fadePanelMat.color;
			
		}

		public override void StopPress()
		{
			base.StopPress();
			
			_line.positionCount = 0;
			_line.enabled = false;
			_fadePanel.gameObject.SetActive(false);
		}

		public void Trigger()
		{
			OnClick.Invoke();
			
			var indicator = Instantiate(_fadePanel.gameObject);
			indicator.transform.parent = _fadePanel.parent;
			indicator.transform.localPosition = _fadePanel.localPosition;
			indicator.transform.localRotation = _fadePanel.localRotation;
			indicator.transform.localScale = _fadePanel.localScale;
			indicator.AddComponent<OneShotter>().StartCoroutine(FadeIndicator(indicator, 0.4f));
			
			StopPress();
			CurrentState = State.Inactive;
			ActivePointer = null;
			AudioSource.PlayClipAtPoint(Panel.PressClip, transform.position, 0.3f);
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

		IEnumerator FadeIndicator(GameObject indicator, float duration)
		{
			var indicatorMaterial = indicator.GetComponent<Renderer>().material;
			
			var startColor = indicatorMaterial.color;
			var endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);

			var startScale = indicator.transform.localScale;
			var endScale = indicator.transform.localScale * 1.2f;

			for (var i = 0f; i < duration; i += Time.deltaTime) {
				var iL = (i - 1f) * (i - 1f) * (i - 1f) * (i - 1f) * (i - 1f) + 1f;
				indicator.transform.localScale = Vector3.Lerp(startScale, endScale, iL);
				indicatorMaterial.color = Color.Lerp(startColor, endColor, iL);
				yield return null;
			}

			Destroy(indicatorMaterial);
			Destroy(indicator);
		}
	}

}