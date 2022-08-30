using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class DebugTools
{
    private static bool enabled = true;
    public static void Print(object str){
        if(enabled){
            Debug.Log(str);
        }
    }
    public static void PrintError(object str){
        if(enabled){
            Debug.LogError(str);
        }
    }
    public static void PrintAnnouncement(object str){
        if(enabled){
            Debug.Log($"<color=green>{str}</color>");
        }
    }
    public static void PrintIntArray(int[] arr, int rowLength=8){
        if(enabled){
            if(rowLength > arr.Length){
                rowLength = arr.Length;
            }
            string result = "";
            for (int i = 0; i < arr.Length; i++)
            {
                if(i % rowLength == 0){
                    result += "\n";
                }
                result += arr[i].ToString() + "  ";
            }
            Debug.Log(result);

        }
    }
    public static void PrintBoolArray(bool[] arr, int rowLength=8){
        if(enabled){
            if(rowLength > arr.Length){
                rowLength = arr.Length;
            }
            string result = "";
            for (int i = 0; i < arr.Length; i++)
            {
                if(i % rowLength == 0){
                    result += "\n";
                }
                result += arr[i].ToString() + "  ";
            }
            Debug.Log(result);
        }
    }
}
