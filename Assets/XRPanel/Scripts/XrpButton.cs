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
		
		private BoxCollider _boxCollider;

		public override void Awake()
		{
			base.Awake();
			_boxCollider = GetComponent<BoxCollider>();
		}

		public override void Update()
		{
			base.Update();
			
			if (ActivePointer == null) return;
			
			if (CurrentState == State.Disabled) CheckReEnable();
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
			Debug.Log(localPos.z);
			if (localPos.z < Panel.TouchDistance) return;


			StopPress();
		}

		

		protected override void DoPress()
		{
			base.DoPress();
			Trigger();
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
		}

		public override void StopPress()
		{
			base.StopPress();
		}

		public void Trigger()
		{
			OnClick.Invoke();

			PopFadePanel();
			
			CurrentState = State.Disabled;
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