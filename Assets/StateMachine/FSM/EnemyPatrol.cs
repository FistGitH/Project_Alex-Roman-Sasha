using System.Collections.Generic;
using UnityEngine;

public sealed class EnemyPatrol : MonoBehaviour
{
	[SerializeField] private List<Transform> _points = new List<Transform>();

	private int _currentPointIndex;

	public bool HasPoints => _points != null && _points.Count > 0;
	public Vector3 CurrentPoint => HasPoints ? _points[_currentPointIndex].position : transform.position;

	public void MoveToNextPoint()
	{
		if (!HasPoints)
			return;

		_currentPointIndex = (_currentPointIndex + 1) % _points.Count;
	}
}
