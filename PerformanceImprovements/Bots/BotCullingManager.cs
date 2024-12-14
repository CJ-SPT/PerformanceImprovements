using System;
using System.Collections.Generic;
using System.Linq;
using Comfort.Common;
using EFT;
using PerformanceImprovements.Config;
using PerformanceImprovements.Utils;
using UnityEngine;
using Logger = PerformanceImprovements.Utils.Logger;

namespace PerformanceImprovements.Bots;

internal class BotCullingManager : MonoBehaviour
{
	public static BotCullingManager Instance;
	
	private static Player MainPlayer => GameUtils.GetMainPlayer();
	private static readonly Dictionary<string, BotCullingData> BotCullingData = [];
	
	private void Start()
	{
		Singleton<IBotGame>.Instance.BotsController.BotSpawner.OnBotCreated += OnBotCreated;
		Singleton<IBotGame>.Instance.BotsController.BotSpawner.OnBotRemoved += OnBotRemoved;
	}
	
	private void OnDestroy()
	{
		Singleton<IBotGame>.Instance.BotsController.BotSpawner.OnBotCreated -= OnBotCreated;
		Singleton<IBotGame>.Instance.BotsController.BotSpawner.OnBotRemoved -= OnBotRemoved;
	}
	
	public void TryCullOrShowBot(BotOwner owner)
	{
		if (!Settings.EnableBotCulling.Value) return;
		
		var data = BotCullingData.Single(t => t.Key == owner.ProfileId).Value;
		
		// No renderers, send a warning
		if (data.BodyRenderers.Count == 0)
		{
			Logger.Warn($"Player {owner.name} doesn't have any renderers or has null data");
			return;
		}
		
		switch (IsValidCullingTarget(data.Owner) && !IsVisibleToCamera(data.BodyRenderers))
		{
			// Valid target, is currently not culled.
			case true:
				SwitchCullingStates(data, true);
				break;
			
			// Valid target, is currently culled.
			case false:
				SwitchCullingStates(data,false);
				break;
		}
	}
	
	private static void OnBotCreated(BotOwner owner)
	{
		BotCullingData.Add(owner.ProfileId, new BotCullingData(owner));
	}
	
	private static void OnBotRemoved(BotOwner owner)
	{
		BotCullingData.Remove(owner.ProfileId);
	}
	
	private static bool IsValidCullingTarget(BotOwner owner)
	{
		// Only cull AI, who are paused and is alive.
		return owner.IsAI && 
		       owner.StandBy.StandByType == BotStandByType.paused &&
		       owner.HealthController.IsAlive;
	}

	private static bool IsVisibleToCamera(List<Renderer> renderers)
	{
		return renderers.Any(r => r.IsVisibleFrom(CameraClass.Instance.Camera));
	}
	
	private static void SwitchCullingStates(BotCullingData data, bool isCulled)
	{
		Logger.Debug($"{(isCulled ? "Culling" : "Showing")} {data.Owner.ProfileId}");
		
		foreach (var renderer in data.BodyRenderers)
		{
			renderer.forceRenderingOff = isCulled;
		}
	}
}

internal class BotCullingData
{
	public BotOwner Owner;
	public bool IsCulled;
	public readonly List<Renderer> BodyRenderers = [];

	public BotCullingData(BotOwner owner)
	{
		Owner = owner;
		IsCulled = false;
		owner.GetPlayer.PlayerBody.GetRenderersNonAlloc(BodyRenderers);

		if (BodyRenderers.Count == 0)
		{
			Logger.Warn($"{owner.name} has no found renderers");
		}
	}
}