using System.Collections.Generic;
using System.Linq;
using Client.Common;
using System.Linq;
using Client.Game.InGame.Block;
using Client.Game.InGame.Context;
using Game.Block.Interface;
using Game.Block.Interface.BlockConfig;
using UnityEngine;

namespace Client.Game.InGame.BlockSystem
{
    public class BlockPlacePreview : MonoBehaviour, IBlockPlacePreview
    {
        private BlockPreviewObject _previewBlock;
        
        public bool IsActive => gameObject.activeSelf;
        
        public bool IsCollisionGround
        {
            get
            {
                if (_collisionDetectors == null) return false;
                
                return _collisionDetectors.Any(detector => detector.IsCollision);
            }
        }
        
        private GroundCollisionDetector[] _collisionDetectors;
        
        public void SetPlaceablePreview(Vector3Int blockPosition, BlockDirection blockDirection, BlockConfigData blockConfig)
        {
            SetPreview(blockPosition, blockDirection, blockConfig);
            SetMaterial(Resources.Load<Material>(MaterialConst.PreviewPlaceBlockMaterial));
        }
        
        public void SetNotPlaceablePreview(Vector3Int blockPosition, BlockDirection blockDirection, BlockConfigData blockConfig)
        {
            SetMaterial(Resources.Load<Material>(MaterialConst.PreviewNotPlaceableBlockMaterial));
            SetPreview(blockPosition, blockDirection, blockConfig);
        }
        
        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }
        
        private void SetPreview(Vector3Int blockPosition, BlockDirection blockDirection, BlockConfigData blockConfig)
        {
            var pos = SlopeBlockPlaceSystem.GetBlockPositionToPlacePosition(blockPosition, blockDirection, blockConfig.BlockId);
            var rot = blockDirection.GetRotation();
            
            if (!_previewBlock || _previewBlock.BlockConfig.BlockId != blockConfig.BlockId) //TODO さっきと同じブロックだったら置き換え
            {
                if (_previewBlock)
                    Destroy(_previewBlock.gameObject);
                
                //プレビューブロックを設置
                _previewBlock = MoorestechContext.BlockGameObjectContainer.CreatePreviewBlock(blockConfig.BlockId);
                _previewBlock.transform.SetParent(transform);
                _previewBlock.transform.localPosition = Vector3.zero;
                _collisionDetectors = _previewBlock.GetComponentsInChildren<GroundCollisionDetector>();
            }
            
            transform.position = pos;
            _previewBlock.transform.rotation = rot;
        }
        
        public void SetMaterial(Material material)
        {
            //プレビューブロックのマテリアルを変更
            _previewBlock.SetMaterial(material);
        }
    }
}