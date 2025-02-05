using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LincolnCpp.HUDIndicator {

	public class IndicatorOnScreen : Indicator
	{
		public IndicatorIconStyle style;

		protected override void CreateIndicatorCanvas(IndicatorRenderer renderer)
		{
			IndicatorCanvasOnScreen indicatorCanvasOnScreen = new IndicatorCanvasOnScreen();
			indicatorCanvasOnScreen.Create(this, renderer);

			indicatorsCanvas.Add(renderer, indicatorCanvasOnScreen);
		}
	}

}