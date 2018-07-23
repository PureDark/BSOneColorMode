using IllusionPlugin;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace OneColorMode
{
    public class OneColorModeBehaviour : MonoBehaviour
    {

        private PlayerController _playerController;
        private GameSongController _gameSongController;
        private NoteCutEffectSpawner _noteCutEffectSpawner;
        private BeatmapObjectSpawnController _beatmapObjectSpawnController;
        private NoteCutHapticEffect _noteCutHapticEffect;
        private HapticFeedbackController _hapticFeedbackController;
        private MainSettingsModel _mainSettingsModel;
        private MainGameSceneSetupData _mainGameSceneSetupData;

        private Saber[] _sabers;

        public static bool IsLeftSaberOn
        {
            get;
            set;
        }

        private void Awake()
        {
            SceneManager.activeSceneChanged += SceneManagerOnActiveSceneChanged;
            DontDestroyOnLoad(gameObject);
            Console.WriteLine("One Color Mode loaded");
        }

        private void OnDestroy()
        {
            SceneManager.activeSceneChanged -= SceneManagerOnActiveSceneChanged;
        }


        /* These are to disable uploading scores to leaderboard*/
        private bool stored = false;
        private bool storedNoEnergy = false;
        IEnumerator RestoreNoEnergy()
        {
            yield return new WaitForSeconds(0.5f);
            _mainGameSceneSetupData.gameplayOptions.noEnergy = storedNoEnergy;
            stored = false;
        }
        private void SceneManagerOnActiveSceneChanged(Scene arg0, Scene scene)
        {
            try
            {
                this._playerController = FindObjectOfType<PlayerController>();

                if (scene.buildIndex == 1)
                {
                    IsLeftSaberOn = true;
                    if (_beatmapObjectSpawnController != null)
                    {
                        _beatmapObjectSpawnController.noteWasCutEvent -= this.HandleNoteWasCutEvent;
                    }

                    if (_mainSettingsModel != null && stored)
                    {
                        StartCoroutine(RestoreNoEnergy());
                    }
                }

                if (scene.buildIndex == 5)
                {
                    var _mainGameSceneSetup = FindObjectOfType<MainGameSceneSetup>();
                    this._gameSongController = FindObjectOfType<GameSongController>();
                    this._noteCutEffectSpawner = FindObjectOfType<NoteCutEffectSpawner>();
                    this._beatmapObjectSpawnController = FindObjectOfType<BeatmapObjectSpawnController>();
                    _noteCutHapticEffect = ReflectionUtil.GetPrivateField<NoteCutHapticEffect>(_noteCutEffectSpawner, "_noteCutHapticEffect");
                    _hapticFeedbackController = ReflectionUtil.GetPrivateField<HapticFeedbackController>(_noteCutHapticEffect, "_hapticFeedbackController");
                    _mainSettingsModel = ReflectionUtil.GetPrivateField<MainSettingsModel>(_hapticFeedbackController, "_mainSettingsModel");
                    _mainSettingsModel.controllersRumbleEnabled = true;
                    
                    this._mainGameSceneSetupData = ReflectionUtil.GetPrivateField<MainGameSceneSetupData>(_mainGameSceneSetup, "_mainGameSceneSetupData");
                    
                    if (!stored)
                    {
                        storedNoEnergy = _mainGameSceneSetupData.gameplayOptions.noEnergy;
                    }
                    stored = true;

                    if (_mainGameSceneSetupData.gameplayMode == GameplayMode.SoloNoArrows)
                    {
                        BeatmapDataModel _beatmapDataModel = ReflectionUtil.GetPrivateField<BeatmapDataModel>(_mainGameSceneSetup, "_beatmapDataModel");
                        BeatmapData beatmapData = CreateTransformedBeatmapData(_mainGameSceneSetupData.difficultyLevel.beatmapData, _mainGameSceneSetupData.gameplayOptions, _mainGameSceneSetupData.gameplayMode);
                        if (beatmapData != null)
                        {
                            _beatmapDataModel.beatmapData = beatmapData;
                            ReflectionUtil.SetPrivateField(_mainGameSceneSetup, "_beatmapDataModel", _beatmapDataModel);
                        }

                        if (Plugin.IsOneColorModeOn)
                        {
                            _mainGameSceneSetupData.gameplayOptions.noEnergy = true;
                            _sabers = FindObjectsOfType<Saber>();

                            Saber targetSaber = (Plugin.IsColorRed) ? _playerController.leftSaber : _playerController.rightSaber;
                            Saber otherSaber = (Plugin.IsColorRed) ? _playerController.rightSaber : _playerController.leftSaber;

                            if (targetSaber == null || otherSaber == null) return;

                            var targetCopy = Instantiate(targetSaber.gameObject);
                            Saber newSaber = targetCopy.GetComponent<Saber>();
                            targetCopy.transform.parent = targetSaber.transform.parent;
                            targetCopy.transform.localPosition = Vector3.zero;
                            targetCopy.transform.localRotation = Quaternion.identity;
                            targetSaber.transform.parent = otherSaber.transform.parent;
                            targetSaber.transform.localPosition = Vector3.zero;
                            targetSaber.transform.localRotation = Quaternion.identity;
                            otherSaber.gameObject.SetActive(false);

                            if (Plugin.IsColorRed)
                            {
                                ReflectionUtil.SetPrivateField(_playerController, "_rightSaber", targetSaber);
                                ReflectionUtil.SetPrivateField(_playerController, "_leftSaber", newSaber);
                            }
                            else
                            {
                                ReflectionUtil.SetPrivateField(_playerController, "_leftSaber", targetSaber);
                                ReflectionUtil.SetPrivateField(_playerController, "_rightSaber", newSaber);
                            }

                            _playerController.leftSaber.gameObject.SetActive(IsLeftSaberOn);

                            if (_beatmapObjectSpawnController != null)
                            {
                                _beatmapObjectSpawnController.noteWasCutEvent += this.HandleNoteWasCutEvent;
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "\n" + ex.StackTrace);
            }
        }

        public virtual void HandleNoteWasCutEvent(BeatmapObjectSpawnController noteSpawnController, NoteController noteController, NoteCutInfo noteCutInfo)
        {
            try
            {
                Vector3 notePos = noteController.noteTransform.position;

                Vector3 leftPos = _playerController.leftSaber.transform.position;
                leftPos += _playerController.leftSaber.transform.forward * 0.5f;
                Vector3 rightPos = _playerController.rightSaber.transform.position;
                rightPos += _playerController.rightSaber.transform.forward * 0.5f;

                float leftDist = Vector3.Distance(leftPos, notePos);
                float rightDist = Vector3.Distance(rightPos, notePos);

                _mainSettingsModel.controllersRumbleEnabled = true;
                Saber.SaberType targetType = (leftDist > rightDist) ? Saber.SaberType.SaberB : Saber.SaberType.SaberA;
                _noteCutHapticEffect.RumbleController(targetType);
                _mainSettingsModel.controllersRumbleEnabled = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "\n" + ex.StackTrace);
            }
        }

        public static BeatmapData CreateTransformedBeatmapData(BeatmapData beatmapData, GameplayOptions gameplayOptions, GameplayMode gameplayMode)
        {
            BeatmapData beatmapData2 = beatmapData;
            if (gameplayOptions.mirror)
            {
                beatmapData2 = BeatDataMirrorTransform.CreateTransformedData(beatmapData2);
            }
            if (gameplayMode == GameplayMode.SoloNoArrows)
            {
                beatmapData2 = BeatmapDataNoArrowsTransform.CreateTransformedData(beatmapData2);
            }
            if (gameplayOptions.obstaclesOption != GameplayOptions.ObstaclesOption.All)
            {
                beatmapData2 = BeatmapDataObstaclesTransform.CreateTransformedData(beatmapData2, gameplayOptions.obstaclesOption);
            }
            if (beatmapData2 == beatmapData)
            {
                beatmapData2 = beatmapData.GetCopy();
            }
            if (gameplayOptions.staticLights)
            {
                BeatmapEventData[] beatmapEventData = new BeatmapEventData[]
                {
                new BeatmapEventData(0f, BeatmapEventType.Event0, 1),
                new BeatmapEventData(0f, BeatmapEventType.Event4, 1)
                };
                beatmapData2 = new BeatmapData(beatmapData2.beatmapLinesData, beatmapEventData);
            }
            return beatmapData2;
        }
    }
}
