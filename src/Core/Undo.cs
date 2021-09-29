
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ZargoEngine
{
    public static unsafe class Undo
    {
        private static readonly RoundStack<Action> UndoStack = new RoundStack<Action>(100);
        private static readonly RoundStack<Action> RedoStack = new RoundStack<Action>(100);

        public static void AddUndoRedo(Action unDo, Action redo)
        {
            UndoStack.Push(unDo);
            RedoStack.Push(redo);
        }

        public static void AddActions(Action Do, Action redo)
        {
            Do();
            RedoStack.Push(Do);
            RedoStack.Push(redo);
        }

        private static readonly Dictionary<IntPtr, bool> RefDatas = new Dictionary<IntPtr, bool>();
        public static void Record<T>(ref T value, bool editing) where T : unmanaged
        {
            var key = (IntPtr)Unsafe.AsPointer(ref value);
            bool firstTime = false; // for debug

            if (!RefDatas.ContainsKey(key)) {
                firstTime = true;
                RefDatas.Add(key, false);
            }

            if (RefDatas.TryGetValue(key, out bool edit))
            {
                if (!edit && editing)
                {
                    Record(ref value);
                }
                RefDatas[key] = editing;
            }
            else if (!firstTime)
            {
                Debug.LogWarning("pointer disappeared undo record, probably GC moved it !");
            }
        }

        public static void Record<T>(ref T value) where T : unmanaged
        {
            var data = new RecordData<T>(ref value);
            UndoStack.Push(data.Undo);
            RedoStack.Push(data.Redo);
        }

        // bool is editing object is old value
        private static readonly Dictionary<(FieldInfo, object), bool> FieldDatas = new Dictionary<(FieldInfo, object), bool>();
        public static void RecordField(FieldInfo info, object @object, bool editing) 
        {
            var key = (info, @object);
            if (!FieldDatas.ContainsKey(key)) FieldDatas.Add(key, false);

            // mouse released
            if (FieldDatas[key] == false && editing == true) {
                object oldValue = info.GetValue(@object);
                UndoStack.Push(() =>
                {
                    object beforeUndo = info.GetValue(@object);
                    info.SetValue(@object, oldValue);
                    RedoStack.Push(() => info.SetValue(@object, beforeUndo));
                });
            }
            FieldDatas[key] = editing;
        }

        #pragma warning disable IDE1006 // Naming Styles
        public static void undo()
        {
            if (UndoStack.TryPop(out var action))
                action.Invoke();
            else  Debug.LogWarning("undo Pool empty");
        }
        
        internal static void Redo()
        {
            if (RedoStack.TryPop(out var action))
                action.Invoke();
            else  Debug.LogWarning("redo pool empty");
        }

        private readonly struct RecordData<T> where T : unmanaged
        {
            readonly byte* location;
            readonly byte[] startValue;
            readonly byte[] undoValue;
            readonly ushort size;

            internal void Undo()
            {
                ushort i = 0;
                for (; i < size; i++) {
                    undoValue[i] = location[i];
                }
                for (i = 0; i < size; i++) {
                    location[i] = startValue[i];
                }
            }

            internal void Redo()
            {
                for (ushort i = 0; i < size; i++) {
                    location[i] = undoValue[i];
                }
            }

            internal RecordData(ref T value) 
            {
                location = (byte*)Unsafe.AsPointer(ref value);
                size = (ushort)Unsafe.SizeOf<T>();
                undoValue = new byte[size];
                startValue = value.GetBytes();
            }
        }
    }
}