using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

[CanEditMultipleObjects, CustomEditor(typeof(Timer))]
public class TimerEditor : Editor
{
    //Add time in seconds to editor to preview*************************************
    public SerializedProperty
        days_Prop,
        hours_Prop,
        minutes_Prop,
        seconds_Prop,
        textType_Prop,
        standardText_Prop,
        textMeshPro_Prop,
        standardSlider_Prop,
        dialSlider_Prop,
        daysDisplay_Prop,
        hoursDisplay_Prop,
        minutesDisplay_Prop,
        secondsDisplay_Prop,
        millisecondsDisplay_Prop,
        adaptiveDisplay_Prop,
        removeLeadingZero_Prop,
        percentageDisplay_Prop,
        countMethod_Prop,
        seperatorType_Prop,
        startAtRuntime_Prop,
        useAsClock_Prop,
        timerSpeed_Prop,
        timeRemaining_Prop,
        repeat_Prop,
        onTimerEnd_Prop,
        onTimerStart_Prop;

    static bool showTimeToSetInfo = true;
    static bool showDisplayInfo = false;
    static bool showCustomInfo = false;
    static bool useAsClockLock = false;
    static bool percentageLock = true;
    void OnEnable()
    {
        // Setup the SerializedProperties
        days_Prop = serializedObject.FindProperty("days");
        hours_Prop = serializedObject.FindProperty("hours");
        minutes_Prop = serializedObject.FindProperty("minutes");
        seconds_Prop = serializedObject.FindProperty("seconds");
        textType_Prop = serializedObject.FindProperty("outputType");
        standardText_Prop = serializedObject.FindProperty("standardText");
        textMeshPro_Prop = serializedObject.FindProperty("textMeshProText");
        standardSlider_Prop = serializedObject.FindProperty("standardSlider");
        dialSlider_Prop = serializedObject.FindProperty("dialSlider");
        daysDisplay_Prop = serializedObject.FindProperty("daysDisplay");
        hoursDisplay_Prop = serializedObject.FindProperty("hoursDisplay");
        minutesDisplay_Prop = serializedObject.FindProperty("minutesDisplay");
        secondsDisplay_Prop = serializedObject.FindProperty("secondsDisplay");
        millisecondsDisplay_Prop = serializedObject.FindProperty("millisecondsDisplay");
        adaptiveDisplay_Prop = serializedObject.FindProperty("adaptiveDisplay");
        removeLeadingZero_Prop = serializedObject.FindProperty("removeLeadingZeros");
        percentageDisplay_Prop = serializedObject.FindProperty("percentageDisplay");
        countMethod_Prop = serializedObject.FindProperty("countMethod");
        seperatorType_Prop = serializedObject.FindProperty("seperatorType");
        startAtRuntime_Prop = serializedObject.FindProperty("startAtRuntime");
        useAsClock_Prop = serializedObject.FindProperty("useAsClock");
        timerSpeed_Prop = serializedObject.FindProperty("timerSpeed");
        timeRemaining_Prop = serializedObject.FindProperty("timeRemaining");
        repeat_Prop = serializedObject.FindProperty("repeat");
        onTimerEnd_Prop = serializedObject.FindProperty("onTimerEnd");
        onTimerStart_Prop = serializedObject.FindProperty("onTimerStart");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        

        showTimeToSetInfo = EditorGUILayout.BeginFoldoutHeaderGroup(showTimeToSetInfo, "Set time");
        if(showTimeToSetInfo)
        {
            EditorGUILayout.HelpBox("Add the days, hours, minutes and seconds to count. \n" +
                "If counting up it will start at 0 and if counting down it will start at this time. \n" +
                "You can also drop any object in the event box to run any function when the timer ends.", MessageType.None);
            EditorGUILayout.PropertyField(days_Prop);
            EditorGUILayout.PropertyField(hours_Prop);
            EditorGUILayout.PropertyField(minutes_Prop);
            EditorGUILayout.PropertyField(seconds_Prop);
            GUI.enabled = false;
            EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Time in seconds remaining");
                EditorGUILayout.TextField(timeRemaining_Prop.doubleValue.ToString("F0"));
            EditorGUILayout.EndHorizontal();
            GUI.enabled = true;
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(onTimerStart_Prop);
            EditorGUILayout.PropertyField(onTimerEnd_Prop);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        
        EditorGUILayout.Space();

        showDisplayInfo = EditorGUILayout.BeginFoldoutHeaderGroup(showDisplayInfo, "Display");
        if(showDisplayInfo)
        {
            EditorGUILayout.HelpBox("Options for displaying the set time, including how to change the seperator and linking text objects", MessageType.None);

            using (new EditorGUI.DisabledScope(percentageLock))
            {
                if (useAsClockLock)
                {
                    GUI.enabled = false;
                    percentageLock = EditorGUILayout.PropertyField(daysDisplay_Prop);
                    GUI.enabled = true;
                }
                else
                {
                    GUI.enabled = true;
                    EditorGUILayout.PropertyField(daysDisplay_Prop);
                }
                EditorGUILayout.PropertyField(hoursDisplay_Prop);
                EditorGUILayout.PropertyField(minutesDisplay_Prop);
                EditorGUILayout.PropertyField(secondsDisplay_Prop);
                EditorGUI.EndDisabledGroup();

                if (useAsClockLock)
                {
                    GUI.enabled = false;
                    EditorGUILayout.PropertyField(millisecondsDisplay_Prop);
                    GUI.enabled = true;
                }

                else
                {
                    GUI.enabled = true;
                    EditorGUILayout.PropertyField(millisecondsDisplay_Prop);
                }
                EditorGUILayout.PropertyField(adaptiveDisplay_Prop);
            }
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(removeLeadingZero_Prop);
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(percentageDisplay_Prop);
            if(percentageDisplay_Prop.boolValue)
            {
                percentageLock = true;
                daysDisplay_Prop.boolValue = false;
                millisecondsDisplay_Prop.boolValue = false;
                hoursDisplay_Prop.boolValue = false;
                minutesDisplay_Prop.boolValue = false;
                secondsDisplay_Prop.boolValue = false;
            }
            else
            {
                percentageLock = false;
            }
            
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(seperatorType_Prop);

            EditorGUILayout.PropertyField(textType_Prop);

            Timer.OutputType tt = (Timer.OutputType)textType_Prop.enumValueIndex;

            switch (tt)
            {
                case Timer.OutputType.StandardText:

                    EditorGUILayout.ObjectField(standardText_Prop);
                    break;

                case Timer.OutputType.TMPro:

                    EditorGUILayout.ObjectField(textMeshPro_Prop);
                    break;

                case Timer.OutputType.HorizontalSlider:

                    EditorGUILayout.PropertyField(standardSlider_Prop);
                    EditorGUILayout.HelpBox("Use any custom slider as long as it uses the 'Slider' component", MessageType.Info);
                    break;

                case Timer.OutputType.Dial:

                    EditorGUILayout.PropertyField(dialSlider_Prop);
                    EditorGUILayout.HelpBox("Use any image with the type set to 'Filled' and the 'Fill Origin' set to top", MessageType.Info);
                    break;
                default:
                    break;
            }
            
            
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        EditorGUILayout.Space();

        showCustomInfo = EditorGUILayout.BeginFoldoutHeaderGroup(showCustomInfo, "Customise");
        if(showCustomInfo)
        {
            EditorGUILayout.HelpBox("General settings for the timer", MessageType.None);
            EditorGUILayout.PropertyField(countMethod_Prop);
            EditorGUILayout.PropertyField(repeat_Prop);
            EditorGUILayout.PropertyField(timerSpeed_Prop);
            EditorGUILayout.PropertyField(startAtRuntime_Prop);
            EditorGUILayout.PropertyField(useAsClock_Prop);
            if(useAsClock_Prop.boolValue)
            {
                percentageDisplay_Prop.boolValue = false;
                useAsClockLock = true;
                countMethod_Prop.enumValueIndex = 1;

                daysDisplay_Prop.boolValue = false;
                millisecondsDisplay_Prop.boolValue = false;
                hoursDisplay_Prop.boolValue = true;
                minutesDisplay_Prop.boolValue = true;
                secondsDisplay_Prop.boolValue = true;
            }
            else
            {
                useAsClockLock = false;
            }
            EditorGUILayout.HelpBox("This will use your current system time as the time to set", MessageType.Info);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        
        serializedObject.ApplyModifiedProperties();
    }
    

}
