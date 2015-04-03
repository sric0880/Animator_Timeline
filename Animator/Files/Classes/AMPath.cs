using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// AMPath: holds a path and interpolation type
[System.Serializable]
public class AMPath {
	public Vector3[] path;
	public int interp;			// interpolation
	public int startFrame;		// starting frame
	public int endFrame;		// ending frame
	public int startIndex;		// starting key index
	public int endIndex;		// ending key index

	public List<Vector2> samples;
	
	public AMPath() {
		
	}
	public AMPath(Vector3[] _path, int _interp, int _startFrame, int _endFrame) {
		path = _path;
		interp = _interp;
		startFrame = _startFrame;
		endFrame = _endFrame;
	}
	public AMPath(Vector3[] _path, int _interp, int _startFrame, int _endFrame, int _startIndex, int _endIndex) {
		path = _path;
		interp = _interp;
		startFrame = _startFrame;
		endFrame = _endFrame;
		startIndex = _startIndex;
		endIndex = _endIndex;
	}
	// number of frames
	public int getNumberOfFrames() {
		return endFrame-startFrame;
	}

//	private void cal(Vector2 start,ref Vector2 end, Vector3 pos1, Vector3 pos2, float e)
//	{
//		float dis = (pos2-pos1).magnitude;
//		if(dis > e)
//		{
//			float x = 0.5*(start.x + end.x);
//			Vector2 mid = new Vector2(x, 0);
//			Vector3 posMid = AMTween.PositionOnPath(path, x);
//			cal (start, mid, pos1, posMid, e);
//			cal (mid, end, posMid, pos2, e);
//		}
//		else
//		{
//			end.y = start.y + dis;
//			samples.Add(end);
//		}
//	}
//
//	public void sample(float e)
//	{
//		int len = path.Length;
//		float s = 0.0f;
//		for (int i = 0; i < len; ++i)
//		{
//			Vector2 start = new Vector2(i, s);
//			Vector2 end = new Vector2(i+1, 0);
//			Vector3 pos1 = AMTween.PositionOnPath(path, start.x);
//			Vector3 pos2 = AMTween.PositionOnPath(path, end.x);
//			samples.Add(start);
//			cal (start, end, pos1, pos2, e);
//			s = end.y;
//		}
//	}
}
