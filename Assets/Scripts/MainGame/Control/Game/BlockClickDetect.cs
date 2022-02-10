using MainGame.Network.Send;
using MainGame.UnityView.Chunk;
using MainGame.UnityView.UI.Inventory.View;
using UnityEngine;
using VContainer;

namespace MainGame.Control.Game
{
    public class BlockClickDetect : MonoBehaviour,IBlockClickDetect
    {
        private Camera _mainCamera;
        private MoorestechInputSettings _input;
        private RequestBlockInventoryProtocol _requestBlockInventoryProtocol;
        
        [Inject]
        public void Construct(Camera mainCamera,RequestBlockInventoryProtocol requestBlockInventoryProtocol)
        {
            _requestBlockInventoryProtocol = requestBlockInventoryProtocol;
            _mainCamera = mainCamera;
            _input = new MoorestechInputSettings();
            _input.Enable();
        }

        public void OnInput()
        {
        }

        public void OffInput()
        {
            
        }
        
        public bool IsBlockClicked()
        {
            
            
            var mousePosition = _input.Playable.ClickPosition.ReadValue<Vector2>();
            var ray = _mainCamera.ScreenPointToRay(mousePosition);
            
            
            // マウスでクリックした位置が地面なら
            if (!Physics.Raycast(ray, out var hit)) return false;
            if (hit.collider.gameObject.GetComponent<BlockGameObject>() == null) return false;
            
            
            var x = Mathf.RoundToInt(hit.point.x);
            var y = Mathf.RoundToInt(hit.point.z);
            
            //その位置のブロックインベントリを取得するパケットを送信する
            //実際にインベントリのパケットを取得できてからUIを開くため、実際の開く処理はNetworkアセンブリで行う
            _requestBlockInventoryProtocol.Send(x,y);
            
            return true;
        }
    }
}