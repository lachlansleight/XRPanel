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

		public virtual void Awake()
		{
			CurrentState = State.Inactive;
		}

		public virtual void StartHover()
		{
			CurrentState = State.Hover;
		}

		public virtual void StopHover()
		{
			CurrentState = State.Inactive;
		}

		public virtual void StartTouch()
		{
			CurrentState = State.Touch;
		}

		public virtual void StopTouch()
		{
			CurrentState = State.Hover;
		}

		public virtual void StartPress()
		{
			CurrentState = State.Press;
		}
		
		public virtual void StopPress()
		{
			CurrentState = State.Hover;
		}

		public virtual Vector3 GetDistance(Vector3 worldPoint)
		{
			return Vector3.zero;
		}
	}
}