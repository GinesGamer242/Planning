using UnityEngine;
using System.Collections;

public class NewNodePlanning
{
	public NewWorld.WorldState mWorldState;
  
	public NewAction mAction;

	public int actionCount;
	public float gCost;
	public float hCost;
                            
	public NewNodePlanning mParent;
	
  /***************************************************************************/

	public NewNodePlanning( NewWorld.WorldState worldState, NewAction action )
	{
		mWorldState     = worldState;
		mAction         = action;

		actionCount		= 0;
		gCost           = 0.0f;
		hCost           = 0.0f;
		mParent         = null;
	}
                                                      
  /***************************************************************************/

	public float fCost {
		get {
			return gCost + hCost;
		}
	}

  /***************************************************************************/

  public bool Equals( NewNodePlanning other )
  {
    return mWorldState == other.mWorldState;
  }

  /***************************************************************************/

}
