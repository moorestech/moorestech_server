using System;
using MainGame.Basic.Server;
using UnityEngine;

namespace MainGame.UnityView.Entity
{
    public class ItemEntityObject : MonoBehaviour, IEntityObject
    {
        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private Material itemMaterial;

        private const float Interval = NetworkConst.UpdateIntervalSeconds;
        
        public void SetTexture(Texture texture)
        {
            var material = new Material(itemMaterial)
            {
                mainTexture = texture
            };
            meshRenderer.material = material;
        }
        
        
        
        private Vector3 _targetPosition;
        private float _linerTime;
        public void SetDirectPosition(Vector3 position)
        {
            transform.position = position;
        }

        public void SetInterpolationPosition(Vector3 position)
        {
            _targetPosition = position;
            _linerTime = 0;
        }

        //Linerでポジションを補完させる
        private void Update()
        {
            //補完する
            var rate = _linerTime / Interval;
            rate = Mathf.Clamp01(rate);
            transform.position = Vector3.Lerp(transform.position, _targetPosition, rate);
            _linerTime += Time.deltaTime;
        }

        public void Destroy()
        {
            Destroy(gameObject);
        }
    }
}