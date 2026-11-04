using Common.Network.Model.Http;
using LetsPiu.Common.Component;
using LetsPiu.Common.Network.Model.Http.Play;
using LetsPiu.Component;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LetsPiu.Common.UI
{
    public class CommonDialogScrollRect : MonoBehaviour
    {
        public ScrollView scrollView;

        public List<Player> players=new List<Player>();

        private void Awake()
        {
            scrollView.onCellUpdate += ScrollView_onCellUpdate;
        }

        private void ScrollView_onCellUpdate(Transform root, int index)
        {
            var player = players[index];
            var item = root.GetComponentInChildren<PlayerAvatarDataUI>();
            var customer = new Customer();
            customer.imageUrl = player.imageUrl;
            customer.sex = player.sex;
            if (player != null&&item!= null)
            {
                item.UpdateHeadTexture(customer);
            }
        }

        public void UpdataData(List<Player> players)
        {
            Debug.Log(players.Count + "玩家数量");
            this.players = players;
            scrollView.Refresh(players.Count,false);
        }

        // 开始
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
