using System.Collections;
using UnityEngine;

namespace XRP
{
	[RequireComponent(typeof(Collider))]
	public class XrpControl : MonoBehaviour
	{
		public enum State
		{
			Inactive,
			Hover,
			Touch,
			Press,
			Disabled
		}

		public struct PointerDisplacement
		{
			public float X;
			public float Y;
			public float Z;
			public float Distance;
		}

		public State CurrentState;
		public XrpPointer ActivePointer;

		public XrpPanel Panel;

		private Transform _fadePanel;
		private Material _fadePanelMat;
		private LineRenderer _line;
		
		private Renderer _debugRenderer;

		public virtual void Awake()
		{
			CurrentState = State.Inactive;
			_debugRenderer = transform.Find("Geometry/Main").GetComponent<Renderer>();
			_fadePanel = transform.Find("ActiveGeometry/FadePanel");
			_fadePanelMat = _fadePanel.GetComponent<Renderer>().material;
			_line = transform.Find("ActiveGeometry/Line").GetComponent<LineRenderer>();
		}

		public virtual void Update()
		{
			if (_debugRenderer != null) ShowDebugColor();

			if (ActivePointer == null) return;
			
			if (CurrentState != State.Press && CurrentState != State.Disabled) CheckForPress();
			if (CurrentState == State.Press) {
				if (ActivePointer == null) Debug.Log("How are we here?");
				DoPress();
			}
		}
		
		public virtual void StartHover()
		{
			CurrentState = State.Hover;
			AudioSource.PlayClipAtPoint(Panel.HoverClip, transform.position, 0.1f);
		}

		public virtual void StopHover()
		{
			CurrentState = State.Inactive;
		}

		public virtual void StartTouch(XrpPointer pointer)
		{
			CurrentState = State.Touch;
			ActivePointer = pointer;
		}

		public virtual void StopTouch()
		{
			CurrentState = State.Hover;
			ActivePointer = null;
		}

		public virtual void StartPress()
		{
			CurrentState = State.Press;
			
			_line.positionCount = 2;
			_line.enabled = true;
			_fadePanel.gameObject.SetActive(true);
			_fadePanel.localPosition = Vector3.zero;
			_fadePanelMat.color = new Color(1f, 1f, 1f, 0f);
			_line.startColor = _line.endColor = _fadePanelMat.color;
		}
		
		public virtual void StopPress()
		{
			CurrentState = State.Hover;
			ActivePointer = null;
			
			_line.positionCount = 0;
			_line.enabled = false;
			_fadePanel.gameObject.SetActive(false);
		}

		public virtual Vector3 GetDistance(Vector3 worldPoint)
		{
			return Vector3.zero;
		}

		protected virtual void DoPress()
		{
			var pointerPos = ActivePointer.transform.position;
			var localPos = transform.InverseTransformPoint(pointerPos);
			var invertedLocalPos = new Vector3(localPos.x, localPos.y, -localPos.z);

			if (localPos.z > Panel.PressThresholdDistance) {
				StopPress();
				return;
			}
		
			_fadePanel.localPosition = new Vector3(0f, 0f, -localPos.z);
			_fadePanelMat.color = new Color(1f, 1f, 1f, Mathf.Lerp(0f, 1f, Mathf.InverseLerp(0f, -Panel.PressMaxDistance, localPos.z)));
			_line.startColor = _line.endColor = _fadePanelMat.color;
			_line.positionCount = 2;

			localPos.x = Mathf.Clamp(localPos.x, -0.5f, 0.5f);
			localPos.y = Mathf.Clamp(localPos.y, -0.5f, 0.5f);
			invertedLocalPos.x = localPos.x;
			invertedLocalPos.y = localPos.y;
			var vertices = new[]
			{
				localPos,
				invertedLocalPos
			};
			_line.SetPositions(vertices);
		}

		protected void PopFadePanel()
		{
			var indicator = Instantiate(_fadePanel.gameObject);
			indicator.transform.parent = _fadePanel.parent;
			indicator.transform.localPosition = _fadePanel.localPosition;
			indicator.transform.localRotation = _fadePanel.localRotation;
			indicator.transform.localScale = _fadePanel.localScale;
			indicator.AddComponent<OneShotter>().StartCoroutine(FadeIndicator(indicator, 0.4f));
		}
		
		private void CheckForPress()
		{
			var pointerPos = ActivePointer.transform.position;
			var localPos = transform.InverseTransformPoint(pointerPos);
			if (localPos.z < -Panel.PressThresholdDistance) {
				StartPress();
			}
		}

		[ContextMenu("FixGeometry")]
		public void FixGeometry()
		{
			GetComponentInChildren<XrpControlGeometry>().FixGeometry();
		}
		
		private void ShowDebugColor()
		{
			switch (CurrentState) {
				case State.Disabled:
					_debugRenderer.material.color = Color.Lerp(_debugRenderer.material.color, Color.red, 0.2f);
					break;
				case State.Inactive:
					_debugRenderer.material.color = Color.Lerp(_debugRenderer.material.color, Color.black, 0.2f);
					break;
				case State.Hover:
					_debugRenderer.material.color = Color.Lerp(_debugRenderer.material.color, Color.gray, 0.2f);
					break;
				case State.Touch:
					_debugRenderer.material.color = Color.Lerp(_debugRenderer.material.color, Color.white, 0.2f);
					break;
				case State.Press:
					_debugRenderer.material.color = Color.Lerp(_debugRenderer.material.color, Color.cyan, 0.2f);
					break;
				default:
					Debug.LogError("Unexpected state unhandled");
					break;
			}
		}
		
		private IEnumerator FadeIndicator(GameObject indicator, float duration)
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