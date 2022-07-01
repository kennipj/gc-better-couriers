using HarmonyLib;
using System.Reflection;
using TMPro;
using U3D.Threading.Tasks;
using UnityEngine;

namespace BetterCouriers
{
    public static class Utils
    {
        public static T GetField<T>(this object obj, string field)
        {
            return Traverse.Create(obj).Field(field).GetValue<T>();
        }

        public static void SetField(this object obj, string field, object value)
        {
            Traverse.Create(obj).Field(field).SetValue(value);
        }

        public static MethodInfo GetPrivateMethod<T>(this T obj, string name)
        {
            return obj.GetType().GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public static void InvokePrivateMethod<T>(this T obj, string name, object[] args)
        {
            MethodInfo method = obj.GetType()
                .GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance);
            method.Invoke(obj, args);
        }

        public static void InvokePrivateMethod<T>(this T obj, string name)
        {
            MethodInfo method = obj.GetType()
                .GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance);
            method.Invoke(obj, null);
        }

        public static T2 InvokePrivateMethod<T, T2>(this T obj, string name, object[] args)
        {
            MethodInfo method = obj.GetType()
                .GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance);
            return (T2)method.Invoke(obj, args);
        }

        public static T2 InvokePrivateMethod<T, T2>(this T obj, string name)
        {
            MethodInfo method = obj.GetType()
                .GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance);
            return (T2)method.Invoke(obj, null);
        }

        public static void DefferedSetText(this TextMeshProUGUI textMesh, string text)
        {
            Canvas.WillRenderCanvases setText = () => textMesh.text = text;
            Canvas.willRenderCanvases += setText;
            Task.Run(
                () =>
                {
                    Task.Delay(50);
                    Canvas.willRenderCanvases -= setText;
                }
            );
        }
    }
}
