using UnityEngine;

namespace XRP
{
	public class XrpPanel : MonoBehaviour
	{
		private XrpPointer[] _pointers;
		private XrpControl[] _controls;

		public float HoverDistance = 0.1f;
		public float TouchDistance = 0.02f;

		public AudioClip HoverClip;
		public AudioClip PressClip;

		private bool[] _hoverCandidates;
		private bool[] _touchCandidates;

		public void Awake()
		{
			_controls = transform.GetComponentsInChildren<XrpControl>();
			foreach (var control in _controls) control.Panel = this;
			_hoverCandidates = new bool[_controls.Length];
			_touchCandidates = new bool[_controls.Length];
			
			_pointers = FindObjectsOfType<XrpPointer>();
		}

		public void Update()
		{
			
			foreach (var pointer in _pointers) {
				for (var i = 0; i < _controls.Length; i++) {
					var displacement = _controls[i].GetDistance(pointer.transform.position);

					_hoverCandidates[i] =
						displacement.magnitude <= HoverDistance;

					_touchCandidates[i] =
						displacement.magnitude <= TouchDistance;

				}
			}
			
			for (var i = 0; i < _controls.Length; i++) {
				var state = _controls[i].CurrentState;
				
				if (_touchCandidates[i]) {
					if (state == XrpControl.State.Inactive || state == XrpControl.State.Hover) _controls[i].StartTouch();
				} else {
					if (state == XrpControl.State.Touch) _controls[i].StopTouch();
					
					if (_hoverCandidates[i]) {
						if (state == XrpControl.State.Inactive) _controls[i].StartHover();
					} else {
						if (state == XrpControl.State.Hover) _controls[i].StopHover();
					}
				}
			}
			
			
		}
	}
}