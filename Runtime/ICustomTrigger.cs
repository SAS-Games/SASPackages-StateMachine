using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SAS.StateMachineGraph
{
	public interface ICustomTrigger
	{
		void OnStateEnter();
		void OnStateExit();
		bool Evaluate();
	}
}
