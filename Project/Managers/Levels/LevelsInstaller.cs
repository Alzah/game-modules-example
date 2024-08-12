using System;
using UnityEngine;
using Zenject;

namespace Project.Managers.Levels
{
	public class LevelsInstaller : ScriptableObjectInstaller
	{
		public override void InstallBindings()
		{
			throw new NotImplementedException("Need create custom installer for current project with inject ILevelLoader and ILevelManager");
		}
	}
}
