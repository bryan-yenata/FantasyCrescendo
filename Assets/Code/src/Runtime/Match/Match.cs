﻿using HouraiTeahouse.FantasyCrescendo.Networking;
using System;
using System.Threading.Tasks;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace HouraiTeahouse.FantasyCrescendo {

public abstract class Match {

  public async Task<MatchResult> RunMatch(MatchConfig config, bool loadScene = true) {
    await DataLoader.LoadTask.Task;
    Task sceneLoad = Task.CompletedTask;
    if (loadScene) {
      var stage = Registry.Get<SceneData>().Get(config.StageID);
      Assert.IsTrue(stage != null && stage.Type == SceneType.Stage);
      await stage.GameScene.LoadAsync();
    }
    var additionalScenes = Config.Get<SceneConfig>().AdditionalStageScenes;
    await Task.WhenAll(additionalScenes.Select(s => s.LoadAsync(LoadSceneMode.Additive)));
    var gameManager = Object.FindObjectOfType<MatchManager>();
    gameManager.Config = config;
    await LoadingScreen.Await(InitializeMatch(gameManager, config));
    await LoadingScreen.AwaitAll();
    return await gameManager.RunMatch();
  }

  protected IMatchSimulation CreateSimulation(MatchConfig config) {
    return new MatchSimulation(new IMatchSimulation[] { 
      new MatchPlayerSimulation(),
      new MatchHitboxSimulation(),
      new MatchRuleSimulation(MatchRuleFactory.CreateRules(config))
    });
  }

	// TODO(james71323): Refactor or move this to somewhere more sane
	protected virtual IMatchController CreateMatchController(MatchConfig config) {
		return new GameController(config);
	}

  protected abstract Task InitializeMatch(MatchManager manager, MatchConfig config);

  protected MatchState CreateInitialState(MatchConfig config) {
    var tag = Config.Get<SceneConfig>().SpawnTag;
    var startPositions = GameObject.FindGameObjectsWithTag(tag);
    if (startPositions.Length > 0) {
      return CreateInitialStateByTransform(config, startPositions);
    } 
    return CreateInitialStateSimple(config);
  }

  MatchState CreateInitialStateByTransform(MatchConfig config, GameObject[] startPositions) {
    var initialState = new MatchState(config);
    startPositions = startPositions.OrderBy(s => s.transform.GetSiblingIndex()).ToArray();
    for (uint i = 0; i < initialState.PlayerCount; i++) {
      var startPos = startPositions[i % startPositions.Length].transform;
      var state = initialState.GetPlayerState(i);
      state.Position = startPos.position;
      state.Direction = startPos.transform.forward.x >= 0;
      initialState.SetPlayerState(i, state);
    }
    return initialState;
  }

  MatchState CreateInitialStateSimple(MatchConfig config) {
    var initialState = new MatchState(config);
    for (uint i = 0; i < initialState.PlayerCount; i++) {
      var state = initialState.GetPlayerState(i);
      state.Position = new Vector3((int)i * 2 - 3, 1, 0);
      initialState.SetPlayerState(i, state);
    }
    return initialState;
  }

}

}