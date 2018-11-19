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

		private BoxCollider _boxCollider;

		public override void Awake()
		{
			base.Awake();
			_boxCollider = GetComponent<BoxCollider>();
		}

		public void Update()
		{
			switch (CurrentState) {
				case State.Disabled:
					DebugRenderer.material.color = Color.red;
					break;
				case State.Inactive:
					DebugRenderer.material.color = Color.black;
					break;
				case State.Hover:
					DebugRenderer.material.color = Color.gray;
					break;
				case State.Touch:
					DebugRenderer.material.color = Color.white;
					break;
				case State.Press:
					DebugRenderer.material.color = Color.cyan;
					break;
				default:
					Debug.LogError("Unexpected state unhandled");
					break;
			} 
		}

		public override void StartHover()
		{
			base.StartHover();
			AudioSource.PlayClipAtPoint(Panel.HoverClip, transform.position, 0.3f);
		}

		public override void StartPress()
		{
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
	}

}