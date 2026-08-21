using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

    public class EventCenter
    {

        public static Dictionary<Eventdefine, Delegate> Table = new Dictionary<Eventdefine, Delegate>();

        //添加监听

        public static void addlistener(Eventdefine eventtype, Callback callback)
        {

            if (!Table.ContainsKey(eventtype))
            {

                Table.Add(eventtype, null);
            }
            if (Table[eventtype] != null && Table[eventtype].GetType() != callback.GetType())
            {

                throw new Exception(string.Format("事件类型{0},对应的委托类型{1},要添加的委托类型{2}", eventtype,
                    Table[eventtype].GetType(), callback.GetType()));
            }

            Table[eventtype] = (Callback)Table[eventtype] + callback;


        }
        //一个参数
        public static void addlistener<T>(Eventdefine eventtype, Callback<T> callback)
        {

            if (!Table.ContainsKey(eventtype))
            {

                Table.Add(eventtype, null);
            }
            if (Table[eventtype] != null && Table[eventtype].GetType() != callback.GetType())
            {

                throw new Exception(string.Format("事件类型{0},对应的委托类型{1},要添加的委托类型{2}", eventtype,
                    Table[eventtype].GetType(), callback.GetType()));
            }

            Table[eventtype] = (Callback<T>)Table[eventtype] + callback;


        }
        //public static string addlistenerPar<T>(Eventdefine eventtype, Callback<T> callback)
        //{

        //    if (!Table.ContainsKey(eventtype))
        //    {

        //        Table.Add(eventtype, null);
        //    }
        //    if (Table[eventtype] != null && Table[eventtype].GetType() != callback.GetType())
        //    {

        //        throw new Exception(string.Format("事件类型{0},对应的委托类型{1},要添加的委托类型{2}", eventtype,
        //            Table[eventtype].GetType(), callback.GetType()));
        //    }

        //    Table[eventtype] = (Callback<T>)Table[eventtype] + callback;


        //}
        //两个参数
        public static void addlistener<T1, T2>(Eventdefine eventtype, Callback<T1, T2> callback)
        {

            if (!Table.ContainsKey(eventtype))
            {

                Table.Add(eventtype, null);
            }
            if (Table[eventtype] != null && Table[eventtype].GetType() != callback.GetType())
            {

                throw new Exception(string.Format("事件类型{0},对应的委托类型{1},要添加的委托类型{2}", eventtype,
                    Table[eventtype].GetType(), callback.GetType()));
            }

            Table[eventtype] = (Callback<T1, T2>)Table[eventtype] + callback;


        }

        //添加返回值为int函数的监听
        public static void AddlistenerString<T3>(Eventdefine eventtype, CallbackString<T3> callback)
        {


            if (!Table.ContainsKey(eventtype))
            {

                Table.Add(eventtype, null);
            }

            if (Table[eventtype] != null && Table[eventtype].GetType() != callback.GetType())
            {

                throw new Exception(string.Format("事件类型{0},对应的委托类型{1},要添加的委托类型{2}", eventtype,
                    Table[eventtype].GetType(), callback.GetType()));
            }

            Table[eventtype] = (CallbackString<T3>)Table[eventtype] + callback;

        }
        public static void AddlistenerBool<T>(Eventdefine eventtype, CallbackBool<T> callback)
        {


            if (!Table.ContainsKey(eventtype))
            {

                Table.Add(eventtype, null);
            }

            if (Table[eventtype] != null && Table[eventtype].GetType() != callback.GetType())
            {

                throw new Exception(string.Format("事件类型{0},对应的委托类型{1},要添加的委托类型{2}", eventtype,
                    Table[eventtype].GetType(), callback.GetType()));
            }

            Table[eventtype] = (CallbackBool<T>)Table[eventtype] + callback;

        }
        //移除监听

        public static void RemoveListener(Eventdefine eventtype, Callback callback)
        {
            if (!Table.ContainsKey(eventtype))
            {

                Log.Debug("要移除的监听不存在");
            }
            if (Table[eventtype] != null && Table[eventtype].GetType() != callback.GetType())
            {
                Log.Debug("要移除的监听类型与字典中存放的监听类型不一致");

            }
            Table[eventtype] = (Callback)Table[eventtype] - callback;
             Log.Debug("移除监听");


        }
        public static void RemoveListenerBool<T>(Eventdefine eventtype, CallbackBool<T> callback)
        {
            if (!Table.ContainsKey(eventtype))
            {

                throw new Exception("要移除的监听不存在");
            }
            if (Table[eventtype] != null && Table[eventtype].GetType() != callback.GetType())
            {
                throw new Exception("要移除的监听类型与字典中存放的监听类型不一致");

            }
            Table[eventtype] = (CallbackBool<T>)Table[eventtype] - callback;
             Log.Debug("移除监听");

        }

        public static void RemoveListener<T>(Eventdefine eventtype, Callback<T> callback)
        {
            if (!Table.ContainsKey(eventtype))
            {

                throw new Exception("要移除的监听不存在");
            }
            if (Table[eventtype] != null && Table[eventtype].GetType() != callback.GetType())
            {
                throw new Exception("要移除的监听类型与字典中存放的监听类型不一致");

            }
            //for (int i = 0; i < Table.Count; i++)
            //{
            Table[eventtype] = (Callback<T>)Table[eventtype] - callback;
            //}




        }
        public static void RemoveListenerTwo<T1, T2>(Eventdefine eventtype, Callback<T1, T2> callback)
        {
            if (!Table.ContainsKey(eventtype))
            {

                throw new Exception("要移除的监听不存在");
            }
            if (Table[eventtype] != null && Table[eventtype].GetType() != callback.GetType())
            {
                throw new Exception("要移除的监听类型与字典中存放的监听类型不一致");

            }

            Table[eventtype] = (Callback<T1, T2>)Table[eventtype] - callback;


        }
        //移除返回值为int的监听
        public static void RemoveListenerString<T3>(Eventdefine eventtype, CallbackString<T3> callback)
        {
            if (!Table.ContainsKey(eventtype))
            {

                throw new Exception("要移除的监听不存在");
            }
            if (Table[eventtype] != null && Table[eventtype].GetType() != callback.GetType())
            {
                throw new Exception("要移除的监听类型与字典中存放的监听类型不一致");

            }
            Table[eventtype] = (CallbackString<T3>)Table[eventtype] - callback;


        }


        //广播监听
        public static void BroadCast(Eventdefine eventtype)
        {
            Delegate d;
            if (Table.TryGetValue(eventtype, out d))
            {
                if (d == null)
                    throw new Exception("字典中委托为空，广播失败");
                Callback callback = (Callback)d;
                callback();
            }


        }
        public static void BroadCast<T>(Eventdefine eventtype, T arg)
        {
            Delegate d;
            if (Table.TryGetValue(eventtype, out d))
            {
                if (d == null)
                    throw new Exception("字典中委托为空，广播失败");
                Callback<T> callback = (Callback<T>)d;
                callback(arg);
            }


        }

        public static void BroadCast<T1, T2>(Eventdefine eventtype, T1 arg, T2 arg2)
        {
            Delegate d;
            if (Table.TryGetValue(eventtype, out d))
            {
                if (d == null)
                    throw new Exception("字典中委托为空，广播失败");
                Callback<T1, T2> callback = (Callback<T1, T2>)d;
                callback(arg, arg2);
            }


        }
        //广播返回值为Int的监听
        public static string BroadCastString<T3>(Eventdefine eventtype, T3 arg)
        {
            Delegate d;
            string str = string.Empty;
            if (Table.TryGetValue(eventtype, out d))
            {
                if (d == null)
                    throw new Exception("字典中委托为空，广播失败");
                CallbackString<T3> callback = (CallbackString<T3>)d;
                str = callback(arg);
            }

            return str;
        }
        public static void BroadCastBool<T>(Eventdefine eventtype, T arg)
        {
            Delegate d;
            if (Table.TryGetValue(eventtype, out d))
            {
                if (d == null)
                    throw new Exception("字典中委托为空，广播失败");
                CallbackBool<T> callback = (CallbackBool<T>)d;
                callback(arg);
            }


        }

    }

