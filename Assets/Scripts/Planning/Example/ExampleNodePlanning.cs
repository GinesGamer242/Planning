using UnityEngine;
using System.Collections;

public class ExampleNodePlanning
{
	public ExampleWorld.WorldState   mWorldState;
  
  public ExampleAction             mAction;
                            
	public float              gCost;
	public float              hCost;
                            
	public ExampleNodePlanning       mParent;
	
  /***************************************************************************/

	public ExampleNodePlanning( ExampleWorld.WorldState worldState, ExampleAction action )
	{
		mWorldState     = worldState;
		mAction         = action;

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

  public bool Equals( ExampleNodePlanning other )
  {
    return mWorldState == other.mWorldState;
  }

  /***************************************************************************/

}
