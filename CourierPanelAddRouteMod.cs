using GoodCompany;
using GoodCompany.GUI;
using UnityEngine;
using HarmonyLib;
using System;
using System.Linq;
using UnityEngine.UI;
using TMPro;

namespace BetterCouriers
{
    public static class CourierPanelAddRouteMod
    {
        public static void AddRouteButton(CourierPalletPanel panel)
        {
            var session = panel.GetField<SessionManager>("_session");
            GameObject configure = panel.gameObject.transform.GetChild(2).gameObject;
            GameObject configure_button = configure.transform.GetChild(0).gameObject;
            configure_button.transform.localPosition = new Vector3(-74, 0, 0);

            GameObject add_route = GameObject.Instantiate(configure_button);
            add_route.transform.parent = configure.transform;
            add_route.transform.localPosition = new Vector3(130, 0, 0);

            Image img = add_route.GetComponent<Image>();
            img.rectTransform.sizeDelta = new Vector2(140, img.rectTransform.sizeDelta.y);

            GUIButton button = add_route.GetComponent<GUIButton>();
            button.OnClick.AddListener(() => CreateRouteWithItems(panel, session));

            GameObject label = add_route.transform.GetChild(0).gameObject;
            label.transform.DestroyAllChildren();
            label.transform.localPosition = new Vector3(-16, 0, 0);

            TextMeshProUGUI text = label.GetComponent<TextMeshProUGUI>();
            text.text = "Add route";
            text.DefferedSetText("Add route");
        }

        private static void CreateRouteWithItems(CourierPalletPanel panel, SessionManager session)
        {
            var palletControl = panel.GetField<CourierPalletPanelControl>("_control");
            var itemRules = panel.GetField<GUIList<ItemRuleListItem>>("_rulesList");
            uint palletId = palletControl.GetField<CourierPalletModel>("_currentPallet").ID;

            ItemType[] itemTypes = itemRules
                .ToArray()
                .Select(ruleListItem => ruleListItem.ItemType)
                .ToArray()
                .SliceArray(0, 5);

            var routesControl = OpenRouteOverview(session);

            var reqId = session.MsgSender.Requests.SendAddRouteRequest(
                new ResponseReceiver.Receiver(
                    (ushort requestId, byte[] content, int offset) =>
                    {
                        uint routeId = BitConverter.ToUInt32(content, content.Length - 4);

                        routesControl.InvokePrivateMethod(
                            "OnAddRouteRequestResponse",
                            new object[] { requestId, content, offset }
                        );

                        routesControl.AddPalletToRoute(routeId, palletId);
                        routesControl.AddItemsToRoute(routeId, itemTypes);
                    }
                )
            );
            routesControl.SetField("_lastAddRouteRequest", reqId);
        }

        private static CourierRoutesPanelControl OpenRouteOverview(SessionManager session)
        {
            var routesPanel = session.WindowManager.OpenFullscreenPanel(
                session.GUIElements.Panels.Get<CourierRoutesPanel>(0)
            );
            return new CourierRoutesPanelControl(routesPanel, session);
        }

        private static void AddPalletToRoute(
            this CourierRoutesPanelControl control,
            uint routeId,
            uint palletId
        )
        {
            control.InvokePrivateMethod(
                "GoodCompany.GUI.CourierRoutesPanel.IControl.AddPalletToRoute",
                new object[] { routeId, palletId }
            );
        }

        private static void AddItemsToRoute(
            this CourierRoutesPanelControl control,
            uint routeId,
            ItemType[] itemTypes
        )
        {
            var addItemMethod = control.GetPrivateMethod(
                "GoodCompany.GUI.CourierRoutesPanel.IControl.AddItemToRoute"
            );
            itemTypes.Do(
                itemType => addItemMethod.Invoke(control, new object[] { routeId, itemType })
            );
        }
    }

    [HarmonyPatch(typeof(CourierPalletPanel), "Initialize")]
    static class CourierPalletPanel_Initialize_Patch
    {
        static void Postfix(ref CourierPalletPanel __instance)
        {
            CourierPanelAddRouteMod.AddRouteButton(__instance);
        }
    }
}
