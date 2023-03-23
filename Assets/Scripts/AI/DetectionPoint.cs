// Author: Liam Bansal
// Collaborator: N/A
// Created On: 28/2/2023

using UnityEngine;

/// <summary>
/// An empty class. Use it as a component when there's more than one point on 
/// an object that contributes to it being detected by a visual sensor.
/// </summary>
public class DetectionPoint : MonoBehaviour {
	/// <summary>
	/// Implement this interface for any object that uses a detection point.
	/// Contains data for classes to use when registering how may points are
	/// visible (accessible to the implementing class and this).
	/// </summary>
	public interface IDetectionPointData {
		public int VisibleDetectionPoints {
			get;
			set;
		}
	}

	private bool isVisible = false;
	private MonoBehaviour detectionPointDataSource = default;

	public void SetDataSource(MonoBehaviour dataSource) {
		detectionPointDataSource = dataSource;
	}

	public void IsVisible(bool isVisible) {
		if (this.isVisible == isVisible) {
			return;
		}

		this.isVisible = isVisible;
		UpdatePointData();		
	}

	/// <summary>
	/// Updates the associated classes' number of visible detection points.
	/// </summary>
	private void UpdatePointData() {
		if (!(detectionPointDataSource is IDetectionPointData)) {
			return;
		}

		IDetectionPointData pointData = detectionPointDataSource as IDetectionPointData;

		if (isVisible) {
			++pointData.VisibleDetectionPoints;
		} else {
			--pointData.VisibleDetectionPoints;
		}
	}
}
