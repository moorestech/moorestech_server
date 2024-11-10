﻿using System.Threading;
using Client.Game.InGame.BlockSystem.PlaceSystem;
using Client.Game.InGame.Control;
using Client.Game.InGame.UI.UIState.Input;
using Client.Game.Skit;
using Client.Input;
using UnityEngine;

namespace Client.Game.InGame.UI.UIState
{
    public class PlaceBlockState : IUIState
    {
        private readonly IBlockPlacePreview _blockPlacePreview;
        private readonly ScreenClickableCameraController _screenClickableCameraController;
        private readonly SkitManager _skitManager;
        
        private Vector3 _startCameraRotation;
        private float _startCameraDistance;
        
        public PlaceBlockState(IBlockPlacePreview blockPlacePreview, SkitManager skitManager, InGameCameraController inGameCameraController)
        {
            _skitManager = skitManager;
            _blockPlacePreview = blockPlacePreview;
            _screenClickableCameraController = new ScreenClickableCameraController(inGameCameraController);
        }
        
        public void OnEnter(UIStateEnum lastStateEnum)
        {
            _screenClickableCameraController.OnEnter();
            _screenClickableCameraController.StartTween();
        }
        
        public UIStateEnum GetNext()
        {
            if (InputManager.UI.OpenInventory.GetKeyDown) return UIStateEnum.PlayerInventory;
            if (BlockClickDetect.IsClickOpenableBlock(_blockPlacePreview)) return UIStateEnum.BlockInventory;
            if (InputManager.UI.BlockDelete.GetKeyDown) return UIStateEnum.DeleteBar;
            if (_skitManager.IsPlayingSkit) return UIStateEnum.Story;
            //TODO InputSystemのリファクタ対象
            if (InputManager.UI.CloseUI.GetKeyDown || UnityEngine.Input.GetKeyDown(KeyCode.B)) return UIStateEnum.GameScreen;
            
            _screenClickableCameraController.GetNextUpdate();
            
            return UIStateEnum.Current;
        }
        
        public void OnExit()
        {
            _screenClickableCameraController.OnExit();
        }
    }
}