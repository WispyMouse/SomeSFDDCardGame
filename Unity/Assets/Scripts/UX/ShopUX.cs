namespace SFDDCards.UX
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.EventSystems;

    public class ShopUX : MonoBehaviour
    {
        [SerializeReference]
        private ShopItemUX ShopItemUXPF;
        [SerializeReference]
        private Transform ShopItemUXHolderTransform;

        [SerializeReference]
        private CentralGameStateController CentralGameStateControllerInstance;

        private List<ShopItemUX> ActiveShopItemUXs { get; set; } = new List<ShopItemUX>();

        private void Awake()
        {
            this.DestroyItems();
        }

        void DestroyItems()
        {
            for (int ii = ShopItemUXHolderTransform.childCount - 1; ii >= 0; ii--)
            {
                Destroy(ShopItemUXHolderTransform.GetChild(ii).gameObject);
            }

            this.ActiveShopItemUXs.Clear();
        }

        public void ShopItemSelected(ShopItemUX selectedItem)
        {
            if (!this.CentralGameStateControllerInstance.CurrentCampaignContext.CanAfford(selectedItem.RepresentingEntry.Costs))
            {
                GlobalUpdateUX.LogTextEvent.Invoke($"Cannot afford this item!", GlobalUpdateUX.LogType.Info);
                return;
            }

            this.CentralGameStateControllerInstance.CurrentCampaignContext.PurchaseShopItem(selectedItem.RepresentingEntry);
            this.ActiveShopItemUXs.Remove(selectedItem);
            Destroy(selectedItem.gameObject);
        }

        public void SetShopItems(params ShopEntry[] shopEntries)
        {
            this.DestroyItems();

            foreach (ShopEntry curEntry in shopEntries)
            {
                ShopItemUX shopEntry = Instantiate(this.ShopItemUXPF, this.ShopItemUXHolderTransform);
                shopEntry.SetFromEntry(this.CentralGameStateControllerInstance.CurrentCampaignContext, curEntry, ShopItemSelected);
                this.ActiveShopItemUXs.Add(shopEntry);
            }
        }

    }
}